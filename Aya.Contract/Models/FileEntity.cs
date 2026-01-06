using Aya.Contract.Services;
using Nestor.Db.Models;

namespace Aya.Contract.Models;

[SourceEntity(nameof(Id), typeof(IFileDbContext))]
public partial class FileEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public FileType Type { get; set; }
    public string Path { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
}
