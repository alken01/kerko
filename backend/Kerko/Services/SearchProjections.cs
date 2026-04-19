using System.Linq.Expressions;
using Kerko.Models;

namespace Kerko.Services;

/// <summary>
/// EF Core projection expressions for each searchable table. Kept out of
/// <see cref="SearchService"/> so the orchestration stays readable.
/// </summary>
internal static class SearchProjections
{
    public static readonly Expression<Func<Person, PersonResponse>> Person = p => new PersonResponse
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
    };

    public static readonly Expression<Func<Rrogat, RrogatResponse>> Rrogat = r => new RrogatResponse
    {
        NumriPersonal = r.NumriPersonal,
        Emri = r.Emri,
        Mbiemri = r.Mbiemri,
        NIPT = r.NIPT,
        DRT = r.DRT,
        PagaBruto = r.PagaBruto,
        Profesioni = r.Profesioni,
        Kategoria = r.Kategoria
    };

    public static readonly Expression<Func<Targat, TargatResponse>> Targat = t => new TargatResponse
    {
        NumriTarges = t.NumriTarges,
        Marka = t.Marka,
        Modeli = t.Modeli,
        Ngjyra = t.Ngjyra,
        NumriPersonal = t.NumriPersonal,
        Emri = t.Emri,
        Mbiemri = t.Mbiemri
    };

    public static readonly Expression<Func<Patronazhist, PatronazhistResponse>> Patronazhist = p => new PatronazhistResponse
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
    };
}
