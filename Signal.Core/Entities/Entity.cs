namespace Signal.Core.Entities;

public class Entity : IEntity
{
    public Entity(EntityType type, string id, string? alias)
    {
        Type = type;
        Id = id;
        Alias = alias ?? id;
    }

    public EntityType Type { get; }

    public string Id { get; }
    
    public string Alias { get; }

    public static IEntity Device(string id, string? alias) => new Entity(EntityType.Device, id, alias);

    public static IEntity Dashboard(string id, string? alias) => new Entity(EntityType.Dashboard, id, alias);

    public static IEntity Station(string id, string? alias) => new Entity(EntityType.Station, id, alias);

    public static IEntity Process(string id, string? alias) => new Entity(EntityType.Process, id, alias);
}
