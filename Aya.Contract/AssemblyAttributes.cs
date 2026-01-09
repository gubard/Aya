using Aya.Contract.Models;
using Nestor.Db.Models;

[assembly: SqliteAdo(typeof(FileEntity), nameof(FileEntity.Id))]
[assembly: SourceEntity(typeof(FileEntity), nameof(FileEntity.Id))]
