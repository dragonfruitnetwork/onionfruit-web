using System.Threading.Tasks;
using DragonFruit.OnionFruit.Web.Worker.Storage.Abstractions;
using JetBrains.Annotations;

namespace DragonFruit.OnionFruit.Web.Worker.Generators;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature, ImplicitUseTargetFlags.WithInheritors)]
public interface IDatabaseGenerator
{
    Task GenerateDatabase(IFileSink fileSink);
}