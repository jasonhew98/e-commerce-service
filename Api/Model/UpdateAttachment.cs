namespace Api.Model
{
    public class UpdateAttachment : AddAttachment
    {
        public string AttachmentId { get; }

        public UpdateAttachment(
            string attachmentBase64,
            string attachmentFileName,
            string blobType,
            string attachmentFileExtension,
            string attachmentId = "")
            : base(
                  attachmentBase64: attachmentBase64,
                  attachmentFileName: attachmentFileName,
                  blobType: blobType,
                  attachmentFileExtension: attachmentFileExtension)
        {
            AttachmentId = attachmentId;
        }
    }
}
