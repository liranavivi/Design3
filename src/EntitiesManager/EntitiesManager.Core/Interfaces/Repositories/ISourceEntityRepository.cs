using EntitiesManager.Core.Entities;

namespace EntitiesManager.Core.Interfaces.Repositories;

public interface ISourceEntityRepository : IBaseRepository<SourceEntity>
{
    Task<IEnumerable<SourceEntity>> GetByAddressAsync(string address);
    Task<IEnumerable<SourceEntity>> GetByVersionAsync(string version);
    Task<IEnumerable<SourceEntity>> GetByNameAsync(string name);
}
