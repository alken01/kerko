using Microsoft.EntityFrameworkCore;
using Kerko.Infrastructure;
using Kerko.Models;

namespace Kerko.Services;

public interface ISearchService
{
    Task<SearchResponse> KerkoAsync(string? mbiemri, string? emri, int pageNumber = 1, int pageSize = 10);
    Task<PaginatedResult<TargatResponse>> TargatAsync(string? numriTarges, int pageNumber = 1, int pageSize = 10);
    Task<PaginatedResult<PatronazhistResponse>> TelefonAsync(string? numriTelefonit, int pageNumber = 1, int pageSize = 10);
    Task<List<Dictionary<string, int>>> DbStatusAsync();
    Task<IEnumerable<SearchLog>> GetSearchLogsAsync(DateTime? startDate = null, DateTime? endDate = null);
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

            var normalizedMbiemri = mbiemri.ToLower().Trim();
            var normalizedEmri = emri.ToLower().Trim();

            var tasks = new List<Task<PaginatedResult<IResponseModel>>>
            {
                GetPersonResultsPaginated(normalizedMbiemri, normalizedEmri, pageNumber, pageSize),
                GetRrogatResultsPaginated(normalizedMbiemri, normalizedEmri, pageNumber, pageSize),
                GetTargatResultsPaginated(normalizedMbiemri, normalizedEmri, pageNumber, pageSize),
                GetPatronazhistResultsPaginated(normalizedMbiemri, normalizedEmri, pageNumber, pageSize)
            };

            await Task.WhenAll(tasks);

