using Microsoft.EntityFrameworkCore;
using Kerko.Infrastructure;
using Kerko.Models;

namespace Kerko.Services;

public interface ISearchService
{
    Task<SearchResponse> KerkoAsync(string? mbiemri, string? emri);
    Task<SearchResponse> TargatAsync(string? numriTarges);
}

public class SearchService : ISearchService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<SearchService> _logger;

    public SearchService(ApplicationDbContext db, ILogger<SearchService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<SearchResponse> KerkoAsync(string? mbiemri, string? emri)
    {
        if (string.IsNullOrEmpty(mbiemri) || string.IsNullOrEmpty(emri))
        {
            throw new ArgumentException("Emri dhe mbiemri nuk mund te jene bosh");
        }

        var personResults = await _db.Person
            .Where(p => p.Mbiemer != null && p.Mbiemer.ToLower().Contains(mbiemri.ToLower()) &&
                        p.Emer != null && p.Emer.ToLower().Contains(emri.ToLower()))
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

        var rrogatResults = await _db.Rrogat
            .Where(p => p.Mbiemri != null && p.Mbiemri.ToLower().Contains(mbiemri.ToLower()) &&
                        p.Emri != null && p.Emri.ToLower().Contains(emri.ToLower()))
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

        var targatResults = await _db.Targat
            .Where(p => p.Mbiemri != null && p.Mbiemri.ToLower().Contains(mbiemri.ToLower()) &&
                        p.Emri != null && p.Emri.ToLower().Contains(emri.ToLower()))
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

        var patronazhistResults = await _db.Patronazhist
            .Where(p => p.Mbiemri != null && p.Mbiemri.ToLower().Contains(mbiemri.ToLower()) &&
                        p.Emri != null && p.Emri.ToLower().Contains(emri.ToLower()))
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

        return new SearchResponse
        {
            Person = personResults,
            Rrogat = rrogatResults,
            Targat = targatResults,
            Patronazhist = patronazhistResults
        };
    }

    public async Task<SearchResponse> TargatAsync(string? numriTarges)
    {
        if (string.IsNullOrEmpty(numriTarges))
        {
            throw new ArgumentException("Numri i targes nuk mund te jene bosh");
        }

        var targatResults = await _db.Targat
            .Where(t => t.NumriTarges == numriTarges)
            .Select(t => new TargatResponse
            {
                NumriTarges = t.NumriTarges,
                Marka = t.Marka,
                Modeli = t.Modeli,
            })
            .ToListAsync();

        return new SearchResponse
        {
            Targat = targatResults
        };
    }
} 