using EntitiesManager.Core.Entities;

namespace EntitiesManager.Core.Interfaces.Repositories;

public interface IStepEntityRepository : IBaseRepository<StepEntity>
{
    Task<IEnumerable<StepEntity>> GetByAddressAsync(string address);
    Task<IEnumerable<StepEntity>> GetByVersionAsync(string version);
    Task<IEnumerable<StepEntity>> GetByNameAsync(string name);
}
