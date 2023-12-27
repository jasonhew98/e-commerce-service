using Api.Seedwork;
using CSharpFunctionalExtensions;
using System.IO;
using System.Threading.Tasks;

namespace Api.Infrastructure.Services
{
    public interface OneDriveService
    {
        Task<Result<byte[], CommandErrorResponse>> DriveDownloadFileFromAccessToken(string accessToken, string encodedUrl);
    }
}
