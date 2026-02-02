using System.Collections.Frozen;
using Aya.Contract.Helpers;
using Aya.Contract.Models;
using Aya.Contract.Services;
using Nestor.Db.Helpers;
using Zeus.Helpers;

InsertHelper.AddDefaultInsert(
    nameof(FileEntity),
    id => new FileEntity[] { new() { Id = id } }.CreateInsertQuery()
);

var migration = new Dictionary<int, string>();

foreach (var (key, value) in SqliteMigration.Migrations)
{
    migration.Add(key, value);
}

foreach (var (key, value) in AyaMigration.Migrations)
{
    migration.Add(key, value);
}

await WebApplication
    .CreateBuilder(args)
    .CreateAndRunZeusApp<
        IFileSystemService,
        FileSystemDbService,
        AyaGetRequest,
        AyaPostRequest,
        AyaGetResponse,
        AyaPostResponse
    >(migration.ToFrozenDictionary(), "Aya");
