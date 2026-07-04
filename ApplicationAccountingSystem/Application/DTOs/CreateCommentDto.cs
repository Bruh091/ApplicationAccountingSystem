using System;

namespace ApplicationAccountingSystem.Application.DTOs
{
    public class CreateCommentDto
    {
        public string Content { get; set; } = "";

        public bool IsInternal { get; set; }

        public Guid UserId { get; set; }

        public Guid TicketId { get; set; }
    }
}
