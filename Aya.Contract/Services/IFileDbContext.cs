using Aya.Contract.Models;
using Microsoft.EntityFrameworkCore;
using Nestor.Db.Services;

namespace Aya.Contract.Services;

public interface IFileDbContext : INestorDbContext
{
    DbSet<FileEntity> Files { get; }
}
