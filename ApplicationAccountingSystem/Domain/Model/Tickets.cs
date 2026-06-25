using ApplicationAccountingSystem.Domain.Designation;
using System;

namespace ApplicationAccountingSystem.Domain.Model
{
    public class Tickets
{
    public Guid Id { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public DateTime? ResolvedAt { get; set; }

    public Guid CreatedById { get; set; }

    public Guid? AssignedToId { get; set; }

    public Guid? SLAPolicyId { get; set; }
}
}