using Gaia.Models;
using Gaia.Services;
using Nestor.Db.Models;

namespace Aya.Contract.Models;

public sealed class AyaPostResponse : IValidationErrors, IPostResponse
{
    public List<ValidationError> ValidationErrors { get; } = [];
    public EventEntity[] Events { get; set; } = [];
    public bool IsEventSaved { get; set; }
}
