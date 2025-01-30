using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VsModDb.Extensions;

namespace VsModDb.Data.Entities.Mods;

public class ModTag : BaseEntity
{
    public Mod Mod { get; set; } = default!;
    public int ModId { get; set; }

    public required string Value { get; set; }

    public class Configuration : IEntityTypeConfiguration<ModTag>
    {
        public void Configure(EntityTypeBuilder<ModTag> builder)
        {
            builder.InitBaseEntity();

            builder.Property(f => f.Value)
                .IsRequired()
                .HasMaxLength(128);

            builder.HasIndex(f => f.Value);
        }
    }
}