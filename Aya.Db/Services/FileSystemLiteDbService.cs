using System.Runtime.CompilerServices;
using Aya.Contract.Helpers;
using Aya.Contract.Models;
using Aya.Contract.Services;
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
        return GetCore(request, ct).ConfigureAwait(false);
    }

    public ConfiguredValueTaskAwaitable UpdateAsync(AyaPostRequest source, CancellationToken ct)
    {
        return UpdateCore(source, ct).ConfigureAwait(false);
    }

    public ConfiguredValueTaskAwaitable UpdateAsync(AyaGetResponse source, CancellationToken ct)
    {
        return UpdateCore(source, ct).ConfigureAwait(false);
    }

    protected override ConfiguredValueTaskAwaitable ExecuteAsync(
        Guid idempotentId,
        AyaPostResponse response,
        AyaPostRequest request,
        CancellationToken ct
    )
    {
        return ExecuteCore(idempotentId, response, request, ct).ConfigureAwait(false);
    }

    private readonly IFactory<DbValues> _dbValuesFactory;
    private readonly IFactory<DbServiceOptions> _factoryOptions;

    private async ValueTask ExecuteCore(
        Guid idempotentId,
        AyaPostResponse response,
        AyaPostRequest request,
        CancellationToken ct
    )
    {
        var dbValues = _dbValuesFactory.Create();
        var userId = dbValues.UserId.ToString();
        var creates = request.CreateFiles.Select(x => x.ToFileEntity()).ToArray();
        var isUseEvents = _factoryOptions.Create().IsUseEvents;
        var database = await Factory.CreateAsync(ct);

        await database.ExecuteAsync(
            db =>
            {
                db.AddEntities(userId, idempotentId, isUseEvents, creates);
                db.DeleteEntities(userId, idempotentId, isUseEvents, request.DeleteIds);
            },
            ct
        );
    }

    private async ValueTask UpdateCore(AyaGetResponse source, CancellationToken ct)
    {
        var entities = source.Files.Select(x => x.ToFileEntity()).ToArray();
        var ids = entities.Select(x => x.Id).ToArray();

        if (entities.Length == 0)
        {
            return;
        }

        var database = await Factory.CreateAsync(ct);

        await database.ExecuteAsync(
            db =>
            {
                var collection = db.GetFileEntityCollection();

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
            },
            ct
        );
    }

    private async ValueTask<AyaGetResponse> GetCore(AyaGetRequest request, CancellationToken ct)
    {
        var response = new AyaGetResponse();
        var database = await Factory.CreateAsync(ct);

        return await database.ExecuteAsync(
            db =>
            {
                var collection = db.GetFileEntityCollection();

                if (request.IsGetFiles)
                {
                    response.Files = collection
                        .FindAll()
                        .Select(x => x.ToFileEntity().ToFile())
                        .ToArray();
                }

                return response;
            },
            ct
        );
    }

    private async ValueTask UpdateCore(AyaPostRequest source, CancellationToken ct)
    {
        await ExecuteAsync(Guid.NewGuid(), new(), source, ct);
    }
}
