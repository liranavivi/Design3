using EntitiesManager.Core.Entities;

namespace EntitiesManager.Core.Interfaces.Repositories;

public interface IExporterEntityRepository : IBaseRepository<ExporterEntity>
{
    Task<IEnumerable<ExporterEntity>> GetByAddressAsync(string address);
    Task<IEnumerable<ExporterEntity>> GetByVersionAsync(string version);
    Task<IEnumerable<ExporterEntity>> GetByNameAsync(string name);
}
