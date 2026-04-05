using Microsoft.EntityFrameworkCore;
using Kerko.Infrastructure;
using Kerko.Models;
using System.Linq.Expressions;

namespace Kerko.Services;

public interface ISearchService
{
    Task<SearchResponse> KerkoAsync(string? mbiemri, string? emri, int pageNumber = 1, int pageSize = 10, string? qyteti = null);
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

    public async Task<SearchResponse> KerkoAsync(string? mbiemri, string? emri, int pageNumber = 1, int pageSize = DefaultPageSize, string? qyteti = null)
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

            Expression<Func<Person, bool>>? cityFilter = null;
            if (!string.IsNullOrEmpty(qyteti))
            {
                var normalizedCity = qyteti.Trim().ToLower();
                cityFilter = p => p.Qyteti != null && p.Qyteti.ToLower().Contains(normalizedCity);
            }

            var personTask = SearchByNameAsync(
                _db.Person,
                p => p.Emer,
                p => p.Mbiemer,
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
                normalizedEmri, normalizedMbiemri, pageNumber, pageSize, cityFilter);

            var rrogatTask = SearchByNameAsync(
                _db.Rrogat,
                r => r.Emri,
                r => r.Mbiemri,
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

            var targatTask = SearchByNameAsync(
                _db.Targat,
                t => t.Emri,
                t => t.Mbiemri,
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

            var patronazhistTask = SearchByNameAsync(
                _db.Patronazhist,
                p => p.Emri,
                p => p.Mbiemri,
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

            await Task.WhenAll(personTask, rrogatTask, targatTask, patronazhistTask);

            return new SearchResponse
            {
                Person = await personTask,
                Rrogat = await rrogatTask,
                Targat = await targatTask,
                Patronazhist = await patronazhistTask
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

    private async Task<PaginatedResult<TResponse>> SearchByNameAsync<TEntity, TResponse>(
        DbSet<TEntity> dbSet,
        Expression<Func<TEntity, string?>> emriSelector,
        Expression<Func<TEntity, string?>> mbiemriSelector,
        Expression<Func<TEntity, TResponse>> mapToResponse,
        string emri,
        string mbiemri,
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>>? additionalFilter = null)
        where TEntity : class
    {
        var param = emriSelector.Parameters[0];

        var emriBody = emriSelector.Body;
        var mbiemriBody = new ParameterReplacer(mbiemriSelector.Parameters[0], param)
            .Visit(mbiemriSelector.Body);

        var likeMethod = typeof(DbFunctionsExtensions).GetMethod(
            nameof(DbFunctionsExtensions.Like),
            [typeof(DbFunctions), typeof(string), typeof(string)])!;

        var efFunctions = Expression.Property(null, typeof(EF), nameof(EF.Functions));

        // Normalize columns SQL-side: REPLACE(REPLACE(LOWER(col), 'ç', 'c'), 'ë', 'e')
        // This produces exactly 1 LIKE per column regardless of input length
        var normalizedEmriCol = BuildSqlNormalize(emriBody);
        var normalizedMbiemriCol = BuildSqlNormalize(mbiemriBody);

        // WHERE: normalized(col) LIKE '%search%'
        var emriPattern = $"%{EscapeLikePattern(emri)}%";
        var mbiemriPattern = $"%{EscapeLikePattern(mbiemri)}%";

        var emriCondition = Expression.Call(likeMethod, efFunctions, normalizedEmriCol, Expression.Constant(emriPattern));
        var mbiemriCondition = Expression.Call(likeMethod, efFunctions, normalizedMbiemriCol, Expression.Constant(mbiemriPattern));

        var emriNotNull = Expression.NotEqual(emriBody, Expression.Constant(null, typeof(string)));
        var mbiemriNotNull = Expression.NotEqual(mbiemriBody, Expression.Constant(null, typeof(string)));

        Expression combinedCondition = Expression.AndAlso(
            Expression.AndAlso(emriNotNull, mbiemriNotNull),
            Expression.AndAlso(emriCondition, mbiemriCondition));

        if (additionalFilter != null)
        {
            var rewrittenFilter = new ParameterReplacer(additionalFilter.Parameters[0], param)
                .Visit(additionalFilter.Body);
            combinedCondition = Expression.AndAlso(combinedCondition, rewrittenFilter);
        }

        var whereExpression = Expression.Lambda<Func<TEntity, bool>>(combinedCondition, param);

        // ORDER BY relevance: exact match (0) > starts with (1) > contains (2)
        var isExactMatch = Expression.AndAlso(
            Expression.Equal(normalizedEmriCol, Expression.Constant(emri)),
            Expression.Equal(normalizedMbiemriCol, Expression.Constant(mbiemri)));

        var emriStartsPattern = $"{EscapeLikePattern(emri)}%";
        var mbiemriStartsPattern = $"{EscapeLikePattern(mbiemri)}%";
        var isStartsWith = Expression.AndAlso(
            Expression.Call(likeMethod, efFunctions, normalizedEmriCol, Expression.Constant(emriStartsPattern)),
            Expression.Call(likeMethod, efFunctions, normalizedMbiemriCol, Expression.Constant(mbiemriStartsPattern)));

        var orderExpression = Expression.Condition(
            isExactMatch,
            Expression.Constant(0),
            Expression.Condition(
                isStartsWith,
                Expression.Constant(1),
                Expression.Constant(2)));
        var orderLambda = Expression.Lambda<Func<TEntity, int>>(orderExpression, param);

        var query = dbSet.AsNoTracking().Where(whereExpression).OrderBy(orderLambda);

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
    /// Normalizes Albanian diacritics in a search string: ç→c, ë→e
    /// </summary>
    private static string NormalizeAlbanian(string input)
    {
        return input.ToLower().Trim()
            .Replace("ç", "c")
            .Replace("ë", "e");
    }

    /// <summary>
    /// Builds a SQL-side expression: REPLACE(REPLACE(LOWER(col), 'ç', 'c'), 'ë', 'e')
    /// Normalizes diacritics in the database column so we only need 1 LIKE per column.
    /// </summary>
    private static Expression BuildSqlNormalize(Expression columnBody)
    {
        var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
        var replaceMethod = typeof(string).GetMethod("Replace", [typeof(string), typeof(string)])!;

        // LOWER(col)
        var lowered = Expression.Call(columnBody, toLowerMethod);
        // REPLACE(LOWER(col), 'ç', 'c')
        var replacedC = Expression.Call(lowered, replaceMethod, Expression.Constant("ç"), Expression.Constant("c"));
        // REPLACE(..., 'Ç', 'c')
        var replacedCUpper = Expression.Call(replacedC, replaceMethod, Expression.Constant("Ç"), Expression.Constant("c"));
        // REPLACE(..., 'ë', 'e')
        var replacedE = Expression.Call(replacedCUpper, replaceMethod, Expression.Constant("ë"), Expression.Constant("e"));
        // REPLACE(..., 'Ë', 'e')
        var replacedEUpper = Expression.Call(replacedE, replaceMethod, Expression.Constant("Ë"), Expression.Constant("e"));

        return replacedEUpper;
    }

    /// <summary>
    /// Escapes special LIKE pattern characters: %, _, \
    /// </summary>
    private static string EscapeLikePattern(string input)
    {
        return input
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
    }

    private class ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam)
        : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == oldParam ? newParam : base.VisitParameter(node);
    }
}
