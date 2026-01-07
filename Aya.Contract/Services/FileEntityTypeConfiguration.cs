using Aya.Contract.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nestor.Db.Helpers;

namespace Aya.Contract.Services;

public sealed class FileEntityTypeConfiguration : IEntityTypeConfiguration<FileEntity>
{
    public void Configure(EntityTypeBuilder<FileEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).SetComparerStruct();
        builder.Property(e => e.Host).HasMaxLength(1000).SetComparerClass();
        builder.Property(e => e.Login).HasMaxLength(255).SetComparerClass();
        builder.Property(e => e.Name).HasMaxLength(255).SetComparerClass();
        builder.Property(e => e.Password).HasMaxLength(512).SetComparerClass();
        builder.Property(e => e.Path).HasMaxLength(1000).SetComparerClass();
        builder.Property(e => e.Type).HasMaxLength(1000).SetComparerStruct();
    }
}
