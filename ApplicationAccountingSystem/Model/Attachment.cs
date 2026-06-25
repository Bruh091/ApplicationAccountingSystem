using System;
namespace ApplicationAccountingSystem.Model.Attachment
{
    public class Attachment
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public string ContentType { get; set; }
        public DateTime UploadedAt { get; set; }
        public Guid TicketId { get; set; }
    }
}