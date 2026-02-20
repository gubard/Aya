using System.Runtime.CompilerServices;
using Aya.Contract.Helpers;
using Aya.Contract.Models;
using Aya.Contract.Services;
using Gaia.Helpers;
using Gaia.Models;
using Gaia.Services;
using Nestor.Db.Helpers;
using Nestor.Db.Models;
using Nestor.Db.Services;

namespace Aya.Db.Services;

public sealed class FileSystemDbService
    : DbService<AyaGetRequest, AyaPostRequest, AyaGetResponse, AyaPostResponse>,
        IFileSystemDbService,
        IFileSystemDbCache
{
    public FileSystemDbService(
        IDbConnectionFactory factory,
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

    private async ValueTask<AyaGetResponse> GetCore(AyaGetRequest request, CancellationToken ct)
    {
        var response = new AyaGetResponse();
        await using var session = await Factory.CreateSessionAsync(ct);

        if (request.IsGetFiles)
        {
            await using var reader = await session.ExecuteReaderAsync(FilesExt.SelectQuery, ct);

            response.Files = (await reader.ReadFilesAsync(ct).ToEnumerableAsync())
                .Select(x => x.ToFile())
                .ToArray();
        }

        return response;
    }

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
        await using var session = await Factory.CreateSessionAsync(ct);
        var isUseEvents = _factoryOptions.Create().IsUseEvents;
        await session.AddEntitiesAsync(userId, idempotentId, isUseEvents, creates, ct);
        await session.DeleteEntitiesAsync(userId, idempotentId, isUseEvents, request.DeleteIds, ct);
        await session.CommitAsync(ct);
    }

    private async ValueTask UpdateCore(AyaPostRequest source, CancellationToken ct)
    {
        await PostAsync(Guid.NewGuid(), source, ct);
    }

    private async ValueTask UpdateCore(AyaGetResponse source, CancellationToken ct)
    {
        await using var session = await Factory.CreateSessionAsync(ct);
        var entities = source.Files.Select(x => x.ToFileEntity()).ToArray();
        var ids = entities.Select(x => x.Id).ToArray();

        if (entities.Length == 0)
        {
            return;
        }

        var deleteIds = await session.GetGuidAsync(
            new(
                FilesExt.SelectIdsQuery + $" WHERE Id NOT IN ({ids.ToParameterNames("Id")})",
                ids.ToQueryParameters("Id")
            ),
            ct
        );

        var exists = await session.IsExistsAsync(entities, ct);

        var updateQueries = entities
            .Where(x => exists.Contains(x.Id))
            .Select(x => x.CreateUpdateFilesQuery())
            .ToArray();

        var inserts = entities.Where(x => !exists.Contains(x.Id)).ToArray();

        if (inserts.Length != 0)
        {
            await session.ExecuteNonQueryAsync(inserts.CreateInsertQuery(), ct);
        }

        foreach (var query in updateQueries)
        {
            await session.ExecuteNonQueryAsync(query, ct);
        }

        if (deleteIds.Length != 0)
        {
            await session.ExecuteNonQueryAsync(deleteIds.CreateDeleteFilesQuery(), ct);
        }

        await session.CommitAsync(ct);
    }
}
