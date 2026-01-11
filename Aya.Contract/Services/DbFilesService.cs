using System.Runtime.CompilerServices;
using Aya.Contract.Helpers;
using Aya.Contract.Models;
using Gaia.Models;
using Gaia.Services;
using Nestor.Db.Helpers;
using Nestor.Db.Models;
using Nestor.Db.Services;

namespace Aya.Contract.Services;

public interface IHttpFilesService
    : IFilesService,
        IHttpService<AyaGetRequest, AyaPostRequest, AyaGetResponse, AyaPostResponse>;

public interface IFilesService
    : IService<AyaGetRequest, AyaPostRequest, AyaGetResponse, AyaPostResponse>;

public interface IDbFilesService
    : IFilesService,
        IDbService<AyaGetRequest, AyaPostRequest, AyaGetResponse, AyaPostResponse>;

public interface IFilesDbCache : IDbCache<AyaPostRequest, AyaGetResponse>;

public class DbFilesService
    : DbService<AyaGetRequest, AyaPostRequest, AyaGetResponse, AyaPostResponse>,
        IDbFilesService,
        IFilesDbCache
{
    private readonly GaiaValues _gaiaValues;
    private readonly IFactory<DbServiceOptions> _factoryOptions;

    public DbFilesService(
        IDbConnectionFactory factory,
        GaiaValues gaiaValues,
        IFactory<DbServiceOptions> factoryOptions
    )
        : base(factory)
    {
        _gaiaValues = gaiaValues;
        _factoryOptions = factoryOptions;
    }

    public override ConfiguredValueTaskAwaitable<AyaGetResponse> GetAsync(
        AyaGetRequest request,
        CancellationToken ct
    )
    {
        return GetCore(request, ct).ConfigureAwait(false);
    }

    private async ValueTask<AyaGetResponse> GetCore(AyaGetRequest request, CancellationToken ct)
    {
        var response = new AyaGetResponse();
        await using var session = await Factory.CreateSessionAsync(ct);

        if (request.IsGetFiles)
        {
            await using var reader = await session.ExecuteReaderAsync(FilesExt.SelectQuery, ct);
            response.Files = reader.ReadFiles().Select(x => x.ToFile()).ToArray();
        }

        return response;
    }

    protected override ConfiguredValueTaskAwaitable<AyaPostResponse> ExecuteAsync(
        Guid idempotentId,
        AyaPostRequest request,
        CancellationToken ct
    )
    {
        return PostCore(idempotentId, request, ct).ConfigureAwait(false);
    }

    private async ValueTask<AyaPostResponse> PostCore(
        Guid idempotentId,
        AyaPostRequest request,
        CancellationToken ct
    )
    {
        var userId = _gaiaValues.UserId.ToString();
        var creates = request.CreateFiles.Select(x => x.ToFileEntity()).ToArray();
        await using var session = await Factory.CreateSessionAsync(ct);
        var isUseEvents = _factoryOptions.Create().IsUseEvents;
        await session.AddEntitiesAsync(userId, idempotentId, isUseEvents, creates, ct);
        await session.CommitAsync(ct);

        return new();
    }

    protected override AyaPostResponse Execute(Guid idempotentId, AyaPostRequest request)
    {
        var userId = _gaiaValues.UserId.ToString();
        var creates = request.CreateFiles.Select(x => x.ToFileEntity()).ToArray();
        using var session = Factory.CreateSession();
        var isUseEvents = _factoryOptions.Create().IsUseEvents;
        session.AddEntities(userId, idempotentId, isUseEvents, creates);
        session.Commit();

        return new();
    }

    public override AyaGetResponse Get(AyaGetRequest request)
    {
        var response = new AyaGetResponse();
        using var session = Factory.CreateSession();

        if (request.IsGetFiles)
        {
            using var reader = session.ExecuteReader(FilesExt.SelectQuery);
            response.Files = reader.ReadFiles().Select(x => x.ToFile()).ToArray();
        }

        return response;
    }

    public ConfiguredValueTaskAwaitable UpdateAsync(AyaPostRequest source, CancellationToken ct)
    {
        return UpdateCore(source, ct).ConfigureAwait(false);
    }

    public async ValueTask UpdateCore(AyaPostRequest source, CancellationToken ct)
    {
        var userId = _gaiaValues.UserId.ToString();
        var creates = source.CreateFiles.Select(x => x.ToFileEntity()).ToArray();
        await using var session = await Factory.CreateSessionAsync(ct);
        await session.AddEntitiesAsync(userId, Guid.NewGuid(), false, creates, ct);
        await session.CommitAsync(ct);
    }

    public void Update(AyaPostRequest source)
    {
        var userId = _gaiaValues.UserId.ToString();
        var creates = source.CreateFiles.Select(x => x.ToFileEntity()).ToArray();
        using var session = Factory.CreateSession();
        session.AddEntities(userId, Guid.NewGuid(), false, creates);
        session.Commit();
    }

    public ConfiguredValueTaskAwaitable UpdateAsync(AyaGetResponse source, CancellationToken ct)
    {
        return UpdateCore(source, ct).ConfigureAwait(false);
    }

    public async ValueTask UpdateCore(AyaGetResponse source, CancellationToken ct)
    {
        await using var session = await Factory.CreateSessionAsync(ct);
        var entities = source.Files.Select(x => x.ToFileEntity()).ToArray();

        if (entities.Length == 0)
        {
            return;
        }

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

        await session.CommitAsync(ct);
    }

    public void Update(AyaGetResponse source)
    {
        using var session = Factory.CreateSession();
        var entities = source.Files.Select(x => x.ToFileEntity()).ToArray();

        if (entities.Length == 0)
        {
            return;
        }

        var exists = session.IsExists(entities);

        var updateQueries = entities
            .Where(x => exists.Contains(x.Id))
            .Select(x => x.CreateUpdateFilesQuery())
            .ToArray();

        var inserts = entities.Where(x => !exists.Contains(x.Id)).ToArray();

        if (inserts.Length != 0)
        {
            session.ExecuteNonQuery(inserts.CreateInsertQuery());
        }

        foreach (var query in updateQueries)
        {
            session.ExecuteNonQuery(query);
        }

        session.Commit();
    }
}
