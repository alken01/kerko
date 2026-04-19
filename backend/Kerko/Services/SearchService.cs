using System.Linq.Expressions;
using System.Text;
using Kerko.Infrastructure;
using Kerko.Models;
using Microsoft.EntityFrameworkCore;

namespace Kerko.Services;

public interface ISearchService
{
    Task<SearchResponse> KerkoAsync(string? mbiemri, string? emri, int pageNumber = 1, int pageSize = 10);
    Task<PaginatedResult<TargatResponse>> TargatAsync(string? numriTarges, int pageNumber = 1, int pageSize = 10);
    Task<PaginatedResult<PatronazhistResponse>> TelefonAsync(string? numriTelefonit, int pageNumber = 1, int pageSize = 10);
}

public class SearchService(ApplicationDbContext db) : ISearchService
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;
    private const int MinTargesLength = 6;
    private const int MinPhoneLength = 10;
    private const int MaxInputLength = 100;

    // Sentinel char appended to a prefix to form an inclusive upper bound for a
    // range scan: any string starting with `prefix` is <= `prefix + \uFFFF`.
    private const char RangeUpperSentinel = '\uFFFF';

    // EF Core translates string.Compare(col, const) op 0 to col op const in SQL.
    private static readonly System.Reflection.MethodInfo StringCompareMethod = typeof(string).GetMethod(
        nameof(string.Compare), [typeof(string), typeof(string)])!;

    public async Task<SearchResponse> KerkoAsync(string? mbiemri, string? emri, int pageNumber = 1, int pageSize = DefaultPageSize)
    {
        ValidateBothNames(mbiemri, emri);
        (pageNumber, pageSize) = ClampPagination(pageNumber, pageSize);

        var normalizedEmri = NormalizeAlbanian(emri);
        var normalizedMbiemri = NormalizeAlbanian(mbiemri);

        if (normalizedEmri.Length == 0 && normalizedMbiemri.Length == 0)
        {
            return EmptySearchResponse(pageNumber, pageSize);
        }

        // Sequential — DbContext is not thread-safe and SQLite serializes
        // access anyway, so parallelism would buy nothing.
        var person = await PrefixSearchAsync(db.Person, p => p.EmerNormalized, p => p.MbiemerNormalized,
            SearchProjections.Person, normalizedEmri, normalizedMbiemri, pageNumber, pageSize);
        var rrogat = await PrefixSearchAsync(db.Rrogat, r => r.EmriNormalized, r => r.MbiemriNormalized,
            SearchProjections.Rrogat, normalizedEmri, normalizedMbiemri, pageNumber, pageSize);
        var targat = await PrefixSearchAsync(db.Targat, t => t.EmriNormalized, t => t.MbiemriNormalized,
            SearchProjections.Targat, normalizedEmri, normalizedMbiemri, pageNumber, pageSize);
        var patronazhist = await PrefixSearchAsync(db.Patronazhist, p => p.EmriNormalized, p => p.MbiemriNormalized,
            SearchProjections.Patronazhist, normalizedEmri, normalizedMbiemri, pageNumber, pageSize);

        return new SearchResponse
        {
            Person = person,
            Rrogat = rrogat,
            Targat = targat,
            Patronazhist = patronazhist
        };
    }

    public async Task<PaginatedResult<TargatResponse>> TargatAsync(string? numriTarges, int pageNumber = 1, int pageSize = DefaultPageSize)
    {
        ValidateSingleField(numriTarges, "Numri i targes", MinTargesLength);
        (pageNumber, pageSize) = ClampPagination(pageNumber, pageSize);

        var needle = numriTarges!.ToLower().Trim();
        var query = db.Targat
            .AsNoTracking()
            .Where(t => t.NumriTarges != null && t.NumriTarges.ToLower().Contains(needle))
            .OrderBy(t => t.NumriTarges!.ToLower() == needle ? 0
                        : t.NumriTarges!.ToLower().StartsWith(needle) ? 1 : 2)
            .ThenBy(t => t.NumriTarges);

        return await PaginateAsync(query, SearchProjections.Targat, pageNumber, pageSize);
    }

    public async Task<PaginatedResult<PatronazhistResponse>> TelefonAsync(string? numriTelefonit, int pageNumber = 1, int pageSize = DefaultPageSize)
    {
        ValidateSingleField(numriTelefonit, "Numri i telefonit", MinPhoneLength);
        (pageNumber, pageSize) = ClampPagination(pageNumber, pageSize);

        var needle = numriTelefonit!.Trim();
        var query = db.Patronazhist
            .AsNoTracking()
            .Where(p => p.Tel != null && p.Tel.Contains(needle))
            .OrderBy(p => p.Tel == needle ? 0
                        : p.Tel!.StartsWith(needle) ? 1 : 2)
            .ThenBy(p => p.Tel);

        return await PaginateAsync(query, SearchProjections.Patronazhist, pageNumber, pageSize);
    }

    // ─── Validation ──────────────────────────────────────────────────────

    private static void ValidateBothNames(string? mbiemri, string? emri)
    {
        if (string.IsNullOrEmpty(mbiemri) && string.IsNullOrEmpty(emri))
        {
            throw new ArgumentException("Emri ose mbiemri duhet te plotesohet");
        }

        if ((mbiemri?.Length ?? 0) > MaxInputLength || (emri?.Length ?? 0) > MaxInputLength)
        {
            throw new ArgumentException($"Emri dhe mbiemri nuk mund te kete me shume se {MaxInputLength} karaktere");
        }
    }

    private static void ValidateSingleField(string? value, string fieldName, int minLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException($"{fieldName} nuk mund te jene bosh");
        }

        if (value.Length < minLength)
        {
            throw new ArgumentException($"{fieldName} duhet te kete te pakten {minLength} karaktere");
        }

        if (value.Length > MaxInputLength)
        {
            throw new ArgumentException($"{fieldName} nuk mund te kete me shume se {MaxInputLength} karaktere");
        }
    }

    private static (int pageNumber, int pageSize) ClampPagination(int pageNumber, int pageSize)
        => (Math.Max(1, pageNumber), Math.Clamp(pageSize, 1, MaxPageSize));

    // ─── Generic paginated projection ────────────────────────────────────

    private static async Task<PaginatedResult<TResponse>> PaginateAsync<TEntity, TResponse>(
        IQueryable<TEntity> query,
        Expression<Func<TEntity, TResponse>> projection,
        int pageNumber, int pageSize)
    {
        var totalItems = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(projection)
            .ToListAsync();

        return new PaginatedResult<TResponse>
        {
            Items = items,
            Pagination = new PaginationInfo
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            }
        };
    }

    // ─── Normalized prefix search (range query on indexed column) ───────

    /// <summary>
    /// Prefix-matches on precomputed normalized columns using a pure range query
    /// (col &gt;= prefix AND col &lt;= prefix + '\uFFFF'), which is directly sargable
    /// against the composite B-tree index on (MbiemriNormalized, EmriNormalized).
    /// No per-row REPLACE/LOWER calls, no LIKE/ESCAPE — SQLite can seek the index.
    /// </summary>
    private static async Task<PaginatedResult<TResponse>> PrefixSearchAsync<TEntity, TResponse>(
        DbSet<TEntity> dbSet,
        Expression<Func<TEntity, string?>> emriNormalizedSelector,
        Expression<Func<TEntity, string?>> mbiemriNormalizedSelector,
        Expression<Func<TEntity, TResponse>> projection,
        string emri,
        string mbiemri,
        int pageNumber,
        int pageSize)
        where TEntity : class
    {
        var (where, rank) = BuildPrefixPredicate(
            emriNormalizedSelector, mbiemriNormalizedSelector, emri, mbiemri);

        var query = dbSet.AsNoTracking().Where(where).OrderBy(rank);
        return await PaginateAsync(query, projection, pageNumber, pageSize);
    }

    private static (Expression<Func<TEntity, bool>> Where, Expression<Func<TEntity, int>> Rank)
        BuildPrefixPredicate<TEntity>(
        Expression<Func<TEntity, string?>> emriSelector,
        Expression<Func<TEntity, string?>> mbiemriSelector,
        string emri,
        string mbiemri)
    {
        var param = emriSelector.Parameters[0];
        var emriBody = emriSelector.Body;
        var mbiemriBody = new ParameterReplacer(mbiemriSelector.Parameters[0], param)
            .Visit(mbiemriSelector.Body);

        var zero = Expression.Constant(0);
        Expression RangeClause(Expression col, string prefix)
        {
            var notNull = Expression.NotEqual(col, Expression.Constant(null, typeof(string)));
            var ge = Expression.GreaterThanOrEqual(
                Expression.Call(null, StringCompareMethod, col, Expression.Constant(prefix)), zero);
            var le = Expression.LessThanOrEqual(
                Expression.Call(null, StringCompareMethod, col, Expression.Constant(prefix + RangeUpperSentinel)), zero);
            return Expression.AndAlso(notNull, Expression.AndAlso(ge, le));
        }

        var hasMbiemri = mbiemri.Length > 0;
        var hasEmri = emri.Length > 0;

        // (MbiemriNormalized >= mbiemri AND <= mbiemri+0xFFFF)
        //  AND (EmriNormalized  >= emri AND <= emri+0xFFFF)
        // One-sided calls omit the missing clause so SQLite can pick the matching
        // single-column index (IX_*_EmriNormalized) instead of scanning the composite.
        Expression condition = (hasMbiemri, hasEmri) switch
        {
            (true, true) => Expression.AndAlso(RangeClause(mbiemriBody, mbiemri), RangeClause(emriBody, emri)),
            (true, false) => RangeClause(mbiemriBody, mbiemri),
            (false, true) => RangeClause(emriBody, emri),
            _ => throw new InvalidOperationException("BuildPrefixPredicate requires at least one prefix")
        };

        Expression? exactExpr = null;
        if (hasMbiemri)
        {
            exactExpr = Expression.Equal(mbiemriBody, Expression.Constant(mbiemri, typeof(string)));
        }

        if (hasEmri)
        {
            var e = Expression.Equal(emriBody, Expression.Constant(emri, typeof(string)));
            exactExpr = exactExpr == null ? e : Expression.AndAlso(exactExpr, e);
        }
        Expression rank = exactExpr != null
            ? Expression.Condition(exactExpr, Expression.Constant(0), Expression.Constant(1))
            : Expression.Constant(1);

        return (
            Expression.Lambda<Func<TEntity, bool>>(condition, param),
            Expression.Lambda<Func<TEntity, int>>(rank, param));
    }

    // ─── Helpers ─────────────────────────────────────────────────────────

    /// <summary>
    /// Normalizes an input search string to match the stored normalized columns:
    /// lowercases (C#'s ToLower handles Ç→ç and Ë→ë that SQLite's LOWER cannot),
    /// trims, folds Albanian diacritics (ç → c, ë → e), and strips control chars
    /// and characters that have no place in a name (%, _, \).
    /// </summary>
    private static string NormalizeAlbanian(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var lowered = input.ToLower().Trim()
            .Replace("ç", "c")
            .Replace("ë", "e");

        var cleaned = new StringBuilder(lowered.Length);
        foreach (var c in lowered)
        {
            if (c == '%' || c == '_' || c == '\\')
            {
                continue;
            }

            if (char.IsControl(c))
            {
                continue;
            }

            cleaned.Append(c);
        }
        return cleaned.ToString();
    }

    private static SearchResponse EmptySearchResponse(int pageNumber, int pageSize)
    {
        var pagination = new PaginationInfo
        {
            CurrentPage = pageNumber,
            PageSize = pageSize,
            TotalItems = 0
        };
        return new SearchResponse
        {
            Person = new PaginatedResult<PersonResponse> { Items = [], Pagination = pagination },
            Rrogat = new PaginatedResult<RrogatResponse> { Items = [], Pagination = pagination },
            Targat = new PaginatedResult<TargatResponse> { Items = [], Pagination = pagination },
            Patronazhist = new PaginatedResult<PatronazhistResponse> { Items = [], Pagination = pagination }
        };
    }

    private class ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == oldParam ? newParam : base.VisitParameter(node);
    }
}
