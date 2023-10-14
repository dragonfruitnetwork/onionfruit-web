// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

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