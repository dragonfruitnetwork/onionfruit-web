using System.Threading.Tasks;

namespace DragonFruit.OnionFruit.Web.Worker.Sources;

public interface IDatabaseGenerator
{
    Task GenerateDatabase();
}