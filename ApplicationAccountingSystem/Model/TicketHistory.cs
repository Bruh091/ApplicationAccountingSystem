using System;
namespace ApplicationAccountingSystem.Model
{
    public class TicketHistory
    {
        public Guid Id { get; set; }
        public string Field { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public DateTime ChangedAt { get; set; }
        public Guid ChangedById { get; set; }
        public Guid TicketId { get; set; }
    }    
}