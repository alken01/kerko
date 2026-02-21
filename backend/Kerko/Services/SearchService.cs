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

            (pageNumber, pageSize) = ValidatePagination(pageNumber, pageSize);

            var normalizedMbiemri = NormalizeAlbanian(mbiemri);
            var normalizedEmri = NormalizeAlbanian(emri);

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
                normalizedEmri, normalizedMbiemri, pageNumber, pageSize);

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
        int pageSize)
        where TEntity : class
    {
        // Use a single parameter for both selectors
        var param = emriSelector.Parameters[0];

        // Rebind mbiemriSelector body to use the same parameter
        var emriBody = emriSelector.Body;
        var mbiemriBody = new ParameterReplacer(mbiemriSelector.Parameters[0], param)
            .Visit(mbiemriSelector.Body);

        var likeMethod = typeof(DbFunctionsExtensions).GetMethod(
            nameof(DbFunctionsExtensions.Like),
            [typeof(DbFunctions), typeof(string), typeof(string)])!;

        var efFunctions = Expression.Property(null, typeof(EF), nameof(EF.Functions));

        // Generate all diacritic variants (e.g. "cela" → ["cela", "çela", "celë", "çelë"])
        var emriVariants = GenerateAlbanianVariants(emri);
        var mbiemriVariants = GenerateAlbanianVariants(mbiemri);

        // Build OR chain: col LIKE '%var1%' OR col LIKE '%var2%' OR ...
        var emriCondition = BuildOrLikeChain(likeMethod, efFunctions, emriBody, emriVariants, contains: true);
        var mbiemriCondition = BuildOrLikeChain(likeMethod, efFunctions, mbiemriBody, mbiemriVariants, contains: true);

        var emriNotNull = Expression.NotEqual(emriBody, Expression.Constant(null, typeof(string)));
        var mbiemriNotNull = Expression.NotEqual(mbiemriBody, Expression.Constant(null, typeof(string)));

        var combinedCondition = Expression.AndAlso(
            Expression.AndAlso(emriNotNull, mbiemriNotNull),
            Expression.AndAlso(emriCondition, mbiemriCondition));

        var whereExpression = Expression.Lambda<Func<TEntity, bool>>(combinedCondition, param);

        // Order by relevance: exact match (0) > starts with (1) > contains (2)
        // Uses variant-based checks on raw columns — no per-row REPLACE
        var exactEmri = BuildOrEqualityChain(emriBody, emriVariants);
        var exactMbiemri = BuildOrEqualityChain(mbiemriBody, mbiemriVariants);
        var isExactMatch = Expression.AndAlso(exactEmri, exactMbiemri);

        var startsWithEmri = BuildOrLikeChain(likeMethod, efFunctions, emriBody, emriVariants, contains: false);
        var startsWithMbiemri = BuildOrLikeChain(likeMethod, efFunctions, mbiemriBody, mbiemriVariants, contains: false);
        var isStartsWith = Expression.AndAlso(startsWithEmri, startsWithMbiemri);

        var orderExpression = Expression.Condition(
            isExactMatch,
            Expression.Constant(0),
            Expression.Condition(
                isStartsWith,
                Expression.Constant(1),
                Expression.Constant(2)
            )
        );
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
    /// Generates all diacritic variants of a normalized search term.
    /// e.g. "cela" → ["cela", "çela", "celë", "çelë"]
    /// </summary>
    /// <summary>
    /// Generates all diacritic variants of a normalized search term,
    /// including uppercase diacritics (SQLite LIKE is only case-insensitive for ASCII).
    /// e.g. "kuci" → ["kuci", "kuçi", "kuÇi"]
    /// </summary>
    private static List<string> GenerateAlbanianVariants(string input)
    {
        var variants = new List<string> { "" };
        foreach (var ch in input)
        {
            // ASCII letters are handled by SQLite's case-insensitive LIKE,
            // but ç/Ç and ë/Ë are Unicode so we need both cases explicitly
            char[] charVariants = ch switch
            {
                'c' => ['c', 'ç', 'Ç'],
                'e' => ['e', 'ë', 'Ë'],
                _ => [ch]
            };

            var newVariants = new List<string>(variants.Count * charVariants.Length);
            foreach (var variant in variants)
            {
                foreach (var cv in charVariants)
                {
                    newVariants.Add(variant + cv);
                }
            }
            variants = newVariants;
        }
        return variants;
    }

    /// <summary>
    /// Builds: col LIKE '%var1%' OR col LIKE '%var2%' OR ...
    /// When contains=false, builds prefix patterns: col LIKE 'var1%' OR col LIKE 'var2%' OR ...
    /// </summary>
    private static Expression BuildOrLikeChain(
        System.Reflection.MethodInfo likeMethod,
        Expression efFunctions,
        Expression columnBody,
        List<string> variants,
        bool contains)
    {
        Expression? result = null;
        foreach (var variant in variants)
        {
            var pattern = contains ? $"%{variant}%" : $"{variant}%";
            var like = Expression.Call(likeMethod, efFunctions, columnBody, Expression.Constant(pattern));
            result = result == null ? like : Expression.OrElse(result, like);
        }
        return result!;
    }

    /// <summary>
    /// Builds: LOWER(col) = 'var1' OR LOWER(col) = 'var2' OR ...
    /// Used for exact match detection in ORDER BY.
    /// </summary>
    private static Expression BuildOrEqualityChain(Expression columnBody, List<string> variants)
    {
        var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
        var loweredCol = Expression.Call(columnBody, toLowerMethod);

        Expression? result = null;
        foreach (var variant in variants)
        {
            var eq = Expression.Equal(loweredCol, Expression.Constant(variant));
            result = result == null ? eq : Expression.OrElse(result, eq);
        }
        return result!;
    }

    private class ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam)
        : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == oldParam ? newParam : base.VisitParameter(node);
    }
}
