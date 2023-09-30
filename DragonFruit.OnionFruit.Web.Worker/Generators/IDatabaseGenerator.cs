using System;
using System.Threading.Tasks;
using DragonFruit.OnionFruit.Web.Worker.Storage;

namespace DragonFruit.OnionFruit.Web.Worker.Generators;

public interface IDatabaseGenerator
{
    Task GenerateDatabase(Lazy<IDatabaseFileSink> fileSink);
}