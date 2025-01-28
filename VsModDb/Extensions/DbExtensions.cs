using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VsModDb.Data.Entities;

namespace VsModDb.Extensions;

public static class DbExtensions
{
    public static EntityTypeBuilder<T> InitBaseEntity<T>(this EntityTypeBuilder<T> builder)
        where T : BaseEntity
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .UseIdentityColumn();

        return builder;
    }

    public static PropertyBuilder<DateTime> IsUtc(this PropertyBuilder<DateTime> builder) =>
        builder.HasConversion(f => f, f => DateTime.SpecifyKind(f, DateTimeKind.Utc));

    public static PropertyBuilder<DateTime?> IsUtc(this PropertyBuilder<DateTime?> builder) =>
        builder.HasConversion(f => f, f => f.HasValue ? DateTime.SpecifyKind(f.Value, DateTimeKind.Utc) : null);
}