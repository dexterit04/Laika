namespace Laika.Models;

public class Evaluacion
{
    public int Puntaje { get; set; }
    public string NivelRiesgo { get; set; } = "Bajo";   // Bajo | Medio | Alto
    public string Recomendacion { get; set; } = "";
    public bool AlertaSeguridad { get; set; }
}

public record EvaluacionPeticion(List<int>? Respuestas);
