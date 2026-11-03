namespace Laika.Models;

public record AccionIA(string Texto, string Url);

public class MensajeIA
{
    public string Rol { get; set; } = "user";        // "user" | "assistant"
    public string Contenido { get; set; } = "";
    public bool Urgencia { get; set; }                // true si se detectó una situación de riesgo
    public List<AccionIA> Acciones { get; set; } = new();   // botones de atajo (reportar, ayuda, etc.)
}

// Perfil: "estudiante" | "docente" | null.  Pagina: ruta que está viendo la persona.
public record ChatPeticion(List<MensajeIA>? Mensajes, string? Perfil, string? Pagina);
