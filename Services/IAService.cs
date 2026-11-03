using System.Text;
using System.Text.Json;
using Laika.Models;

namespace Laika.Services;

public class IAService
{
    private readonly HttpClient _http;
    private readonly string? _apiKey;
    private readonly string _modelo;

    private const string SistemaBase = """
        Eres Laika, una compañera virtual empática dentro de una plataforma anónima para estudiantes y docentes de México.
        Temas: bullying (incluido el que sufren los maestros), drogas, violencia, ansiedad, estrés e inteligencia emocional.

        Cómo conversar:
        - Adáptate a la persona: usa su idioma, su tono (formal o informal) y su nivel. Si escribe corto, responde corto; si se abre y cuenta mucho, profundiza.
        - Primero reconoce lo que siente, después ayuda. Nunca minimices ("no es para tanto") ni juzgues.
        - Máximo una pregunta por mensaje. Da pasos pequeños y concretos, no listas enormes.
        - Habla natural, sin sonar a folleto. No uses diagnósticos ni te presentes como psicóloga.
        - No pidas nombre, escuela ni datos personales: la plataforma es anónima.
        - Nunca des instrucciones para conseguir, usar o fabricar drogas, armas o para hacerse daño.

        La plataforma tiene estas secciones y puedes recomendarlas cuando encajen: Evaluación (/evaluacion, test anónimo),
        Reportar (/reporte, reporte anónimo con código de seguimiento), Bienestar (/bienestar, respiración y ejercicios),
        Emociones (/emociones, rueda de emociones), Prevención (/prevencion) y Ayuda (/ayuda, teléfonos).

        Si hay riesgo (daño propio, abuso, amenazas, violencia inmediata): mantén la calma, valida, anima a hablar YA con un
        adulto de confianza y menciona el 911 y la Línea de la Vida (800 911 2000). No termines la conversación; sigue acompañando.
        """;

    private static readonly string[] PalabrasUrgentes =
    {
        "suicid", "quitarme la vida", "matarme", "no quiero vivir", "hacerme daño", "cortarme", "me corto",
        "me pegan", "me amenazan", "me golpean", "abuso", "arma"
    };

    public IAService(HttpClient http, IConfiguration cfg)
    {
        _http = http;
        _apiKey = cfg["OpenAI:ApiKey"];
        _modelo = cfg["OpenAI:Model"] ?? "gpt-4o-mini";
    }

    /// <summary>Sin API Key la app responde con mensajes predefinidos.</summary>
    public bool ModoPrueba => string.IsNullOrWhiteSpace(_apiKey);

    public async Task<MensajeIA> ResponderAsync(List<MensajeIA> historial, string? perfil = null, string? pagina = null)
    {
        var ultimo = historial.LastOrDefault(m => m.Rol == "user")?.Contenido ?? "";
        var urgente = PalabrasUrgentes.Any(p => ultimo.Contains(p, StringComparison.OrdinalIgnoreCase));

        string texto;
        if (ModoPrueba) texto = RespuestaPrueba(ultimo, perfil);
        else
        {
            try { texto = await LlamarAsync(historial, ConstruirSistema(perfil, pagina)); }
            catch { texto = "Ahora mismo no puedo conectarme, pero aquí estoy contigo. ¿Quieres contarme más?"; }
        }

        if (urgente)
            texto += "\n\nTu seguridad es lo más importante. Habla ya con un adulto de confianza o llama al 911. " +
                     "También puedes marcar a la Línea de la Vida: 800 911 2000.";

        return new MensajeIA { Rol = "assistant", Contenido = texto, Urgencia = urgente, Acciones = Acciones(ultimo + " " + texto, urgente) };
    }

    public async Task<string> GenerarAsync(string prompt) =>
        await LlamarAsync(new List<MensajeIA> { new() { Rol = "user", Contenido = prompt } }, SistemaBase);

