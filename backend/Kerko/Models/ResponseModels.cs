namespace Kerko.Models;

public class PaginationInfo
{
    public int CurrentPage { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
}

public class PaginatedResult<T>
{
    public List<T> Items { get; init; } = new();
    public PaginationInfo Pagination { get; init; } = new();
}

public interface IResponseModel
{
    string? Emri { get; init; }
    string? Mbiemri { get; init; }
}
public class PersonResponse : IResponseModel
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

public class RrogatResponse : IResponseModel
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

public class TargatResponse : IResponseModel
{
    public string? NumriTarges { get; init; }
    public string? Marka { get; init; }
    public string? Modeli { get; init; }
    public string? Ngjyra { get; init; }
    public string? NumriPersonal { get; init; }
    public string? Emri { get; init; }
    public string? Mbiemri { get; init; }
}

public class PatronazhistResponse : IResponseModel
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
    public PaginatedResult<PersonResponse> Person { get; init; } = new();
    public PaginatedResult<RrogatResponse> Rrogat { get; init; } = new();
    public PaginatedResult<TargatResponse> Targat { get; init; } = new();
    public PaginatedResult<PatronazhistResponse> Patronazhist { get; init; } = new();
}