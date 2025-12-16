using Gaia.Models;
using Gaia.Services;
using Nestor.Db.Models;

namespace Aya.Contract.Models;

public class AyaPostResponse : IValidationErrors, IResponse
{
    public List<ValidationError> ValidationErrors { get; } = [];
    public EventEntity[] Events { get; set; } = [];
}
