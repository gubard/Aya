using System.Runtime.CompilerServices;
using Aya.Contract.Helpers;
using Aya.Contract.Models;
using Aya.Contract.Services;
using Gaia.Helpers;
using Gaia.Models;
using Gaia.Services;
using Nestor.Db.LiteDb.Services;
using Nestor.Db.Models;
using UltraLiteDB;

namespace Aya.Db.Services;

public sealed class FileSystemLiteDbService
    : LiteDbService<AyaGetRequest, AyaPostRequest, AyaGetResponse, AyaPostResponse>,
        IFileSystemDbService,
        IFileSystemDbCache
{
    public FileSystemLiteDbService(
        IDatabaseFactory factory,
        IFactory<DbValues> dbValuesFactory,
        IFactory<DbServiceOptions> factoryOptions
    )
        : base(factory, nameof(FileEntity))
    {
        _dbValuesFactory = dbValuesFactory;
        _factoryOptions = factoryOptions;
    }

    public override ConfiguredValueTaskAwaitable<AyaGetResponse> GetAsync(
        AyaGetRequest request,
        CancellationToken ct
    )
    {
        var response = new AyaGetResponse();
        using var database = Factory.Create();
        var collection = database.GetFileEntityCollection();

        if (request.IsGetFiles)
        {
            response.Files = collection.FindAll().Select(x => x.ToFileEntity().ToFile()).ToArray();
        }

        return TaskHelper.FromResult(response);
    }

    public ConfiguredValueTaskAwaitable UpdateAsync(AyaPostRequest source, CancellationToken ct)
    {
        return UpdateCore(source, ct).ConfigureAwait(false);
    }

    public ConfiguredValueTaskAwaitable UpdateAsync(AyaGetResponse source, CancellationToken ct)
    {
        Update(source, ct);

        return TaskHelper.ConfiguredCompletedTask;
    }

    protected override ConfiguredValueTaskAwaitable ExecuteAsync(
        Guid idempotentId,
        AyaPostResponse response,
        AyaPostRequest request,
        CancellationToken ct
    )
    {
        var dbValues = _dbValuesFactory.Create();
        var userId = dbValues.UserId.ToString();
        var creates = request.CreateFiles.Select(x => x.ToFileEntity()).ToArray();
        using var database = Factory.Create();
        var isUseEvents = _factoryOptions.Create().IsUseEvents;
        database.AddEntities(userId, idempotentId, isUseEvents, creates);
        database.DeleteEntities(userId, idempotentId, isUseEvents, request.DeleteIds);
        database.SaveChanges();

        return TaskHelper.ConfiguredCompletedTask;
    }

    private readonly IFactory<DbValues> _dbValuesFactory;
    private readonly IFactory<DbServiceOptions> _factoryOptions;

    private async ValueTask UpdateCore(AyaPostRequest source, CancellationToken ct)
    {
        await ExecuteAsync(Guid.NewGuid(), new(), source, ct);
    }

    private void Update(AyaGetResponse source, CancellationToken ct)
    {
        var entities = source.Files.Select(x => x.ToFileEntity()).ToArray();
        var ids = entities.Select(x => x.Id).ToArray();

        if (entities.Length == 0)
        {
            return;
        }

        using var database = Factory.Create();
        var collection = database.GetFileEntityCollection();

        var exists = entities
            .Where(x => collection.Exists(Query.EQ("_id", x.Id)))
            .Select(x => x.Id)
            .ToArray();

        var deleteIds = collection
            .Find(Query.Not(Query.In("_id", ids.Select(x => new BsonValue(x)))))
            .Select(x => x["_id"])
            .ToArray();

        var updates = entities
            .Where(x => exists.Contains(x.Id))
            .Select(x => x.ToBsonDocument())
            .ToArray();

        var inserts = entities
            .Where(x => !exists.Contains(x.Id))
            .Select(x => x.ToBsonDocument())
            .ToArray();

        if (inserts.Length != 0)
        {
            collection.Insert(inserts);
        }

        if (updates.Length != 0)
        {
            collection.Update(updates);
        }

        if (deleteIds.Length != 0)
        {
            collection.Delete(Query.In("_id", deleteIds));
        }

        database.SaveChanges();
    }
}
