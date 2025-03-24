using System.ComponentModel.DataAnnotations;

namespace Kerko.Models;

public class Person
{
    [Key]
    public int Id { get; set; }
    
    public string? Adresa { get; set; }
    public string? NrBaneses { get; set; }
    public string? Emer { get; set; }
    public string? Mbiemer { get; set; }
    public string? Atesi { get; set; }
    public string? Amesi { get; set; }
    public string? Datelindja { get; set; }
    public string? Vendlindja { get; set; }
    public string? Seksi { get; set; }
    public string? LidhjaMeKryefamiljarin { get; set; }
    public string? Qyteti { get; set; }
    public string? GjendjeCivile { get; set; }
    public string? Kombesia { get; set; }
}