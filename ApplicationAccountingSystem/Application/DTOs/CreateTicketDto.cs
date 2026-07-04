using System;
using ApplicationAccountingSystem.Domain.Designation;

namespace ApplicationAccountingSystem.Application.DTOs
{
    public class CreateTicketDto
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public TicketPriority Priority { get; set; }
        public Guid CreatedById { get; set; }
    }
}
