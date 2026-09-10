using AstralNexus.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace AstralNexus.Web.Data;

public static class DatabaseBootstrapper
{
    public static async Task InitializeAsync(IServiceProvider services, ILogger logger)
    {
        const int attempts = 45;
        Exception? lastError = null;

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            try
            {
                using var scope = services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var passwords = scope.ServiceProvider.GetRequiredService<PasswordService>();
                var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

                await db.Database.EnsureCreatedAsync();
                await DatabaseSeeder.SeedAsync(db, passwords, environment);

                logger.LogInformation("Astral Nexus SQL Server baza je spremna.");
                return;
            }
            catch (Exception ex) when (attempt < attempts)
            {
                lastError = ex;
                logger.LogWarning(
                    "SQL Server još nije spreman (pokušaj {Attempt}/{Attempts}). Novi pokušaj za 2 sekunde...",
                    attempt,
                    attempts);
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }

        throw new InvalidOperationException(
            "Nije moguće povezati se na Microsoft SQL Server. Pokrenite 'docker compose up -d' i pokušajte ponovo.",
            lastError);
    }
}
