using EntitiesManager.Core.Entities;

namespace EntitiesManager.Core.Interfaces.Repositories;

public interface IProcessorEntityRepository : IBaseRepository<ProcessorEntity>
{
    Task<IEnumerable<ProcessorEntity>> GetByAddressAsync(string address);
    Task<IEnumerable<ProcessorEntity>> GetByVersionAsync(string version);
    Task<IEnumerable<ProcessorEntity>> GetByNameAsync(string name);
}
