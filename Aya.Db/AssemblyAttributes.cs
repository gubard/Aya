using Aya.Contract.Models;
using Nestor.Db.LiteDb.Models;
using Nestor.Db.Models;

[assembly: LiteDb(typeof(FileEntity), nameof(FileEntity.Id), false)]
[assembly: LiteDbSourceEntity(typeof(FileEntity), nameof(FileEntity.Id))]
[assembly: Ado(typeof(FileEntity), nameof(FileEntity.Id), false)]
[assembly: AdoSourceEntity(typeof(FileEntity), nameof(FileEntity.Id))]
