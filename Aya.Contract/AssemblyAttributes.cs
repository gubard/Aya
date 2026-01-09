using Aya.Contract.Models;
using Nestor.Db.Models;

[assembly: SqliteAdo(typeof(FileEntity), nameof(FileEntity.Id), false)]
[assembly: SourceEntity(typeof(FileEntity), nameof(FileEntity.Id))]
