using Laika.Models;

namespace Laika.Services;

public class EvaluacionService
{
    private readonly IAService _ia;
    public EvaluacionService(IAService ia) => _ia = ia;

    // Cada respuesta va de 0 (nunca) a 3 (siempre). La pregunta 8 es de seguridad personal.
    public async Task<Evaluacion> CalcularAsync(List<int> respuestas)
    {
        var r = respuestas.Select(x => Math.Clamp(x, 0, 3)).ToList();
        var puntaje = r.Sum();
        var nivel = puntaje <= 6 ? "Bajo" : puntaje <= 14 ? "Medio" : "Alto";
        var alerta = r.Count >= 8 && r[7] > 0;

        var recomendacion = RecomendacionBase(nivel);
        if (!_ia.ModoPrueba)
        {
            try
            {
                recomendacion = await _ia.GenerarAsync(
                    $"Una persona hizo un test anónimo de bienestar escolar y su nivel de riesgo es {nivel} " +
                    $"({puntaje}/{r.Count * 3}). Escribe 3 recomendaciones breves, cálidas y concretas.");
            }
            catch { /* se queda la recomendación base */ }
        }

        return new Evaluacion { Puntaje = puntaje, NivelRiesgo = nivel, Recomendacion = recomendacion, AlertaSeguridad = alerta };
    }

    private static string RecomendacionBase(string nivel) => nivel switch
    {
        "Bajo"  => "Vas bien. Sigue cuidando tus amistades, descansa y habla de lo que sientes. Si algo cambia, aquí estaremos.",
        "Medio" => "Hay señales que vale la pena atender. Platica con un adulto de confianza o con orientación, y practica los ejercicios de la sección Bienestar.",
        _       => "Lo que vives es pesado y no tienes que cargarlo solo/a. Habla pronto con orientación escolar o un adulto de confianza, y revisa la sección Ayuda."
    };
}
