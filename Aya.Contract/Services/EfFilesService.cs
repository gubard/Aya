using Aya.Contract.Helpers;
using Aya.Contract.Models;
using Gaia.Models;
using Gaia.Services;
using Microsoft.EntityFrameworkCore;
using Nestor.Db.Models;
using Nestor.Db.Services;

namespace Aya.Contract.Services;

public interface IHttpFilesService : IFilesService;

public interface IFilesService
    : IService<AyaGetRequest, AyaPostRequest, AyaGetResponse, AyaPostResponse>;

public interface IEfFilesService
    : IFilesService,
        IEfService<AyaGetRequest, AyaPostRequest, AyaGetResponse, AyaPostResponse>;

public class EfFilesService
    : EfService<AyaGetRequest, AyaPostRequest, AyaGetResponse, AyaPostResponse>,
        IEfFilesService
{
    private readonly GaiaValues _gaiaValues;

    public EfFilesService(DbContext dbContext, GaiaValues gaiaValues)
        : base(dbContext)
    {
        _gaiaValues = gaiaValues;
    }

    public override async ValueTask<AyaGetResponse> GetAsync(
        AyaGetRequest request,
        CancellationToken ct
    )
    {
        var response = new AyaGetResponse();
        var files = await FileEntity.GetEntitiesAsync(DbContext.Set<EventEntity>(), ct);

        if (request.IsGetFiles)
        {
            response.Files = files.Select(x => x.ToFile()).ToArray();
        }

        return response;
    }

    public override async ValueTask<AyaPostResponse> PostAsync(
        AyaPostRequest request,
        CancellationToken ct
    )
    {
        var userId = _gaiaValues.UserId.ToString();
        var creates = request.CreateFiles.Select(x => x.ToFileEntity()).ToArray();
        await FileEntity.AddEntitiesAsync(DbContext, userId, ct, creates);
        await FileEntity.DeleteEntitiesAsync(DbContext, userId, ct, request.DeleteIds);
        await DbContext.SaveChangesAsync(ct);

        return new();
    }

    public override AyaPostResponse Post(AyaPostRequest request)
    {
        var userId = _gaiaValues.UserId.ToString();
        var creates = request.CreateFiles.Select(x => x.ToFileEntity()).ToArray();
        FileEntity.AddEntities(DbContext, userId, creates);
        FileEntity.DeleteEntities(DbContext, userId, request.DeleteIds);
        DbContext.SaveChanges();

        return new();
    }

    public override AyaGetResponse Get(AyaGetRequest request)
    {
        var response = new AyaGetResponse();
        var files = FileEntity.GetEntities(DbContext.Set<EventEntity>());

        if (request.IsGetFiles)
        {
            response.Files = files.Select(x => x.ToFile()).ToArray();
        }

        return response;
    }
}
