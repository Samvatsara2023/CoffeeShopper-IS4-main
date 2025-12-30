# CoffeeShopper IdentityServer4 EF Core Migrations Guide

This README provides **step-by-step instructions** for setting up IdentityServer4 with EF Core, creating migrations, and updating the database for the `Server` project using LocalDB.

---
# Prepare Api Project:
## Migrations In Api Project:
```
Add-Migration InitialApplicationDbContextMigration
Update-Database
```
## Insert Dummy data in database
```
migrationBuilder.Sql($"INSERT INTO CoffeeShops (Name, OpeningHours, Address) VALUES ('PJ''s Coffee of New Orleans',                 '9-5 Mon-Sat',              '9079 West Locust St. Buffalo, NY 14221')");
migrationBuilder.Sql($"INSERT INTO CoffeeShops (Name, OpeningHours, Address) VALUES ('Victory Sweet Shop/Victory Garden Cafe',      '7AM-7PM Mon-Fri',          '51 W. Myers Avenue Brooklyn, NY 11201')");
migrationBuilder.Sql($"INSERT INTO CoffeeShops (Name, OpeningHours, Address) VALUES ('Kaffe Landskap NYC (South)',                  '24/7',                     '25 Whitemarsh Court Jamaica, NY 11435')");
migrationBuilder.Sql($"INSERT INTO CoffeeShops (Name, OpeningHours, Address) VALUES ('Culture Espresso',                            '9-5 Mon-Fri',              '8945 Newbridge Street New York, NY 10024')");
migrationBuilder.Sql($"INSERT INTO CoffeeShops (Name, OpeningHours, Address) VALUES ('Manon Cafe',                                  '6AM-4PM Mon-Sat',          '16 Woodsman Lane Jamaica, NY 11432')");
migrationBuilder.Sql($"INSERT INTO CoffeeShops (Name, OpeningHours, Address) VALUES ('Café-Flor',                                   '7:30AM-5:30PM Mon-Sat',    '21 Airport St. Brooklyn, NY 11221')");
migrationBuilder.Sql($"INSERT INTO CoffeeShops (Name, OpeningHours, Address) VALUES ('Bluestone Lane Times Square Coffee Shop',     '4:30AM-2:00PM Mon-Sun',    '9611 Bradford Dr. Flushing, NY 11354')");
migrationBuilder.Sql($"INSERT INTO CoffeeShops (Name, OpeningHours, Address) VALUES ('In Common NYC Cafe',                          '24/7',                     '339 1st Ave. Brooklyn, NY 11228')");
migrationBuilder.Sql($"INSERT INTO CoffeeShops (Name, OpeningHours, Address) VALUES ('Bird & Branch',                               '6AM-7PM Mon-Fri',          '393 Pearl Street Buffalo, NY 14221')");
migrationBuilder.Sql($"INSERT INTO CoffeeShops (Name, OpeningHours, Address) VALUES ('Coffee Project New York',                     '8-6 Mon-Sat',              '7998 Vermont Street Astoria, NY 11106')");
```

## Table Created in LocalDb
```
dbo.CoffeeShops
```

## 1. Prerequisites

* **.NET SDK** installed
* `Server` project references:

```text
Microsoft.EntityFrameworkCore
Microsoft.EntityFrameworkCore.SqlServer
Microsoft.EntityFrameworkCore.Tools
IdentityServer4.EntityFramework
```

* **LocalDB connection string** in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=CoffeeShopperDb;Trusted_Connection=True"
}
```

* Knowledge of **Package Manager Console (PMC)** or **dotnet CLI** for EF Core commands.

---

## 2. Configure IdentityServer4 in `Program.cs` or `Startup.cs`

```csharp
var assembly = typeof(Program).Assembly.GetName().Name;
var defaultConnString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddIdentityServer()
    .AddAspNetIdentity<IdentityUser>()
    .AddConfigurationStore(options =>
    {
        options.ConfigureDbContext = b =>
            b.UseSqlServer(defaultConnString, opt => opt.MigrationsAssembly(assembly));
    })
    .AddOperationalStore(options =>
    {
        options.ConfigureDbContext = b =>
            b.UseSqlServer(defaultConnString, opt => opt.MigrationsAssembly(assembly));
    })
    .AddDeveloperSigningCredential();
