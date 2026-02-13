using Nestor.Db.Models;

namespace Aya.Contract.Models;

public sealed class AyaPostRequest : IPostRequest
{
    public File[] CreateFiles { get; set; } = [];
    public Guid[] DeleteIds { get; set; } = [];
    public long LastLocalId { get; set; }
    public EventEntity[] Events { get; set; } = [];
}
