using System.Net.Sockets;
using SafeFlow.API.Shared.Infrastructure.Interfaces.ASP.Configuration.Extensions;
using SafeFlow.API.Shared.Infrastructure.Persistence.EFC.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.AddSafeFlowServices();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await SafeFlowDbSeeder.InitializeAsync(context, app.Configuration);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("SafeFlowFrontend");
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.MapControllers();

try
{
    app.Run();
}
catch (IOException ex) when (IsPortInUse(ex))
{
    PrintPortInUseHelp();
    return 1;
}

return 0;

static bool IsPortInUse(Exception ex)
{
    for (var current = ex; current != null; current = current.InnerException)
    {
        if (current is SocketException { SocketErrorCode: SocketError.AddressAlreadyInUse })
            return true;
        if (current.Message.Contains("address already in use", StringComparison.OrdinalIgnoreCase)
            || current.Message.Contains("Solo se permite un uso de cada dirección", StringComparison.OrdinalIgnoreCase))
            return true;
    }

    return false;
}

static void PrintPortInUseHelp()
{
    Console.Error.WriteLine();
    Console.Error.WriteLine("ERROR: El puerto 5115 ya esta en uso (otra instancia de safeflow-backend sigue activa).");
    Console.Error.WriteLine("Solucion: cierra la terminal anterior o ejecuta desde la raiz del repo:");
    Console.Error.WriteLine("  run-api.bat");
    Console.Error.WriteLine("  powershell -File run-api.ps1");
    Console.Error.WriteLine("O manualmente: taskkill /F /IM safeflow-backend.exe");
    Console.Error.WriteLine();
}
