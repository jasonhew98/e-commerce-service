using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediatR;
using Api.Seedwork;
using System.Net;
using Api.Model;

namespace Api.Features.User
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public UserController(
            ILogger<UserController> logger,
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet]
        [Route("")]
        [ProducesResponseType(typeof(UserDto[]), (int)HttpStatusCode.Accepted)]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string sortBy,
            [FromQuery] int sortOrder,
            [FromQuery] int currentPage,
            [FromQuery] int pageSize)
        {
            var command = new GetUsersQuery(
                sortBy: sortBy,
                sortOrder: sortOrder,
                currentPage: currentPage,
                pageSize: pageSize);

            return this.OkOrError(await _mediator.Send(command));
        }

        [HttpGet]
        [Route("pageSize")]
        [ProducesResponseType(typeof(PageSizeDto), (int)HttpStatusCode.Accepted)]
        public async Task<IActionResult> GetUserPageSize(
            [FromQuery] int pageSize)
        {
            var command = new GetUserPageSizeQuery(
                pageSize: pageSize);

            return this.OkOrError(await _mediator.Send(command));
        }

        [HttpGet]
        [Route("detail")]
        [ProducesResponseType(typeof(UserDetailDto), (int)HttpStatusCode.Accepted)]
        public async Task<IActionResult> GetUser(
            [FromQuery] string userId)
        {
            var command = new GetUserQuery(
                userId: userId);

            return this.OkOrError(await _mediator.Send(command));
        }

        [HttpPatch]
        [ProducesResponseType(typeof(UpdatedUserDto), (int)HttpStatusCode.Accepted)]
        public async Task<IActionResult> UpdateUser(
            [FromBody] UpdateUserCommand command)
        {
            return this.OkOrError(await _mediator.Send(command));
        }
    }
}
