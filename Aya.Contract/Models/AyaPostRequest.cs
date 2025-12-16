using Gaia.Services;

namespace Aya.Contract.Models;

public class AyaPostRequest : IPostRequest
{
    public File[] CreateFiles { get; set; } = [];
    public Guid[] DeleteIds { get; set; } = [];
    public long LastLocalId { get; set; }
}
