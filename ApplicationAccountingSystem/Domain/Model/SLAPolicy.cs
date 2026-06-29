using System;
using ApplicationAccountingSystem.Domain.Designation;
namespace ApplicationAccountingSystem.Domain.Model
{
    public class SLAPolicy
    {
        public Guid Id { get; set; }
        public TicketPriority Priority { get; set; }
        public int ResponseTimeInHours { get; set; }
        public int ResolutionTimeInHours { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CalculateDeadline(DateTime createAt)
        {
            return createAt.AddHours(ResolutionTimeInHours);
        }
        public bool CheckViolatoin(DateTime createAt, DateTime? resolvedAt = null)
        {
            var deadline = CalculateDeadline(createAt);
            var compareDate = resolvedAt ?? DateTime.UtcNow;
            return compareDate > deadline;
        }
    }
}