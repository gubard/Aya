using Aya.Contract.Models;
using Nestor.Db.Models;

[assembly: Ado(typeof(FileEntity), nameof(FileEntity.Id), false)]
[assembly: SourceEntity(typeof(FileEntity), nameof(FileEntity.Id))]
