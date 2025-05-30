using EntitiesManager.Core.Entities;

namespace EntitiesManager.Core.Interfaces.Repositories;

public interface IImporterEntityRepository : IBaseRepository<ImporterEntity>
{
    Task<IEnumerable<ImporterEntity>> GetByAddressAsync(string address);
    Task<IEnumerable<ImporterEntity>> GetByVersionAsync(string version);
    Task<IEnumerable<ImporterEntity>> GetByNameAsync(string name);
}
