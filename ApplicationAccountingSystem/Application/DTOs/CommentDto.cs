using System;

namespace ApplicationAccountingSystem.Application.DTOs
{
    public class CommentDto
    {
        public Guid Id { get; set; }

        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsInternal { get; set; }

        public Guid UserId { get; set; }

        public Guid TicketId { get; set; }
    }
}