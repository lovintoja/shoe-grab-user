using ShoeGrabCommonModels.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ShoeGrabMonolith.Extensions;

public static class AppExtension
{
    public static void ApplyMigrations(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            using (var context = scope.ServiceProvider.GetRequiredService<UserContext>())
            {
                try
                {
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating the database");
                    throw;
                }
            }
        }
    }
}
