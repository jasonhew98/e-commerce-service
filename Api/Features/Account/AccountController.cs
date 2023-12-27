using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediatR;
using Api.Seedwork;
using System.Net;
using Api.Model;

namespace Api.Features.Account
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public AccountController(
            ILogger<AccountController> logger,
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpPost]
        [Route("")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.Accepted)]
        public async Task<IActionResult> AddAccount([FromBody] AddAccountCommand command)
        {
            return this.OkOrError(await _mediator.Send(command));
        }

        [HttpGet]
        [Route("")]
        [ProducesResponseType(typeof(AccountDto[]), (int)HttpStatusCode.Accepted)]
        public async Task<IActionResult> GetAccounts(
            [FromQuery] string sortBy,
            [FromQuery] int sortOrder,
            [FromQuery] int currentPage,
            [FromQuery] int pageSize)
        {
            var command = new GetAccountsQuery(
                sortBy: sortBy,
                sortOrder: sortOrder,
                currentPage: currentPage,
                pageSize: pageSize);

            return this.OkOrError(await _mediator.Send(command));
        }

        [HttpGet]
        [Route("pageSize")]
        [ProducesResponseType(typeof(PageSizeDto), (int)HttpStatusCode.Accepted)]
        public async Task<IActionResult> GetAccountPageSize(
            [FromQuery] int pageSize)
        {
            var command = new GetAccountPageSizeQuery(
                pageSize: pageSize);

            return this.OkOrError(await _mediator.Send(command));
        }

        [HttpGet]
        [Route("detail")]
        [ProducesResponseType(typeof(AccountDetailDto), (int)HttpStatusCode.Accepted)]
        public async Task<IActionResult> GetAccount(
            [FromQuery] string accountId)
        {
            var command = new GetAccountQuery(
                accountId: accountId);

            return this.OkOrError(await _mediator.Send(command));
        }

        [HttpPatch]
        [ProducesResponseType(typeof(UpdatedAccountDto), (int)HttpStatusCode.Accepted)]
        public async Task<IActionResult> UpdateAccount(
            [FromBody] UpdateAccountCommand command)
        {
            return this.OkOrError(await _mediator.Send(command));
        }
    }
}
