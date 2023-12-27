using System.Linq;

namespace Api.Model
{
    public class AddAttachment
    {
        public string AttachmentFileName { get; }
        private readonly string _attachmentFileExtension;
        public string AttachmentFileExtension =>
            string.IsNullOrEmpty(_attachmentFileExtension)
            ? AttachmentFileName?.Split(".").Last()
            : _attachmentFileExtension;

        public string AttachmentBase64 { get; }
        public string BlobType { get; }
        
        public AddAttachment(
            string attachmentBase64,
            string attachmentFileName,
            string blobType,
            string attachmentFileExtension = "")
        {
            AttachmentBase64 = attachmentBase64;
            AttachmentFileName = attachmentFileName;
            _attachmentFileExtension = attachmentFileExtension;
            BlobType = blobType;
        }
    }
}
