using System.ComponentModel.DataAnnotations;

namespace Kerko.Models;

public class Targat
{
    [Key]
    public int Id { get; set; }

    public string? NumriTarges { get; set; }
    public string? Marka { get; set; }
    public string? Modeli { get; set; }
    public string? Ngjyra { get; set; }
    public string? NumriPersonal { get; set; }
    public string? Emri { get; set; }
    public string? Mbiemri { get; set; }
}