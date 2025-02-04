using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VsModDb.Extensions;

namespace VsModDb.Data.Entities.Account;

public class AccountLinkRequest : BaseEntity
{
    public DateTime TimeCreatedUtc { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string LinkToken { get; init; }
    public required string Secret { get; set; }

    public class Configuration : IEntityTypeConfiguration<AccountLinkRequest>
    {
        public void Configure(EntityTypeBuilder<AccountLinkRequest> builder)
        {
            builder.InitBaseEntity();

            builder.Property(f => f.TimeCreatedUtc)
                .IsUtc(defaultValue: true);

            builder.Property(f => f.Username)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(f => f.Email)
                .HasMaxLength(512)
                .IsRequired();

            builder.Property(f => f.LinkToken)
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(f => f.Secret)
                .HasMaxLength(64)
                .IsRequired();

            builder.HasIndex(f => f.LinkToken)
                .IsUnique();
        }
    }
}
