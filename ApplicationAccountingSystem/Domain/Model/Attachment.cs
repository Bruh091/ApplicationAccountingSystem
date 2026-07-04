using System;
namespace ApplicationAccountingSystem.Domain.Model
{
    public class Attachment
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public long FileSize { get; set; }
        public string ContentType { get; set; } = "";
        public DateTime UploadedAt { get; set; } = DateTime.Now;
        public Guid TicketId { get; set; }
    }
}
