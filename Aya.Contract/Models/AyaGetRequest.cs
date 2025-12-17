using Gaia.Services;

namespace Aya.Contract.Models;

public class AyaGetRequest : IGetRequest
{
    public bool IsGetFiles { get; set; }
    public long LastId { get; set; } = -1;
}
