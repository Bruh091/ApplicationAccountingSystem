using System;
namespace ApplicationAccountingSystem.Domain.Model
{
    public class TicketHistory
    {
        public Guid Id { get; set; }
        public string Field { get; set; } = "";
        public string OldValue { get; set; } = "";
        public string NewValue { get; set; } = "";
        public DateTime ChangedAt { get; set; } = DateTime.Now;
        public Guid ChangedById { get; set; }
        public Guid TicketId { get; set; }
    }    
}
