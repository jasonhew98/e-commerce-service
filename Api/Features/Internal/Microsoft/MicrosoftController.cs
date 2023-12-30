using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediatR;
using Api.Seedwork;
using System.Net;
using Newtonsoft.Json.Linq;
using System;
using Google.Apis.Auth.OAuth2.Responses;

namespace Api.Internal.Microsoft
{
    [Route("api/Internal/Microsoft")]
    [ApiController]
    public class MicrosoftController : Controller
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public MicrosoftController(
            ILogger<MicrosoftController> logger,
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
    }
}
