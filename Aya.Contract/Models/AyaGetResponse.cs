using Gaia.Models;
using Nestor.Db.Models;

namespace Aya.Contract.Models;

public class AyaGetResponse : IResponse
{
    public File[] Files { get; set; } = [];
    public List<ValidationError> ValidationErrors { get; } = [];
    public EventEntity[] Events { get; set; } = [];
}
