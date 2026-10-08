using Application.Shared.Abstractions;
using Domain.Shared.Authorization;
using NotificationLog.RentalService.Domain.Visitors;

namespace NotificationLog.RentalService.Application.Visitors.Commands.UpsertVisitor;

public sealed class UpsertVisitorHandler(IVisitorRepository visitors, IUnitOfWork uow)
{
    private readonly IVisitorRepository _visitors = visitors;
    private readonly IUnitOfWork _uow = uow;

    public async Task HandleAsync(UpsertVisitorCommand cmd, CancellationToken ct)
    {
        if (cmd.Role is not (null or UserRole.Visitor))
            return;

        var visitor = await _visitors.LoadAsync(cmd.VisitorId, ct);

        if (visitor is null)
        {
            if (cmd.Role is null)
                return;

            visitor = Visitor.Register(cmd.VisitorId, cmd.Name, cmd.Email, cmd.Phone);
        }
        else
        {
            visitor.Update(cmd.Name, cmd.Email, cmd.Phone);
        }

        await _visitors.AppendAsync(visitor, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
