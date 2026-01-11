using System.Collections.Frozen;

namespace Aya.Contract.Helpers;

public static class AyaMigration
{
    public static readonly FrozenDictionary<int, string> Migrations;

    static AyaMigration()
    {
        Migrations = new Dictionary<int, string>
        {
            {
                4,
                @"
CREATE TABLE IF NOT EXISTS Files (
    Id TEXT PRIMARY KEY NOT NULL,
    Name TEXT NOT NULL CHECK(length(Name) <= 255),
    Type INTEGER NOT NULL,
    Path TEXT NOT NULL CHECK(length(Path) <= 1000),
    Password TEXT NOT NULL CHECK(length(Password) <= 512),
    Login TEXT NOT NULL CHECK(length(Login) <= 255),
    Host TEXT NOT NULL CHECK(length(Host) <= 1000),
    Icon TEXT NOT NULL CHECK(length(Icon) <= 255),
    Color TEXT NOT NULL CHECK(length(Color) <= 255)
);
"
            },
        }.ToFrozenDictionary();
    }
}
