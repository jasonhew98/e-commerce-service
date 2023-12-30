using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediatR;
using Api.Seedwork;
using System.Net;
using Newtonsoft.Json.Linq;
using System;

namespace Api.Internal.Trigger
{
    [Route("api/Internal/Trigger")]
    [ApiController]
    public class TriggerController : Controller
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public TriggerController(
            ILogger<TriggerController> logger,
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.Accepted)]
        public async Task<IActionResult> Trigger([FromBody] JObject commandObj)
        {
            try
            {
                TriggerEventCommand command = commandObj.ToObject<TriggerEventCommand>();
                return this.OkOrError(await _mediator.Send(command));
            }
            catch (Exception ex)
            {
                return this.OkOrError(ResultYm.Error<bool>(ex));
            }
        }
    }
}
