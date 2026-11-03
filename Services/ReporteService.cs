using System.Collections.Concurrent;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.Json;
using Laika.Models;

namespace Laika.Services;

public class ReporteService
{
    private readonly ConcurrentDictionary<string, Reporte> _reportes = new();
    private readonly ILogger<ReporteService> _log;
    private readonly IConfiguration _cfg;
    private readonly string _archivo;
    private static readonly object _lock = new();

    public ReporteService(ILogger<ReporteService> log, IConfiguration cfg, IWebHostEnvironment env)
    {
        _log = log; _cfg = cfg;
        var carpeta = Path.Combine(env.ContentRootPath, "App_Data");
        Directory.CreateDirectory(carpeta);
        _archivo = Path.Combine(carpeta, "reportes.jsonl");
    }

    public string Registrar(string tipo, string descripcion)
    {
        string codigo;
        var reporte = new Reporte { Tipo = tipo, Descripcion = descripcion };
        do { codigo = $"LAIKA-{RandomNumberGenerator.GetInt32(1000, 10000)}"; reporte.Codigo = codigo; }
        while (!_reportes.TryAdd(codigo, reporte));

        // 1) Respaldo local: así ningún reporte se pierde aunque falle el correo.
        lock (_lock) File.AppendAllText(_archivo, JsonSerializer.Serialize(reporte) + Environment.NewLine);

        // 2) Correo a orientación (si está configurado). No se guarda IP ni datos personales.
        _ = Task.Run(() => EnviarCorreoAsync(reporte));
        return codigo;
    }

    private async Task EnviarCorreoAsync(Reporte r)
    {
        var destino = _cfg["Reportes:CorreoOrientacion"];
        var host = _cfg["Reportes:Smtp:Host"];
        var usuario = _cfg["Reportes:Smtp:Usuario"];
        var clave = _cfg["Reportes:Smtp:Clave"];
        if (string.IsNullOrWhiteSpace(destino) || string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(clave))
        {
            _log.LogWarning("Reporte {Codigo} guardado en App_Data/reportes.jsonl. Correo NO configurado.", r.Codigo);
            return;
        }
        try
        {
            using var smtp = new SmtpClient(host, int.Parse(_cfg["Reportes:Smtp:Puerto"] ?? "587"))
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(usuario, clave)
            };
            var msg = new MailMessage(usuario, destino)
            {
                Subject = $"[Laika] Nuevo reporte anónimo {r.Codigo} ({r.Tipo})",
                Body = $"Código: {r.Codigo}\nTipo: {r.Tipo}\nFecha (UTC): {r.Fecha:u}\n\n{r.Descripcion}"
            };
            await smtp.SendMailAsync(msg);
            _log.LogInformation("Reporte {Codigo} enviado por correo a orientación.", r.Codigo);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "No se pudo enviar el correo del reporte {Codigo}.", r.Codigo);
        }
    }
}
