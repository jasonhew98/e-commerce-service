namespace Domain.Model
{
    public class Attachment
    {
        public string AttachmentId { get; private set; }
        public string Name { get; private set; }
        public string BlobType { get; private set; }

        public Attachment(
            string attachmentId = null,
            string name = null,
            string blobType = null)
        {
            AttachmentId = attachmentId;
            Name = name;
            BlobType = blobType;
        }
    }
}
