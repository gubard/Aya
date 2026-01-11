namespace Aya.Contract.Models;

public class FileEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public FileType Type { get; set; }
    public string Path { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}
