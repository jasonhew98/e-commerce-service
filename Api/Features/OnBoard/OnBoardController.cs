using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediatR;
using Api.Seedwork;
using System.Net;
using Api.Model;

namespace Api.Features.OnBoard
{
    [Route("api/[controller]")]
    [ApiController]
    public class OnBoardController : Controller
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public OnBoardController(
            ILogger<OnBoardController> logger,
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpPost]
        [Route("signup")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.Accepted)]
        public async Task<IActionResult> SignUp([FromBody] SignUpCommand command)
        {
            return this.OkOrError(await _mediator.Send(command));
        }

        [HttpPost]
        [Route("login")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.Accepted)]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            return this.OkOrError(await _mediator.Send(command));
        }
    }
}
