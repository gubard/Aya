using System.Collections.Frozen;

namespace Aya.Contract.Helpers;

public static class AyaMigration
{
    public static readonly FrozenDictionary<long, string> Migrations;

    static AyaMigration()
    {
        Migrations = new Dictionary<long, string>
        {
            {
                202601041204,
                @"
CREATE TABLE IF NOT EXISTS FileEntity (
    Id TEXT PRIMARY KEY NOT NULL,
    Name TEXT NOT NULL CHECK(length(Name) <= 255),
    Type INTEGER NOT NULL,
    Path TEXT NOT NULL CHECK(length(Path) <= 1000),
    Password TEXT NOT NULL CHECK(length(Password) <= 512),
    Login TEXT NOT NULL CHECK(length(Login) <= 255),
    Host TEXT NOT NULL CHECK(length(Host) <= 1000)
);
"
            },
        }.ToFrozenDictionary();
    }
}
