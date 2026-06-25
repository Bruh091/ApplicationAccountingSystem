using System;
using ApplicationAccountingSystem.Designation;
namespace ApplicationAccountingSystem.Model
{
    public class SLAPolicy
    {
        public Guid Id { get; set; }
        public TicketPriority Priority { get; set; }
        public int ResponseTimeInHours { get; set; }
        public int ResolutionTimeInHours { get; set; }
        public bool IsActive { get; set; } = true;
    }
}