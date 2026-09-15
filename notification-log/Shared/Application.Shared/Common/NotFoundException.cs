namespace Application.Shared.Common;

public sealed class NotFoundException : Exception
{
    public string EntityName { get; }
    public object Key { get; }

    public NotFoundException(string entityName, object key)
        : base($"{entityName} con id '{key}' no fue encontrado.")
    {
        EntityName = entityName;
        Key = key;
    }
}
