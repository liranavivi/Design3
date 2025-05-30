using EntitiesManager.Core.Entities.Base;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace EntitiesManager.Core.Entities;

public class ProtocolEntity : BaseEntity
{
    public override string GetCompositeKey() => $"{Name}_{Version}";
}
