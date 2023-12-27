using Api.Seedwork;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Api.Infrastructure.Services
{
    public class ApiFileService : FileService
    {
        private readonly ILogger _logger;
        private readonly BaseAddressConfigurationOptions _baseAddressConfiguration;

        public ApiFileService(
            IOptions<BaseAddressConfigurationOptions> baseAddressConfiguration,
            ILogger<ApiFileService> logger)
        {
            _baseAddressConfiguration = baseAddressConfiguration.Value;
            _logger = logger;
        }

        public async Task<Result<bool, CommandErrorResponse>> CreateFile(string base64, string fileName, string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                    throw new DirectoryNotFoundException($@"Directory path does not exist: {folderPath}");

                await File.WriteAllBytesAsync(Path.Combine(folderPath, fileName), Convert.FromBase64String(base64));

                return ResultYm.Success(true);
            }
            catch (Exception ex)
            {
                return ResultYm.Error<bool>(ex);
            }
        }

        public async Task<Result<byte[], CommandErrorResponse>> DownloadFileFromUrl(string url)
        {
            using (var client = new HttpClient())
            {
                using (var result = await client.GetAsync(url))
                {
                    if (result.IsSuccessStatusCode)
                    {
                        var trueBytes = await result.Content.ReadAsByteArrayAsync();
                        return trueBytes;
                    }
                    else
                    {
                        return ResultYm.Error<byte[]>(result.ReasonPhrase + " " + result.Content.ReadAsStringAsync());
                    }
                }
            }
        }

        public Result<string, CommandErrorResponse> GetEncodedUrl(string url)
        {
            string base64Value = Convert.ToBase64String(Encoding.UTF8.GetBytes(url));
            string encodedUrl = "u!" + base64Value.TrimEnd('=').Replace('/', '_').Replace('+', '-');
            return encodedUrl;
        }

        public Result<string, CommandErrorResponse> GetFileIdFromUrl(string url)
        {
            var gDriveBaseAddress = _baseAddressConfiguration.GoogleDrive;
            var gSpreadsheetBaseAddress = _baseAddressConfiguration.GoogleSpreadsheet;

            if (url.Contains(gDriveBaseAddress))
            {
                url = url.Replace(gDriveBaseAddress, "");
                var fileId = url.Substring(0, url.IndexOf('/'));

                return fileId;
            }

            if (url.Contains(gSpreadsheetBaseAddress))
            {
                url = url.Replace(gSpreadsheetBaseAddress, "");
                var fileId = url.Substring(0, url.IndexOf('/'));

                return fileId;
            }

            //Unlikely to end up here
            return ResultYm.Error<string>("");
        }

        public Result<string, CommandErrorResponse> GetFormattedDownloadUrl(string url)
        {
            var fileId = GetFileIdFromUrl(url);
            if (string.IsNullOrEmpty(fileId.Value))
                return ResultYm.Error<string>("");

            var gSpreadsheetBaseAddress = _baseAddressConfiguration.GoogleSpreadsheet;
            var downloadUrl = gSpreadsheetBaseAddress + fileId + "/export";

            return downloadUrl.ToString();
        }
    }
}
