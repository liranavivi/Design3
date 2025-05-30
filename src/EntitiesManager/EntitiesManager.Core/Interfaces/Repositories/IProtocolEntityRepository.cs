using EntitiesManager.Core.Entities;
using EntitiesManager.Core.Interfaces.Repositories.Base;

namespace EntitiesManager.Core.Interfaces.Repositories;

public interface IProtocolEntityRepository : IBaseRepository<ProtocolEntity>
{
    Task<IEnumerable<ProtocolEntity>> GetByAddressAsync(string address);
    Task<IEnumerable<ProtocolEntity>> GetByVersionAsync(string version);
    Task<IEnumerable<ProtocolEntity>> GetByNameAsync(string name);
}
