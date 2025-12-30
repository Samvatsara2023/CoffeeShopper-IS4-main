namespace Server
{
    using IdentityServer4.EntityFramework.DbContexts;
    using IdentityServer4.EntityFramework.Options;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;

    public class PersistedGrantDbContextFactory
        : IDesignTimeDbContextFactory<PersistedGrantDbContext>
    {
        public PersistedGrantDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PersistedGrantDbContext>();

            optionsBuilder.UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;Database=CoffeeShopperDb;Trusted_Connection=True",
                b => b.MigrationsAssembly(typeof(Program).Assembly.GetName().Name));

            return new PersistedGrantDbContext(
                optionsBuilder.Options,
                new OperationalStoreOptions());
        }
    }
}
