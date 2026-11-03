namespace Laika.Models;

public class Reporte
{
    public string Codigo { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}

public record ReportePeticion(string? Tipo, string? Descripcion);
