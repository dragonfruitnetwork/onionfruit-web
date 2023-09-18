using System;
using System.Threading.Tasks;

namespace DragonFruit.OnionFruit.Web.Worker.Sources;

public interface IDataSource
{
    Task<bool> HasDataChanged(DateTime lastVersionDate);
}