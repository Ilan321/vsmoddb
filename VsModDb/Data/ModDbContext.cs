using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VsModDb.Data.Entities;

namespace VsModDb.Data;

public class ModDbContext(DbContextOptions<ModDbContext> options) : IdentityDbContext<User>(options)
{
    public required DbSet<Mod> Mods { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ModDbContext).Assembly);
    }
}
