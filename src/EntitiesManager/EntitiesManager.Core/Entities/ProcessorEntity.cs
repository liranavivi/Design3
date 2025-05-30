using EntitiesManager.Core.Entities.Base;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace EntitiesManager.Core.Entities;

public class ProcessorEntity : BaseEntity
{
    [BsonElement("address")]
    [Required(ErrorMessage = "Address is required")]
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string Address { get; set; } = string.Empty;

    [BsonElement("configuration")]
    public Dictionary<string, object> Configuration { get; set; } = new();

    public override string GetCompositeKey() => $"{Address}_{Version}";
}
