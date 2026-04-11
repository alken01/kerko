using Microsoft.EntityFrameworkCore;
using Kerko.Infrastructure;
using Kerko.Models;
using System.Linq.Expressions;

namespace Kerko.Services;

public interface ISearchService
{
    Task<SearchResponse> KerkoAsync(string? mbiemri, string? emri, int pageNumber = 1, int pageSize = 10);
    Task<PaginatedResult<TargatResponse>> TargatAsync(string? numriTarges, int pageNumber = 1, int pageSize = 10);
    Task<PaginatedResult<PatronazhistResponse>> TelefonAsync(string? numriTelefonit, int pageNumber = 1, int pageSize = 10);
    Task<List<Dictionary<string, int>>> DbStatusAsync();
}

public class SearchService : ISearchService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<SearchService> _logger;
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;
    private const int MinTargesLength = 6;
    private const int MinNameLength = 2;
    private const int MaxInputLength = 100;

    // Sentinel char appended to a prefix to form an inclusive upper bound for a
    // range scan: any string starting with `prefix` is <= `prefix + \uFFFF`.
    // This turns StartsWith into a pure B-tree range query that uses the index.
    private const char RangeUpperSentinel = '\uFFFF';

    // EF Core translates string.Compare(col, const) op 0 to col op const in SQL.
    private static readonly System.Reflection.MethodInfo StringCompareMethod = typeof(string).GetMethod(
        nameof(string.Compare), [typeof(string), typeof(string)])!;

    public SearchService(ApplicationDbContext db, ILogger<SearchService> logger)
    {
        _db = db;
        _logger = logger;
    }

    private static (int pageNumber, int pageSize) ValidatePagination(int pageNumber, int pageSize)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);
        return (pageNumber, pageSize);
    }

    public async Task<SearchResponse> KerkoAsync(string? mbiemri, string? emri, int pageNumber = 1, int pageSize = DefaultPageSize)
    {
        try
        {
            if (string.IsNullOrEmpty(mbiemri) || string.IsNullOrEmpty(emri))
            {
                throw new ArgumentException("Emri dhe mbiemri nuk mund te jene bosh");
            }

            if (mbiemri.Length < MinNameLength || emri.Length < MinNameLength)
            {
                throw new ArgumentException($"Emri dhe mbiemri duhet te kete te pakten {MinNameLength} karaktere");
            }

            if (mbiemri.Length > MaxInputLength || emri.Length > MaxInputLength)
            {
                throw new ArgumentException($"Emri dhe mbiemri nuk mund te kete me shume se {MaxInputLength} karaktere");
            }

            (pageNumber, pageSize) = ValidatePagination(pageNumber, pageSize);

            var normalizedMbiemri = NormalizeAlbanian(mbiemri);
            var normalizedEmri = NormalizeAlbanian(emri);

            // Reject any input that would leave us without a usable prefix. After
            // normalization the search term must still have characters; a user
            // typing only wildcards/whitespace should not match everything.
            if (normalizedMbiemri.Length == 0 || normalizedEmri.Length == 0)
            {
                return EmptySearchResponse(pageNumber, pageSize);
            }

            // Run sequentially — DbContext is not thread-safe and SQLite
            // serializes access anyway, so parallelism would buy nothing.
            var person = await SearchByNameAsync(
                _db.Person,
                p => p.EmerNormalized,
                p => p.MbiemerNormalized,
                p => new PersonResponse
                {
                    Adresa = p.Adresa,
                    NrBaneses = p.NrBaneses,
                    Emri = p.Emer,
                    Mbiemri = p.Mbiemer,
                    Atesi = p.Atesi,
                    Amesi = p.Amesi,
                    Datelindja = p.Datelindja,
                    Vendlindja = p.Vendlindja,
                    Seksi = p.Seksi,
                    LidhjaMeKryefamiljarin = p.LidhjaMeKryefamiljarin,
                    Qyteti = p.Qyteti,
                    GjendjeCivile = p.GjendjeCivile,
                    Kombesia = p.Kombesia
                },
                normalizedEmri, normalizedMbiemri, pageNumber, pageSize);

            var rrogat = await SearchByNameAsync(
                _db.Rrogat,
                r => r.EmriNormalized,
                r => r.MbiemriNormalized,
                r => new RrogatResponse
                {
                    NumriPersonal = r.NumriPersonal,
                    Emri = r.Emri,
                    Mbiemri = r.Mbiemri,
                    NIPT = r.NIPT,
                    DRT = r.DRT,
                    PagaBruto = r.PagaBruto,
                    Profesioni = r.Profesioni,
                    Kategoria = r.Kategoria
                },
                normalizedEmri, normalizedMbiemri, pageNumber, pageSize);

            var targat = await SearchByNameAsync(
                _db.Targat,
                t => t.EmriNormalized,
                t => t.MbiemriNormalized,
                t => new TargatResponse
                {
                    NumriTarges = t.NumriTarges,
                    Marka = t.Marka,
                    Modeli = t.Modeli,
                    Ngjyra = t.Ngjyra,
                    NumriPersonal = t.NumriPersonal,
                    Emri = t.Emri,
                    Mbiemri = t.Mbiemri
                },
                normalizedEmri, normalizedMbiemri, pageNumber, pageSize);

            var patronazhist = await SearchByNameAsync(
                _db.Patronazhist,
                p => p.EmriNormalized,
                p => p.MbiemriNormalized,
                p => new PatronazhistResponse
                {
                    NumriPersonal = p.NumriPersonal,
                    Emri = p.Emri,
                    Mbiemri = p.Mbiemri,
                    Atesi = p.Atesi,
                    Datelindja = p.Datelindja,
                    QV = p.QV,
                    ListaNr = p.ListaNr,
                    Tel = p.Tel,
                    Emigrant = p.Emigrant,
                    Country = p.Country,
                    ISigurte = p.ISigurte,
                    Koment = p.Koment,
                    Patronazhisti = p.Patronazhisti,
                    Preferenca = p.Preferenca,
                    Census2013Preferenca = p.Census2013Preferenca,
                    Census2013Siguria = p.Census2013Siguria,
                    Vendlindja = p.Vendlindja,
                    Kompania = p.Kompania,
                    KodBanese = p.KodBanese
                },
                normalizedEmri, normalizedMbiemri, pageNumber, pageSize);

            return new SearchResponse
            {
                Person = person,
                Rrogat = rrogat,
                Targat = targat,
                Patronazhist = patronazhist
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for {mbiemri} {emri}", mbiemri, emri);
            throw;
        }
    }

    public async Task<PaginatedResult<TargatResponse>> TargatAsync(string? numriTarges, int pageNumber = 1, int pageSize = DefaultPageSize)
    {
        try
        {
            if (string.IsNullOrEmpty(numriTarges))
            {
                throw new ArgumentException("Numri i targes nuk mund te jene bosh");
            }

            if (numriTarges.Length < MinTargesLength)
            {
                throw new ArgumentException($"Numri i targes duhet te kete te pakten {MinTargesLength} karaktere");
            }

            if (numriTarges.Length > MaxInputLength)
            {
                throw new ArgumentException($"Numri i targes nuk mund te kete me shume se {MaxInputLength} karaktere");
            }

            (pageNumber, pageSize) = ValidatePagination(pageNumber, pageSize);

            var normalizedNumriTarges = numriTarges.ToLower().Trim();

            var query = _db.Targat
                .AsNoTracking()
                .Where(t => t.NumriTarges != null && t.NumriTarges.ToLower().Contains(normalizedNumriTarges))
                .OrderBy(t => t.NumriTarges!.ToLower() == normalizedNumriTarges ? 0 :
                             t.NumriTarges!.ToLower().StartsWith(normalizedNumriTarges) ? 1 : 2)
                .ThenBy(t => t.NumriTarges);

            var totalItems = await query.CountAsync();

            var results = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TargatResponse
                {
                    NumriTarges = t.NumriTarges,
                    Marka = t.Marka,
                    Modeli = t.Modeli,
                    Ngjyra = t.Ngjyra,
                    NumriPersonal = t.NumriPersonal,
                    Emri = t.Emri,
                    Mbiemri = t.Mbiemri
                })
                .ToListAsync();

            return new PaginatedResult<TargatResponse>
            {
                Items = results,
                Pagination = new PaginationInfo
                {
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for targat {numriTarges}", numriTarges);
            throw;
        }
    }

    public async Task<PaginatedResult<PatronazhistResponse>> TelefonAsync(string? numriTelefonit, int pageNumber = 1, int pageSize = DefaultPageSize)
    {
        try
        {
            if (string.IsNullOrEmpty(numriTelefonit))
            {
                throw new ArgumentException("Numri i telefonit nuk mund te jene bosh");
            }

            if (numriTelefonit.Length < 10)
            {
                throw new ArgumentException("Numri i telefonit duhet te kete te pakten 10 karaktere");
            }

            if (numriTelefonit.Length > MaxInputLength)
            {
                throw new ArgumentException($"Numri i telefonit nuk mund te kete me shume se {MaxInputLength} karaktere");
            }

            (pageNumber, pageSize) = ValidatePagination(pageNumber, pageSize);

            var normalizedNumriTelefonit = numriTelefonit.Trim();

            var query = _db.Patronazhist
                .AsNoTracking()
                .Where(p => p.Tel != null && p.Tel.Contains(normalizedNumriTelefonit))
                .OrderBy(p => p.Tel == normalizedNumriTelefonit ? 0 :
                             p.Tel!.StartsWith(normalizedNumriTelefonit) ? 1 : 2)
                .ThenBy(p => p.Tel);

            var totalItems = await query.CountAsync();

            var results = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PatronazhistResponse
                {
                    NumriPersonal = p.NumriPersonal,
                    Emri = p.Emri,
                    Mbiemri = p.Mbiemri,
                    Atesi = p.Atesi,
                    Datelindja = p.Datelindja,
                    QV = p.QV,
                    ListaNr = p.ListaNr,
                    Tel = p.Tel,
                    Emigrant = p.Emigrant,
                    Country = p.Country,
                    ISigurte = p.ISigurte,
                    Koment = p.Koment,
                    Patronazhisti = p.Patronazhisti,
                    Preferenca = p.Preferenca,
                    Census2013Preferenca = p.Census2013Preferenca,
                    Census2013Siguria = p.Census2013Siguria,
                    Vendlindja = p.Vendlindja,
                    Kompania = p.Kompania,
                    KodBanese = p.KodBanese
                })
                .ToListAsync();

            return new PaginatedResult<PatronazhistResponse>
            {
                Items = results,
                Pagination = new PaginationInfo
                {
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for telefon {numriTelefonit}", numriTelefonit);
            throw;
        }
    }

    public async Task<List<Dictionary<string, int>>> DbStatusAsync()
    {
        return
        [
            new() { { "Person", await _db.Person.CountAsync() } },
            new() { { "Rrogat", await _db.Rrogat.CountAsync() } },
            new() { { "Targat", await _db.Targat.CountAsync() } },
            new() { { "Patronazhist", await _db.Patronazhist.CountAsync() } }
        ];
    }

    /// <summary>
    /// Prefix-matches on precomputed normalized columns using a pure range query
    /// (col >= prefix AND col <= prefix + '\uFFFF'), which is directly sargable
    /// against the composite B-tree index on (MbiemriNormalized, EmriNormalized).
    /// No per-row REPLACE/LOWER calls, no LIKE/ESCAPE — SQLite can seek the index.
    /// </summary>
    private async Task<PaginatedResult<TResponse>> SearchByNameAsync<TEntity, TResponse>(
        DbSet<TEntity> dbSet,
        Expression<Func<TEntity, string?>> emriNormalizedSelector,
        Expression<Func<TEntity, string?>> mbiemriNormalizedSelector,
        Expression<Func<TEntity, TResponse>> mapToResponse,
        string emri,
        string mbiemri,
        int pageNumber,
        int pageSize)
        where TEntity : class
    {
        var param = emriNormalizedSelector.Parameters[0];
        var emriBody = emriNormalizedSelector.Body;
        var mbiemriBody = new ParameterReplacer(mbiemriNormalizedSelector.Parameters[0], param)
            .Visit(mbiemriNormalizedSelector.Body);

        var emriUpper = emri + RangeUpperSentinel;
        var mbiemriUpper = mbiemri + RangeUpperSentinel;

        var zero = Expression.Constant(0);

        Expression GeRange(Expression col, string lower) =>
            Expression.GreaterThanOrEqual(
                Expression.Call(null, StringCompareMethod, col, Expression.Constant(lower)),
                zero);
        Expression LeRange(Expression col, string upper) =>
            Expression.LessThanOrEqual(
                Expression.Call(null, StringCompareMethod, col, Expression.Constant(upper)),
                zero);

        var emriNotNull = Expression.NotEqual(emriBody, Expression.Constant(null, typeof(string)));
        var mbiemriNotNull = Expression.NotEqual(mbiemriBody, Expression.Constant(null, typeof(string)));

        // (MbiemriNormalized >= mbiemri AND MbiemriNormalized <= mbiemri+0xFFFF)
        //  AND (EmriNormalized >= emri AND EmriNormalized <= emri+0xFFFF)
        // Ordered this way so the composite index (Mbiemri_N, Emri_N) is used.
        var condition = Expression.AndAlso(
            Expression.AndAlso(mbiemriNotNull, emriNotNull),
            Expression.AndAlso(
                Expression.AndAlso(
                    GeRange(mbiemriBody, mbiemri),
                    LeRange(mbiemriBody, mbiemriUpper)),
                Expression.AndAlso(
                    GeRange(emriBody, emri),
                    LeRange(emriBody, emriUpper))));

        var whereLambda = Expression.Lambda<Func<TEntity, bool>>(condition, param);

        // Order: exact match first, then by surname then first name.
        var isExact = Expression.AndAlso(
            Expression.Equal(mbiemriBody, Expression.Constant(mbiemri, typeof(string))),
            Expression.Equal(emriBody, Expression.Constant(emri, typeof(string))));
        var orderRank = Expression.Condition(isExact, Expression.Constant(0), Expression.Constant(1));
        var orderRankLambda = Expression.Lambda<Func<TEntity, int>>(orderRank, param);

        var query = dbSet.AsNoTracking()
            .Where(whereLambda)
            .OrderBy(orderRankLambda);

        var totalItems = await query.CountAsync();

        var results = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(mapToResponse)
            .ToListAsync();

        return new PaginatedResult<TResponse>
        {
            Items = results,
            Pagination = new PaginationInfo
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            }
        };
    }

    /// <summary>
    /// Normalizes an input search string to match the stored normalized columns:
    /// lowercases (C#'s ToLower handles Ç→ç and Ë→ë that SQLite's LOWER cannot),
    /// trims, folds Albanian diacritics (ç → c, ë → e), and strips control chars
    /// and characters that have no place in a name (%, _, \).
    /// </summary>
    private static string NormalizeAlbanian(string input)
    {
        var lowered = input.ToLower().Trim()
            .Replace("ç", "c")
            .Replace("ë", "e");

        // Strip chars that have no place in a name and would otherwise pollute
        // the range query bounds.
        var cleaned = new System.Text.StringBuilder(lowered.Length);
        foreach (var c in lowered)
        {
            if (c == '%' || c == '_' || c == '\\') continue;
            if (char.IsControl(c)) continue;
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

    private class ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam)
        : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == oldParam ? newParam : base.VisitParameter(node);
    }
}
