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

public interface IEfFilesService
    : IFilesService,
        IDbService<AyaGetRequest, AyaPostRequest, AyaGetResponse, AyaPostResponse>;

public class DbFilesService
    : DbService<AyaGetRequest, AyaPostRequest, AyaGetResponse, AyaPostResponse>,
        IEfFilesService
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

    public override ConfiguredValueTaskAwaitable<AyaPostResponse> PostAsync(
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

    public override AyaPostResponse Post(Guid idempotentId, AyaPostRequest request)
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
}
