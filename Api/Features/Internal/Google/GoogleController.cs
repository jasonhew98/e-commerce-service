using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediatR;
using Api.Seedwork;
using System.Net;
using Newtonsoft.Json.Linq;
using System;
using Google.Apis.Auth.OAuth2.Responses;

namespace Api.Internal.Google
{
    [Route("api/Internal/Google")]
    [ApiController]
    public class GoogleController : Controller
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public GoogleController(
            ILogger<GoogleController> logger,
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpPost]
        [Route("Refresh")]
        [ProducesResponseType(typeof(TokenResponse), (int)HttpStatusCode.Accepted)]
        public async Task<IActionResult> RefreshAccessToken([FromBody] JObject commandObj)
        {
            try
            {
                RefreshAccessTokenCommand command = commandObj.ToObject<RefreshAccessTokenCommand>();
                return this.OkOrError(await _mediator.Send(command));
            }
            catch (Exception ex)
            {
                return this.OkOrError(ResultYm.Error<bool>(ex));
            }
        }

        [HttpPost]
        [Route("File/Download")]
        [ProducesResponseType(typeof(TokenResponse), (int)HttpStatusCode.Accepted)]
        public async Task<IActionResult> DownloadDriveFile([FromBody] JObject commandObj)
        {
            try
            {
                DownloadDriveFileCommand command = commandObj.ToObject<DownloadDriveFileCommand>();
                return this.OkOrError(await _mediator.Send(command));
            }
            catch (Exception ex)
            {
                return this.OkOrError(ResultYm.Error<bool>(ex));
            }
        }
    }
}
