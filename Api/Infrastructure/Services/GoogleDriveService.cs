using Api.Seedwork;
using CSharpFunctionalExtensions;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;

namespace Api.Infrastructure.Services
{
    public interface GoogleDriveService
    {
        Task<Result<MemoryStream, CommandErrorResponse>> DriveDownloadFileFromServiceAccount(JObject serviceAccCreds, string fileId);
        Task<Result<MemoryStream, CommandErrorResponse>> DriveDownloadFileFromAccessToken(string accessToken, string fileId);
    }
}
