namespace Application.Shared.Common;

public sealed class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}
