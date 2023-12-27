using Api.Seedwork;
using CSharpFunctionalExtensions;
using System.Threading.Tasks;

namespace Api.Infrastructure.Services
{
    public interface FileService
    {
        Result<string, CommandErrorResponse> GetFormattedDownloadUrl(string url);
        Result<string, CommandErrorResponse> GetEncodedUrl(string url);
        Result<string, CommandErrorResponse> GetFileIdFromUrl(string url);
        Task<Result<byte[], CommandErrorResponse>> DownloadFileFromUrl(string url);
        Task<Result<bool, CommandErrorResponse>> CreateFile(string base64, string fileName, string folderPath);
    }
}
