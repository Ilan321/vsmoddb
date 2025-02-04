using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VsModDb.Data.Entities;
using VsModDb.Data.Entities.Account;
using VsModDb.Data.Entities.Mods;

namespace VsModDb.Data;

public class ModDbContext(DbContextOptions<ModDbContext> options) : IdentityDbContext<User>(options)
{
    public required DbSet<AccountLinkRequest> AccountLinkRequests { get; init; }

    public required DbSet<Mod> Mods { get; init; }
    public required DbSet<ModComment> ModComments { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ModDbContext).Assembly);
    }
}