    private static string ConstruirSistema(string? perfil, string? pagina)
    {
        var sb = new StringBuilder(SistemaBase);
        if (perfil == "docente")
            sb.Append("\n\nLa persona es DOCENTE: trátala con respeto profesional; reconoce que el bullying hacia maestros existe y es real; sugiere registrar hechos con fecha y acudir a dirección u orientación.");
        else if (perfil == "estudiante")
            sb.Append("\n\nLa persona es ESTUDIANTE: usa un lenguaje cercano y sencillo; anímala a apoyarse en un adulto de confianza.");
        if (!string.IsNullOrWhiteSpace(pagina) && pagina.Length < 60)
            sb.Append($"\n\nLa persona está viendo la sección \"{pagina}\" del sitio; úsalo solo si ayuda.");
        return sb.ToString();
    }

    // Botones de atajo según el tema (funciona con y sin API Key)
    private static List<AccionIA> Acciones(string t, bool urgente)
    {
        t = t.ToLowerInvariant();
        var a = new List<AccionIA>();
        if (urgente) a.Add(new("Ver teléfonos de ayuda", "/ayuda"));
        if (new[] { "bullying", "acoso", "amenaz", "violencia", "droga", "burl", "golpe" }.Any(t.Contains))
            a.Add(new("Hacer un reporte anónimo", "/reporte"));
        if (new[] { "triste", "ansios", "estres", "estrés", "nervios", "llor", "miedo" }.Any(t.Contains))
        {
            a.Add(new("Ejercicio de respiración", "/bienestar"));
            a.Add(new("Rueda de emociones", "/emociones"));
        }
        if (new[] { "test", "evaluaci", "cómo estoy", "como estoy" }.Any(t.Contains))
            a.Add(new("Hacer la evaluación", "/evaluacion"));
        return a.DistinctBy(x => x.Url).Take(3).ToList();
    }

    private async Task<string> LlamarAsync(IEnumerable<MensajeIA> historial, string sistema)
    {
        var msgs = new List<object> { new { role = "system", content = sistema } };
        msgs.AddRange(historial.TakeLast(16).Select(m => new
        {
            role = m.Rol == "assistant" ? "assistant" : "user",
            content = m.Contenido
        }));

        var cuerpo = JsonSerializer.Serialize(new { model = _modelo, messages = msgs, max_tokens = 450, temperature = 0.8 });
        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
        {
            Content = new StringContent(cuerpo, Encoding.UTF8, "application/json")
        };
        req.Headers.Authorization = new("Bearer", _apiKey);

        using var res = await _http.SendAsync(req);
        res.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
    }

    private static string RespuestaPrueba(string t, string? perfil)
    {
        t = t.ToLowerInvariant();
        var doc = perfil == "docente";
        if (t.Contains("maestr") || t.Contains("profe") || t.Contains("docente") || (doc && (t.Contains("alumn") || t.Contains("grupo"))))
            return "Los docentes también merecen respeto y apoyo; lo que vives importa. Te sugiero anotar los hechos con fecha y hablar con dirección u orientación. ¿Quieres contarme qué ha pasado?";
        if (t.Contains("bullying") || t.Contains("acoso") || t.Contains("molest") || t.Contains("burl"))
            return "Siento mucho que estés pasando por eso. No es tu culpa y mereces sentirte seguro/a. ¿Quieres contarme qué ha pasado? También puedes hacer un reporte anónimo.";
        if (t.Contains("droga") || t.Contains("vape") || t.Contains("alcohol") || t.Contains("fum"))
            return "Gracias por confiar en mí. Hablar de esto es un paso valiente. Un adulto de confianza puede ayudarte sin juzgarte. ¿Qué es lo que más te preocupa?";
        if (t.Contains("triste") || t.Contains("ansios") || t.Contains("estres") || t.Contains("estrés") || t.Contains("solo") || t.Contains("llor"))
            return "Gracias por decírmelo. Lo que sientes es válido. Prueba respirar lento: 4 segundos al inhalar y 6 al soltar. Si quieres, cuéntame qué pasó hoy.";
        if (t.Contains("hola") || t.Contains("buenas"))
            return "¡Hola! Soy Laika. Este es un espacio seguro y anónimo. ¿Cómo te sientes hoy?";
        return "Te escucho. Cuéntame más, sin prisa. Estoy aquí para acompañarte.\n\n(Estoy en Modo Prueba: con una API Key podré conversar de forma mucho más libre.)";
    }
}
