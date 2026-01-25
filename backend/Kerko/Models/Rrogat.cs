using System.ComponentModel.DataAnnotations;

namespace Kerko.Models;

public class Rrogat
{
    [Key]
    public int Id { get; set; }

    public string? NumriPersonal { get; set; }
    public string? Emri { get; set; }
    public string? Mbiemri { get; set; }
    public string? NIPT { get; set; }
    public string? DRT { get; set; }
    public int? PagaBruto { get; set; }
    public string? Profesioni { get; set; }
    public string? Kategoria { get; set; }
}