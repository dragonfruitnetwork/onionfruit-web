using DragonFruit.Data;
using DragonFruit.Data.Serializers.SystemJson;

namespace DragonFruit.OnionFruit.Api.Tests
{
    public class OnionFruitApiTest
    {
        protected static ApiClient Client { get; } = new ApiClient<ApiSystemTextJsonSerializer>();
    }
}