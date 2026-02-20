using Aya.Contract.Models;
using Gaia.Services;
using Nestor.Db.Services;

namespace Aya.Contract.Services;

public interface IFileSystemHttpService
    : IFileSystemService,
        IHttpService<AyaGetRequest, AyaPostRequest, AyaGetResponse, AyaPostResponse>;

public interface IFileSystemService
    : IService<AyaGetRequest, AyaPostRequest, AyaGetResponse, AyaPostResponse>;

public interface IFileSystemDbService
    : IFileSystemService,
        IDbService<AyaGetRequest, AyaPostRequest, AyaGetResponse, AyaPostResponse>;

public interface IFileSystemDbCache : IDbCache<AyaPostRequest, AyaGetResponse>;

public sealed class EmptyFileSystemDbCache
    : EmptyDbCache<AyaPostRequest, AyaGetResponse>,
        IFileSystemDbCache;

public sealed class EmptyFileSystemDbService
    : EmptyDbService<AyaGetRequest, AyaPostRequest, AyaGetResponse, AyaPostResponse>,
        IFileSystemDbService;
