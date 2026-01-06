using Aya.CompiledModels;
using Aya.Contract.Models;
using Gaia.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Nestor.Db.Services;

namespace Aya.Services;

public sealed class AyaDbContext
    : NestorDbContext,
        IStaticFactory<DbContextOptions, NestorDbContext>
{
    public AyaDbContext() { }

    public AyaDbContext(DbContextOptions options)
        : base(options) { }

    public DbSet<FileEntity> Files { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseModel(AyaDbContextModel.Instance);
    }

    public static NestorDbContext Create(DbContextOptions input)
    {
        return new AyaDbContext(input);
    }
}

public class AyaDbContextFactory : IDesignTimeDbContextFactory<AyaDbContext>
{
    public AyaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AyaDbContext>();
        optionsBuilder.UseSqlite("");

        return new(optionsBuilder.Options);
    }
}