```

**Explanation:**

* `AddConfigurationStore` → registers `ConfigurationDbContext` for clients, resources, and scopes
* `AddOperationalStore` → registers `PersistedGrantDbContext` for tokens, consents, and device codes
* `opt.MigrationsAssembly(assembly)` → ensures migrations are created in your project assembly

---

## 3. Problem: EF Core Migrations Not Found

When running:

```powershell
Add-Migration InitialPersistedGrantMigration -Context PersistedGrantDbContext
```

You may encounter:

```text
No DbContext named 'PersistedGrantDbContext' was found.
Service type: IUserClaimsPrincipalFactory`1 not registered
```

**Cause:**

* IdentityServer4 DbContexts are **internal to the package**
* EF Core tooling **cannot discover them at design time**
* Requires **explicit DbContext factories**

---

## 4. Add Design-Time DbContext Factories

### 4.1 PersistedGrantDbContextFactory

```csharp
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
```

### 4.2 ConfigurationDbContextFactory

```csharp
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
```

📌 Place both classes **in the Server project** (any folder).

---

## 5. Create Migrations

### 5.1 PersistedGrantDbContext Migration

```powershell
Add-Migration InitialPersistedGrantMigration -Context PersistedGrantDbContext -Project Server -StartupProject Server -OutputDir Migrations
```

### 5.2 ConfigurationDbContext Migration

```powershell
Add-Migration InitialConfigurationMigration -Context ConfigurationDbContext -Project Server -StartupProject Server -OutputDir Migrations
```

**Notes:**

* Separate migrations are required for each DbContext
* Migrations will be saved under `Data/Migrations/IdentityServer/`

---

## 6. Update Database

### 6.1 Apply PersistedGrantDbContext

```powershell
Update-Database -Context PersistedGrantDbContext -Project Server -StartupProject Server
```

### 6.2 Apply ConfigurationDbContext

```powershell
Update-Database -Context ConfigurationDbContext -Project Server -StartupProject Server
```

**Result:**

* Tables for IdentityServer4 will be created in **CoffeeShopperDb**
* Examples:

  **PersistedGrantDbContext** → `PersistedGrants`, `DeviceCodes`, `Keys`
  **ConfigurationDbContext** → `Clients`,`ClientScopes`, `ClientSecrets`,`ClientClaims`,`ClientCrosOrigin`,`ClientGrantTypes`,`ClientIdPRestrictions`,`ClientPostLogoutRedirectUris`,`ClientProperties`,`ClientRedirectUris`, `ApiScopes`,`ApiScopeClaims`,`ApiScopeProperties`, `ApiResources`,`ApiResourceClaims`, `ApiResourceProperties`,`ApiResourceScopes`,`ApiResourceSecrets`, `IdentityResources`,`IdentityResourceClaims`,`IdentityResourceProperties`.

---

## 7. Common Warnings

* **IUserClaimsPrincipalFactory warning** during migration is harmless
* Fully avoided after adding **design-time DbContext factories**

---

## 8. Optional Tips

* Organize migrations clearly under `Data/Migrations/IdentityServer/`
* Always specify **-Context** for IdentityServer migrations
* Seed IdentityServer clients, API scopes, and resources after updating the database
* Use **AddDeveloperSigningCredential()** only for development

---

## 9. Summary Workflow

1. Configure IdentityServer4 with `AddConfigurationStore` and `AddOperationalStore`
2. Add **design-time DbContext factories** for EF Core
3. Create migrations for both DbContexts separately
4. Run `Update-Database` for both contexts
5. Verify tables are created in `CoffeeShopperDb`

## 10. ASPNetIdentityMigrations script
```
Add-Migration InitialAspNetIdentityMigration -Context AspNetIdentityDbContext
Update-Database -Context AspNetIdentityDbContext

```

**Result:**

* Tables for ASPNetIdentity will be created in **CoffeeShopperDb**
* Examples:

  `ApiScopeClaims`,`ApiScopeProperties`,`ApiScopes`,`AspNetRoleClaims`,`AspNetRoles`,`AspNetUserClaims`,`AspNetUserLogins`,`AspNetUserRoles`,`AspNetUsers`,`AspNetUserTokens`.

---


## 11. Seed Data Scripts
```
dotnet run C:\Users\AvaniSoam\Downloads\CoffeeShopper-IS4-main-main\CoffeeShopper-IS4-main-main\Server\bin\Debug\net9.0\Server.exe /seed --project Server
```
Url: https://localhost:5443/.well-known/openid-configuration