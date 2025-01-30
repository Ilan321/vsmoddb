using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VsModDb.Extensions;
using VsModDb.Models.Mods;

namespace VsModDb.Data.Entities.Mods;

public class ModComment : BaseEntity
{
    public Mod LinkedMod { get; set; } = default!;
    public int LinkedModId { get; set; }

    public User LinkedUser { get; set; } = default!;
    public string LinkedUserId { get; set; } = default!;

    public required string Comment { get; set; }
    public DateTime TimeCreatedUtc { get; init; }
    public DateTime? TimeUpdatedUtc { get; set; }
    public ModCommentContentType ContentType { get; set; }

    public class Configuration : IEntityTypeConfiguration<ModComment>
    {
        public void Configure(EntityTypeBuilder<ModComment> builder)
        {
            builder.InitBaseEntity();

            builder.Property(f => f.Comment)
                .IsRequired()
                .HasMaxLength(-1);

            builder.Property(f => f.TimeCreatedUtc)
                .HasDefaultValueSql("GETUTCDATE()")
                .IsUtc();

            builder.Property(f => f.TimeUpdatedUtc)
                .IsUtc();

            builder.Property(f => f.ContentType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(64)
                .HasDefaultValue(ModCommentContentType.Html);

            builder.HasOne(f => f.LinkedMod)
                .WithMany(f => f.Comments)
                .HasForeignKey(f => f.LinkedModId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(f => f.LinkedUser)
                .WithMany(f => f.Comments)
                .HasForeignKey(f => f.LinkedUserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
