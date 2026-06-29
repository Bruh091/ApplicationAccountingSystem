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
    public void AssignTo(Guid agentId)
        {
            AssignedToId = agentId;
            Status = TicketStatus.InProgress;
            UpdatedAt = DateTime.UtcNow;
        }
    public void ChangeStatus(TicketStatus status)
        {
            Status = status;
            UpdatedAt = DateTime.UtcNow;
            if (status == TicketStatus.Resolved)
            {
                ResolvedAt = DateTime.UtcNow;
            }
        }
    public void ChangePriority(TicketPriority priority)
        {
            Priority = priority;
            UpdatedAt = DateTime.UtcNow;
        }
    public TimeSpan? GetTimeToResolution()
        {
            if (ResolvedAt == null)
            {
                return null;
            }
            return ResolvedAt.Value - CreatedAt;
        } 
    public bool IsOverdue(SLAPolicy slaPolicy)
        {
            var deadline = CreatedAt.AddHours(slaPolicy.ResolutionTimeInHours);
            if (DateTime.UtcNow > deadline && Status != TicketStatus.Resolved && Status != TicketStatus.Closed)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
}
    


}