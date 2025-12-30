namespace Server
{
    using IdentityServer4.EntityFramework.DbContexts;
    using IdentityServer4.EntityFramework.Options;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;

    public class ConfigurationDbContextFactory
        : IDesignTimeDbContextFactory<ConfigurationDbContext>
    {
        public ConfigurationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ConfigurationDbContext>();

            optionsBuilder.UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;Database=CoffeeShopperDb;Trusted_Connection=True",
                b => b.MigrationsAssembly(typeof(Program).Assembly.GetName().Name));

            return new ConfigurationDbContext(
                optionsBuilder.Options,
                new ConfigurationStoreOptions());
        }
    }
}
