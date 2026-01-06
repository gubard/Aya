using System.Runtime.CompilerServices;
using Aya.Contract.Helpers;
using Aya.Contract.Models;
using Gaia.Models;
using Gaia.Services;
using Microsoft.EntityFrameworkCore;
using Nestor.Db.Services;

namespace Aya.Contract.Services;

public interface IHttpFilesService : IFilesService;

public interface IFilesService
    : IService<AyaGetRequest, AyaPostRequest, AyaGetResponse, AyaPostResponse>;

public interface IEfFilesService
    : IFilesService,
        IEfService<AyaGetRequest, AyaPostRequest, AyaGetResponse, AyaPostResponse>;

public class EfFilesService<TDbContext>
    : EfService<AyaGetRequest, AyaPostRequest, AyaGetResponse, AyaPostResponse, TDbContext>,
        IEfFilesService
    where TDbContext : NestorDbContext, IFileDbContext
{
    private readonly GaiaValues _gaiaValues;

    public EfFilesService(TDbContext dbContext, GaiaValues gaiaValues)
        : base(dbContext)
    {
        _gaiaValues = gaiaValues;
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
        var files = await DbContext.Files.ToArrayAsync(ct);

        if (request.IsGetFiles)
        {
            response.Files = files.Select(x => x.ToFile()).ToArray();
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
        await FileEntity.AddEntitiesAsync(DbContext, userId, idempotentId, creates, ct);

        await FileEntity.DeleteEntitiesAsync(
            DbContext,
            userId,
            idempotentId,
            request.DeleteIds,
            ct
        );

        await DbContext.SaveChangesAsync(ct);

        return new();
    }

    public override AyaPostResponse Post(Guid idempotentId, AyaPostRequest request)
    {
        var userId = _gaiaValues.UserId.ToString();
        var creates = request.CreateFiles.Select(x => x.ToFileEntity()).ToArray();
        FileEntity.AddEntities(DbContext, userId, idempotentId, creates);
        FileEntity.DeleteEntities(DbContext, userId, idempotentId, request.DeleteIds);
        DbContext.SaveChanges();

        return new();
    }

    public override AyaGetResponse Get(AyaGetRequest request)
    {
        var response = new AyaGetResponse();
        var files = DbContext.Files.ToArray();

        if (request.IsGetFiles)
        {
            response.Files = files.Select(x => x.ToFile()).ToArray();
        }

        return response;
    }
}
