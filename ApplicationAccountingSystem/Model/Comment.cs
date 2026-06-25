using System;
namespace ApplicationAccountingSystem.Model
{
    public class Comment
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsInternal { get; set; } = false;
        public Guid UserId { get; set; }
        public Guid TicketId { get; set; }
    }
}