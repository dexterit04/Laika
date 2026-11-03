# Laika
Plataforma web anónima y empática para estudiantes y docentes (ASP.NET Core Razor Pages, .NET 10).

## Ejecutar
    cd Laika
    dotnet watch run

## Activar la IA real (opcional)
Sin clave, el chat funciona en "Modo Prueba" con respuestas predefinidas.
    dotnet user-secrets set "OpenAI:ApiKey" "TU_API_KEY"
No pegues la clave dentro de appsettings.json ni la subas a GitHub.
