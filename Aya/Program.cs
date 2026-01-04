using Aya.Contract.Models;
using Aya.Contract.Services;
using Aya.Services;
using Zeus.Helpers;

await WebApplication
    .CreateBuilder(args)
    .CreateAndRunZeusApp<
        IFilesService,
        EfFilesService,
        AyaGetRequest,
        AyaPostRequest,
        AyaGetResponse,
        AyaPostResponse,
        AyaDbContext
    >("Aya");
