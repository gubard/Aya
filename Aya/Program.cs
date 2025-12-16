using Aya.Contract.Models;
using Aya.Contract.Services;
using Zeus.Helpers;

await WebApplication
    .CreateBuilder(args)
    .CreateAndRunZeusApp<
        IFilesService,
        EfFilesService,
        AyaGetRequest,
        AyaPostRequest,
        AyaGetResponse,
        AyaPostResponse
    >("Aya");
