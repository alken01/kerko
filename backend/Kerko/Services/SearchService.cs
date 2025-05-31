using Microsoft.EntityFrameworkCore;
using Kerko.Infrastructure;
using Kerko.Models;

namespace Kerko.Services;

public interface ISearchService
{
    Task<SearchResponse> KerkoAsync(string? mbiemri, string? emri);
    Task<SearchResponse> TargatAsync(string? numriTarges);
    Task<List<Dictionary<string, int>>> DbStatusAsync();
    Task<IEnumerable<SearchLog>> GetSearchLogsAsync(DateTime? startDate = null, DateTime? endDate = null);
}

public class SearchService : ISearchService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<SearchService> _logger;
    private const int MaxResults = 100;
    private const int MinTargesLength = 6;
    private const int MinNameLength = 2;

    public SearchService(ApplicationDbContext db, ILogger<SearchService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<SearchResponse> KerkoAsync(string? mbiemri, string? emri)
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

            var normalizedMbiemri = mbiemri.ToLower().Trim();
            var normalizedEmri = emri.ToLower().Trim();

            var tasks = new List<Task<IEnumerable<IResponseModel>>>
            {
                GetPersonResults(normalizedMbiemri, normalizedEmri),
                GetRrogatResults(normalizedMbiemri, normalizedEmri),
                GetTargatResults(normalizedMbiemri, normalizedEmri),
                GetPatronazhistResults(normalizedMbiemri, normalizedEmri)
            };

            await Task.WhenAll(tasks);

            return new SearchResponse
            {
                Person = [.. (await tasks[0]).Cast<PersonResponse>()],
                Rrogat = [.. (await tasks[1]).Cast<RrogatResponse>()],
                Targat = [.. (await tasks[2]).Cast<TargatResponse>()],
                Patronazhist = [.. (await tasks[3]).Cast<PatronazhistResponse>()]
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for {mbiemri} {emri}", mbiemri, emri);
            throw;
        }
    }

    public async Task<SearchResponse> TargatAsync(string? numriTarges)
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

            var normalizedNumriTarges = numriTarges.ToLower().Trim();

            var targatResults = await _db.Targat
                .AsNoTracking()
                .Where(t => t.NumriTarges != null && t.NumriTarges.ToLower().Contains(normalizedNumriTarges))
                .Take(MaxResults)
                .OrderBy(t => t.NumriTarges)
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

            targatResults = [.. targatResults.Where(t => !IsNameBanned(t?.Emri, t?.Mbiemri))];

            return new SearchResponse
            {
                Targat = targatResults
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for targat {numriTarges}", numriTarges);
            throw;
        }
    }

    public async Task<IEnumerable<SearchLog>> GetSearchLogsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _db.SearchLogs.AsNoTracking();

        if (startDate.HasValue)
        {
            query = query.Where(l => l.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(l => l.Timestamp <= endDate.Value);
        }

        return await query
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync();
    }

    public async Task<List<Dictionary<string, int>>> DbStatusAsync()
    {
        return
        [
            new() { { "Person", await _db.Person.CountAsync() } },
            new() { { "Rrogat", await _db.Rrogat.CountAsync() } },
            new() { { "Targat", await _db.Targat.CountAsync() } },
            new() { { "Patronazhist", await _db.Patronazhist.CountAsync() } },
            new() { { "SearchLogs", await _db.SearchLogs.CountAsync() } }
        ];
    }

    private bool IsNameBanned(string? emri, string? mbiemri)
    {
        return false;
    }

    private async Task<IEnumerable<IResponseModel>> GetPersonResults(string mbiemri, string emri)
    {
        var results = await _db.Person
            .AsNoTracking()
            .Where(p => p.Mbiemer != null && p.Emer != null)
            .Where(p => EF.Functions.Like(p.Mbiemer, $"%{mbiemri}%") &&
                       EF.Functions.Like(p.Emer, $"%{emri}%"))
            .OrderBy(p => p.Mbiemer)
            .Take(MaxResults)
            .Select(p => new PersonResponse
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
            })
            .ToListAsync();

        return results.Where(p => !IsNameBanned(p?.Emri, p?.Mbiemri));
    }

    private async Task<IEnumerable<IResponseModel>> GetRrogatResults(string mbiemri, string emri)
    {
        var results = await _db.Rrogat
            .AsNoTracking()
            .Where(p => p.Mbiemri != null && p.Emri != null)
            .Where(p => EF.Functions.Like(p.Mbiemri, $"%{mbiemri}%") &&
                       EF.Functions.Like(p.Emri, $"%{emri}%"))
            .Take(MaxResults)
            .OrderBy(p => p.Mbiemri)
            .Select(r => new RrogatResponse
            {
                NumriPersonal = r.NumriPersonal,
                Emri = r.Emri,
                Mbiemri = r.Mbiemri,
                NIPT = r.NIPT,
                DRT = r.DRT,
                PagaBruto = r.PagaBruto,
                Profesioni = r.Profesioni,
                Kategoria = r.Kategoria
            })
            .ToListAsync();

        return results.Where(r => !IsNameBanned(r?.Emri, r?.Mbiemri));
    }

    private async Task<IEnumerable<IResponseModel>> GetTargatResults(string mbiemri, string emri)
    {
        var results = await _db.Targat
            .AsNoTracking()
            .Where(p => p.Mbiemri != null && p.Emri != null)
            .Where(p => EF.Functions.Like(p.Mbiemri, $"%{mbiemri}%") &&
                       EF.Functions.Like(p.Emri, $"%{emri}%"))
            .Take(MaxResults)
            .OrderBy(t => t.NumriTarges)
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

        return results.Where(t => !IsNameBanned(t?.Emri, t?.Mbiemri));
    }

    private async Task<IEnumerable<IResponseModel>> GetPatronazhistResults(string mbiemri, string emri)
    {
        var results = await _db.Patronazhist
            .AsNoTracking()
            .Where(p => p.Mbiemri != null && p.Emri != null)
            .Where(p => EF.Functions.Like(p.Mbiemri, $"%{mbiemri}%") &&
                       EF.Functions.Like(p.Emri, $"%{emri}%"))
            .Take(MaxResults)
            .OrderBy(p => p.Mbiemri)
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

        return results.Where(p => !IsNameBanned(p?.Emri, p?.Mbiemri));
    }
}