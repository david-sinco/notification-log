using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NotificationLog.NotificationService.Infrastructure.Persistence.Context;

/// <summary>
/// Permite a las herramientas de diseño de EF Core (dotnet ef) crear el DbContext
/// sin depender de la cadena de conexión que Aspire inyecta solo en tiempo de ejecución.
/// </summary>
public sealed class NotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
{
    public NotificationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NotificationDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=NotificationLog;Trusted_Connection=True;TrustServerCertificate=True");

        return new NotificationDbContext(optionsBuilder.Options);
    }
}
