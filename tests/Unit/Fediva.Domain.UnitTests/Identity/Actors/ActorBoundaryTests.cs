using System.Reflection;
using Fediva.Domain.Identity.Actors;

namespace Fediva.Domain.UnitTests.Identity.Actors;

public class ActorBoundaryTests
{
    [Test]
    public async Task PublicApi_DoesNotExposeProtocolSpecificTypes()
    {
        var members = typeof(Actor).GetMembers(BindingFlags.Public | BindingFlags.Instance);
        var publicApiTypes = members
            .SelectMany(member =>
                member switch
                {
                    PropertyInfo property => FlattenType(property.PropertyType),
                    MethodInfo method => FlattenMethodTypes(method),
                    _ => [],
                }
            )
            .Distinct();

        var forbiddenTerms = new[]
        {
            "ExternalIdentity",
            "Did",
            "AtUri",
            "Cid",
            "ActivityPub",
            "AtProtocol",
            "Fediverse",
        };

        var protocolTypes = publicApiTypes
            .Where(type =>
                forbiddenTerms.Any(term =>
                    type.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                )
            )
            .ToArray();

        await Assert.That(protocolTypes.Length).IsEqualTo(0);
    }

    private static IEnumerable<Type> FlattenMethodTypes(MethodInfo method)
    {
        return FlattenType(method.ReturnType)
            .Concat(
                method.GetParameters().SelectMany(parameter => FlattenType(parameter.ParameterType))
            );
    }

    private static IEnumerable<Type> FlattenType(Type type)
    {
        yield return type;

        if (type.IsGenericType)
        {
            foreach (var argument in type.GetGenericArguments())
            {
                foreach (var nestedType in FlattenType(argument))
                {
                    yield return nestedType;
                }
            }
        }
    }
}
