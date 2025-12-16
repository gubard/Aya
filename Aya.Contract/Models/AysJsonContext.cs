using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Gaia.Models;

namespace Aya.Contract.Models;

[JsonSerializable(typeof(AyaGetRequest))]
[JsonSerializable(typeof(AyaPostRequest))]
[JsonSerializable(typeof(AyaGetResponse))]
[JsonSerializable(typeof(AyaPostResponse))]
[JsonSerializable(typeof(AlreadyExistsValidationError))]
[JsonSerializable(typeof(NotFoundValidationError))]
public partial class AysJsonContext : JsonSerializerContext
{
    public static readonly IJsonTypeInfoResolver Resolver;

    static AysJsonContext()
    {
        Resolver = Default.WithAddedModifier(typeInfo =>
        {
            if (typeInfo.Type == typeof(ValidationError))
            {
                typeInfo.PolymorphismOptions = new()
                {
                    TypeDiscriminatorPropertyName = "$type",
                    DerivedTypes =
                    {
                        new(
                            typeof(AlreadyExistsValidationError),
                            typeof(AlreadyExistsValidationError).FullName!
                        ),
                        new(
                            typeof(NotFoundValidationError),
                            typeof(NotFoundValidationError).FullName!
                        ),
                    },
                };
            }
        });
    }
}
