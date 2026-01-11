using Aya.Contract.Models;
using File = Aya.Contract.Models.File;

namespace Aya.Contract.Helpers;

public static class Mapper
{
    public static File ToFile(this FileEntity entity)
    {
        return new()
        {
            Name = entity.Name,
            Type = entity.Type,
            Host = entity.Host,
            Id = entity.Id,
            Login = entity.Login,
            Password = entity.Password,
            Path = entity.Path,
            Color = entity.Color,
            Icon = entity.Icon,
        };
    }

    public static FileEntity ToFileEntity(this File file)
    {
        return new()
        {
            Name = file.Name,
            Type = file.Type,
            Host = file.Host,
            Id = file.Id,
            Login = file.Login,
            Password = file.Password,
            Path = file.Path,
            Color = file.Color,
            Icon = file.Icon,
        };
    }
}
