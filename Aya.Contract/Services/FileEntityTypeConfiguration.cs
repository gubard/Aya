using Aya.Contract.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aya.Contract.Services;

public sealed class FileEntityTypeConfiguration : IEntityTypeConfiguration<FileEntity>
{
    public void Configure(EntityTypeBuilder<FileEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder
            .Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .Metadata.SetValueComparer(
                new ValueComparer<Guid>((c1, c2) => c1 == c2, c => c.GetHashCode(), c => c)
            );

        builder.Property(e => e.Host).HasMaxLength(1000);
        builder.Property(e => e.Login).HasMaxLength(255);
        builder.Property(e => e.Name).HasMaxLength(255);
        builder.Property(e => e.Password).HasMaxLength(512);
        builder.Property(e => e.Path).HasMaxLength(1000);
    }
}
