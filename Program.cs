using Laika.Models;
using Laika.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddHttpClient<IAService>();
builder.Services.AddScoped<EvaluacionService>();
builder.Services.AddSingleton<ReporteService>();

var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

// ---- API que consume el JS del frontend ----
app.MapPost("/api/chat", async (ChatPeticion p, IAService ia) =>
    Results.Ok(await ia.ResponderAsync(p.Mensajes ?? new(), p.Perfil, p.Pagina)));

app.MapPost("/api/evaluacion", async (EvaluacionPeticion p, EvaluacionService svc) =>
    Results.Ok(await svc.CalcularAsync(p.Respuestas ?? new())));

app.MapPost("/api/reporte", (ReportePeticion p, ReporteService svc) =>
{
    if (string.IsNullOrWhiteSpace(p.Descripcion) || p.Descripcion.Trim().Length < 10)
        return Results.BadRequest(new { error = "Cuéntanos un poco más (mínimo 10 caracteres)." });
    return Results.Ok(new { codigo = svc.Registrar(p.Tipo ?? "otro", p.Descripcion.Trim()) });
});

app.MapGet("/api/emociones", () => Results.Ok(Emocion.Todas));

app.Run();