            return new SearchResponse
            {
                Person = new PaginatedResult<PersonResponse>
                {
                    Items = (await tasks[0]).Items.Cast<PersonResponse>().ToList(),
                    Pagination = (await tasks[0]).Pagination
                },
                Rrogat = new PaginatedResult<RrogatResponse>
                {
                    Items = (await tasks[1]).Items.Cast<RrogatResponse>().ToList(),
                    Pagination = (await tasks[1]).Pagination
                },
                Targat = new PaginatedResult<TargatResponse>
                {
                    Items = (await tasks[2]).Items.Cast<TargatResponse>().ToList(),
                    Pagination = (await tasks[2]).Pagination
                },
                Patronazhist = new PaginatedResult<PatronazhistResponse>
                {
                    Items = (await tasks[3]).Items.Cast<PatronazhistResponse>().ToList(),
                    Pagination = (await tasks[3]).Pagination
                }
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
                // Order by relevance: exact matches first, then starts with, then contains
                .OrderBy(t => t.NumriTarges!.ToLower() == normalizedNumriTarges ? 0 :
                             t.NumriTarges!.ToLower().StartsWith(normalizedNumriTarges) ? 1 : 2)
                .ThenBy(t => t.NumriTarges);

            var totalItems = await query.CountAsync();

            var targatResults = await query
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

            var filteredResults = targatResults.Where(t => !IsNameBanned(t?.Emri, t?.Mbiemri)).ToList();

            return new PaginatedResult<TargatResponse>
            {
                Items = filteredResults,
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
                // Order by relevance: exact matches first, then starts with, then contains
                .OrderBy(p => p.Tel == normalizedNumriTelefonit ? 0 :
                             p.Tel!.StartsWith(normalizedNumriTelefonit) ? 1 : 2)
                .ThenBy(p => p.Tel);

            var totalItems = await query.CountAsync();

            var patronazhistResults = await query
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

            var filteredResults = patronazhistResults.Where(p => !IsNameBanned(p?.Emri, p?.Mbiemri)).ToList();

            return new PaginatedResult<PatronazhistResponse>
            {
                Items = filteredResults,
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
            // Order by relevance: exact matches first, then starts with, then contains
            .OrderBy(p => p.Mbiemer!.ToLower() == mbiemri ? 0 :
                         p.Mbiemer!.ToLower().StartsWith(mbiemri) ? 1 : 2)
            .ThenBy(p => p.Emer!.ToLower() == emri ? 0 :
                        p.Emer!.ToLower().StartsWith(emri) ? 1 : 2)
            .ThenBy(p => p.Mbiemer)
            .ThenBy(p => p.Emer)
            .Take(DefaultPageSize)
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
            // Order by relevance: exact matches first, then starts with, then contains
            .OrderBy(p => p.Mbiemri!.ToLower() == mbiemri ? 0 :
                         p.Mbiemri!.ToLower().StartsWith(mbiemri) ? 1 : 2)
            .ThenBy(p => p.Emri!.ToLower() == emri ? 0 :
                        p.Emri!.ToLower().StartsWith(emri) ? 1 : 2)
            .ThenBy(p => p.Mbiemri)
            .ThenBy(p => p.Emri)
            .Take(DefaultPageSize)
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
            // Order by relevance: exact matches first, then starts with, then contains
            .OrderBy(p => p.Mbiemri!.ToLower() == mbiemri ? 0 :
                         p.Mbiemri!.ToLower().StartsWith(mbiemri) ? 1 : 2)
            .ThenBy(p => p.Emri!.ToLower() == emri ? 0 :
                        p.Emri!.ToLower().StartsWith(emri) ? 1 : 2)
            .ThenBy(p => p.Mbiemri)
            .ThenBy(p => p.Emri)
            .Take(DefaultPageSize)
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
            // Order by relevance: exact matches first, then starts with, then contains
            .OrderBy(p => p.Mbiemri!.ToLower() == mbiemri ? 0 :
                         p.Mbiemri!.ToLower().StartsWith(mbiemri) ? 1 : 2)
            .ThenBy(p => p.Emri!.ToLower() == emri ? 0 :
                        p.Emri!.ToLower().StartsWith(emri) ? 1 : 2)
            .ThenBy(p => p.Mbiemri)
            .ThenBy(p => p.Emri)
            .Take(DefaultPageSize)
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

    private async Task<PaginatedResult<IResponseModel>> GetPersonResultsPaginated(string mbiemri, string emri, int pageNumber, int pageSize)
    {
        var query = _db.Person
            .AsNoTracking()
            .Where(p => p.Mbiemer != null && p.Emer != null)
            .Where(p => EF.Functions.Like(p.Mbiemer, $"%{mbiemri}%") &&
                       EF.Functions.Like(p.Emer, $"%{emri}%"))
            .OrderBy(p => p.Mbiemer!.ToLower() == mbiemri ? 0 :
                         p.Mbiemer!.ToLower().StartsWith(mbiemri) ? 1 : 2)
            .ThenBy(p => p.Emer!.ToLower() == emri ? 0 :
                        p.Emer!.ToLower().StartsWith(emri) ? 1 : 2)
            .ThenBy(p => p.Mbiemer)
            .ThenBy(p => p.Emer);

        var totalItems = await query.CountAsync();

        var results = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
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

        var filteredResults = results.Where(p => !IsNameBanned(p?.Emri, p?.Mbiemri))
            .Cast<IResponseModel>().ToList();

        return new PaginatedResult<IResponseModel>
        {
            Items = filteredResults,
            Pagination = new PaginationInfo
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            }
        };
    }

    private async Task<PaginatedResult<IResponseModel>> GetRrogatResultsPaginated(string mbiemri, string emri, int pageNumber, int pageSize)
    {
        var query = _db.Rrogat
            .AsNoTracking()
            .Where(p => p.Mbiemri != null && p.Emri != null)
            .Where(p => EF.Functions.Like(p.Mbiemri, $"%{mbiemri}%") &&
                       EF.Functions.Like(p.Emri, $"%{emri}%"))
            .OrderBy(p => p.Mbiemri!.ToLower() == mbiemri ? 0 :
                         p.Mbiemri!.ToLower().StartsWith(mbiemri) ? 1 : 2)
            .ThenBy(p => p.Emri!.ToLower() == emri ? 0 :
                        p.Emri!.ToLower().StartsWith(emri) ? 1 : 2)
            .ThenBy(p => p.Mbiemri)
            .ThenBy(p => p.Emri);

        var totalItems = await query.CountAsync();

        var results = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
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

        var filteredResults = results.Where(r => !IsNameBanned(r?.Emri, r?.Mbiemri))
            .Cast<IResponseModel>().ToList();

        return new PaginatedResult<IResponseModel>
        {
            Items = filteredResults,
            Pagination = new PaginationInfo
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            }
        };
    }

    private async Task<PaginatedResult<IResponseModel>> GetTargatResultsPaginated(string mbiemri, string emri, int pageNumber, int pageSize)
    {
        var query = _db.Targat
            .AsNoTracking()
            .Where(p => p.Mbiemri != null && p.Emri != null)
            .Where(p => EF.Functions.Like(p.Mbiemri, $"%{mbiemri}%") &&
                       EF.Functions.Like(p.Emri, $"%{emri}%"))
            .OrderBy(p => p.Mbiemri!.ToLower() == mbiemri ? 0 :
                         p.Mbiemri!.ToLower().StartsWith(mbiemri) ? 1 : 2)
            .ThenBy(p => p.Emri!.ToLower() == emri ? 0 :
                        p.Emri!.ToLower().StartsWith(emri) ? 1 : 2)
            .ThenBy(p => p.Mbiemri)
            .ThenBy(p => p.Emri);

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

        var filteredResults = results.Where(t => !IsNameBanned(t?.Emri, t?.Mbiemri))
            .Cast<IResponseModel>().ToList();

        return new PaginatedResult<IResponseModel>
        {
            Items = filteredResults,
            Pagination = new PaginationInfo
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            }
        };
    }

    private async Task<PaginatedResult<IResponseModel>> GetPatronazhistResultsPaginated(string mbiemri, string emri, int pageNumber, int pageSize)
    {
        var query = _db.Patronazhist
            .AsNoTracking()
            .Where(p => p.Mbiemri != null && p.Emri != null)
            .Where(p => EF.Functions.Like(p.Mbiemri, $"%{mbiemri}%") &&
                       EF.Functions.Like(p.Emri, $"%{emri}%"))
            .OrderBy(p => p.Mbiemri!.ToLower() == mbiemri ? 0 :
                         p.Mbiemri!.ToLower().StartsWith(mbiemri) ? 1 : 2)
            .ThenBy(p => p.Emri!.ToLower() == emri ? 0 :
                        p.Emri!.ToLower().StartsWith(emri) ? 1 : 2)
            .ThenBy(p => p.Mbiemri)
            .ThenBy(p => p.Emri);

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

        var filteredResults = results.Where(p => !IsNameBanned(p?.Emri, p?.Mbiemri))
            .Cast<IResponseModel>().ToList();

        return new PaginatedResult<IResponseModel>
        {
            Items = filteredResults,
            Pagination = new PaginationInfo
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            }
        };
    }
}