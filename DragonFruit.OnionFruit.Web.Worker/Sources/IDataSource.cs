using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace DragonFruit.OnionFruit.Web.Worker.Sources;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature, ImplicitUseTargetFlags.WithInheritors)]
public interface IDataSource
{
    Task<bool> HasDataChanged(DateTimeOffset lastVersionDate);

    Task CollectData();
}