using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VsModDb.Data.Entities.Assets;
using VsModDb.Extensions;

namespace VsModDb.Data.Entities.Mods;

public class Mod : BaseEntity
{
    /// <summary>
    /// The name of the mod.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The slogan/subtitle of the mod.
    /// </summary>
    public required string Summary { get; set; }

    /// <summary>
    /// The URL alias of the mod.
    /// </summary>
    public string? UrlAlias { get; set; }

    /// <summary>
    /// The time the mod was created.
    /// </summary>
    public required DateTime TimeCreatedUtc { get; set; }

    /// <summary>
    /// The time the mod was last updated.
    /// </summary>
    public required DateTime TimeUpdatedUtc { get; set; }

    /// <summary>
    /// The description of the mod.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// List of tags for the mod.
    /// </summary>
    public List<ModTag> Tags { get; set; }

    /// <summary>
    /// The banner mod asset.
    /// </summary>
    public Asset? Banner { get; set; }
    public int? BannerId { get; set; }

    /// <summary>
    /// List of comments for the mod.
    /// </summary>
    public List<ModComment> Comments { get; set; } = default!;

    public class Configuration : IEntityTypeConfiguration<Mod>
    {
        public void Configure(EntityTypeBuilder<Mod> builder)
        {
            builder.InitBaseEntity();

            builder.Property(f => f.Name)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(f => f.Summary)
                .IsRequired()
                .HasMaxLength(512);

            builder.Property(f => f.UrlAlias)
                .HasMaxLength(256);

            builder.Property(f => f.TimeCreatedUtc)
                .HasDefaultValueSql("GETUTCDATE()")
                .IsUtc();

            builder.Property(f => f.TimeUpdatedUtc)
                .HasDefaultValueSql("GETUTCDATE()")
                .IsUtc();

            builder.Property(f => f.Description)
                .HasMaxLength(-1);

            builder.HasIndex(f => f.Name);

            builder.HasIndex(f => f.UrlAlias);

            builder.HasIndex(f => f.TimeCreatedUtc);

            builder.HasIndex(f => f.TimeUpdatedUtc);

            builder.HasMany(f => f.Tags)
                .WithOne(f => f.Mod)
                .HasForeignKey(f => f.ModId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(f => f.Banner)
                .WithOne()
                .HasPrincipalKey<Asset>()
                .HasForeignKey<Mod>(f => f.BannerId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
