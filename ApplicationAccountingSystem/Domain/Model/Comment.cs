using System;
namespace ApplicationAccountingSystem.Domain.Model
{
    public class Comment
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsInternal { get; set; } = false;
        public Guid UserId { get; set; }
        public Guid TicketId { get; set; }
        public string GetPreview()
        {
            if (Content.Length <= 67)
            {
                return Content;
            }
            return Content.Substring(0, 67) + "...";
        }
    }
}
