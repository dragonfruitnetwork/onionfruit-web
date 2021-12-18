// OnionFruit API/Tooling Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using DragonFruit.Data;
using DragonFruit.Data.Serializers.SystemJson;

namespace DragonFruit.OnionFruit.Api.Tests
{
    public class OnionFruitApiTest
    {
        protected static ApiClient Client { get; } = new ApiClient<ApiSystemTextJsonSerializer>();
    }
}
