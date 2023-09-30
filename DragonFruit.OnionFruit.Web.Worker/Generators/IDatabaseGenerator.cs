using System.Threading.Tasks;

namespace DragonFruit.OnionFruit.Web.Worker.Generators;

public interface IDatabaseGenerator
{
    Task GenerateDatabase();
}