using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Identity.Client;
using VsModDb.Extensions;

namespace VsModDb.Data.Entities.Assets;

public class Asset : BaseEntity
{
    /// <summary>
    /// The original file name.
    /// </summary>
    public required string FileName { get; set; }

    /// <summary>
    /// The internal path/key of the asset.
    /// </summary>
    public required string AssetPath { get; set; }

    /// <summary>
    /// The content type of the file.
    /// </summary>
    public required string ContentType { get; set; }

    public class Configuration : IEntityTypeConfiguration<Asset>
    {
        public void Configure(EntityTypeBuilder<Asset> builder)
        {
            builder.InitBaseEntity();

            builder.Property(f => f.FileName)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(f => f.AssetPath)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(f => f.ContentType)
                .IsRequired()
                .HasMaxLength(128);
        }
    }
}
