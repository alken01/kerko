namespace Kerko.Models;

public class PersonResponse
{
    public string? Adresa { get; init; }
    public string? NrBaneses { get; init; }
    public string? Emri { get; init; }
    public string? Mbiemri { get; init; }
    public string? Atesi { get; init; }
    public string? Amesi { get; init; }
    public string? Datelindja { get; init; }
    public string? Vendlindja { get; init; }
    public string? Seksi { get; init; }
    public string? LidhjaMeKryefamiljarin { get; init; }
    public string? Qyteti { get; init; }
    public string? GjendjeCivile { get; init; }
    public string? Kombesia { get; init; }
}

public class RrogatResponse
{
    public string? NumriPersonal { get; init; }
    public string? Emri { get; init; }
    public string? Mbiemri { get; init; }
    public string? NIPT { get; init; }
    public string? DRT { get; init; }
    public int? PagaBruto { get; init; }
    public string? Profesioni { get; init; }
    public string? Kategoria { get; init; }
}

public class TargatResponse
{
    public string? NumriTarges { get; init; }
    public string? Marka { get; init; }
    public string? Modeli { get; init; }
    public string? Ngjyra { get; init; }
    public string? NumriPersonal { get; init; }
    public string? Emri { get; init; }
    public string? Mbiemri { get; init; }
}

public class PatronazhistResponse
{
    public string? NumriPersonal { get; init; }
    public string? Emri { get; init; }
    public string? Mbiemri { get; init; }
    public string? Atesi { get; init; }
    public string? Datelindja { get; init; }
    public string? QV { get; init; }
    public string? ListaNr { get; init; }
    public string? Tel { get; init; }
    public string? Emigrant { get; init; }
    public string? Country { get; init; }
    public string? ISigurte { get; init; }
    public string? Koment { get; init; }
    public string? Patronazhisti { get; init; }
    public string? Preferenca { get; init; }
    public string? Census2013Preferenca { get; init; }
    public string? Census2013Siguria { get; init; }
    public string? Vendlindja { get; init; }
    public string? Kompania { get; init; }
    public string? KodBanese { get; init; }
}

public class SearchResponse
{
    public List<PersonResponse>? Person { get; init; }
    public List<RrogatResponse>? Rrogat { get; init; }
    public List<TargatResponse>? Targat { get; init; }
    public List<PatronazhistResponse>? Patronazhist { get; init; }

    public SearchResponse()
    {
        Person = new List<PersonResponse>();
        Rrogat = new List<RrogatResponse>();
        Targat = new List<TargatResponse>();
        Patronazhist = new List<PatronazhistResponse>();
    }
}