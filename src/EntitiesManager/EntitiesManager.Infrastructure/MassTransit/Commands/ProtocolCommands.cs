namespace EntitiesManager.Infrastructure.MassTransit.Commands;

public class CreateProtocolCommand
{
    public string Address { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
    public string RequestedBy { get; set; } = string.Empty;
}

public class UpdateProtocolCommand
{
    public Guid Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
    public string RequestedBy { get; set; } = string.Empty;
}

public class DeleteProtocolCommand
{
    public Guid Id { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
}

public class GetProtocolQuery
{
    public Guid? Id { get; set; }
    public string? CompositeKey { get; set; }
}
