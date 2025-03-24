using System.ComponentModel.DataAnnotations;

namespace Kerko.Models;

public class Patronazhist
{
    [Key]
    public int Id { get; set; }
    
    public string? NumriPersonal { get; set; }
    public string? Emri { get; set; }
    public string? Mbiemri { get; set; }
    public string? Atesi { get; set; }
    public string? Datelindja { get; set; }
    public string? QV { get; set; }
    public string? ListaNr { get; set; }
    public string? Tel { get; set; }
    public string? Emigrant { get; set; }
    public string? Country { get; set; }
    public string? ISigurte { get; set; }
    public string? Koment { get; set; }
    public string? Patronazhisti { get; set; }
    public string? Preferenca { get; set; }
    public string? Census2013Preferenca { get; set; }
    public string? Census2013Siguria { get; set; }
    public string? Vendlindja { get; set; }
    public string? Kompania { get; set; }
    public string? KodBanese { get; set; }
}