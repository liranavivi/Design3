using EntitiesManager.Core.Entities;

namespace EntitiesManager.Core.Interfaces.Repositories;

public interface IDestinationEntityRepository : IBaseRepository<DestinationEntity>
{
    Task<IEnumerable<DestinationEntity>> GetByVersionAsync(string version);
    Task<IEnumerable<DestinationEntity>> GetByNameAsync(string name);
}
