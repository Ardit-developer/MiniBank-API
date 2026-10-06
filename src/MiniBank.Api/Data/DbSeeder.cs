using MiniBank.Api.Data;

namespace MiniBank.Api.Data;

public static class DbSeeder
{
    public static Task SeedAsync(MiniBankDbContext context) => DbInitializer.SeedAsync(context);
}
