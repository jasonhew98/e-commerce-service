using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediatR;
using Api.Seedwork;
using System.Net;
using Api.Model;

namespace Api.Features.Product
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : Controller
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public ProductController(
            ILogger<ProductController> logger,
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpPost]
        [Route("")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.Accepted)]
        public async Task<IActionResult> AddProduct([FromBody] AddProductCommand command)
        {
            return this.OkOrError(await _mediator.Send(command));
        }

        [HttpGet]
        [Route("")]
        [ProducesResponseType(typeof(ProductDto[]), (int)HttpStatusCode.Accepted)]
        public async Task<IActionResult> GetProducts(
            [FromQuery]string sortBy,
            [FromQuery]int sortOrder,
            [FromQuery]int currentPage,
            [FromQuery]int pageSize)
        {
            var command = new GetProductsQuery(
                sortBy: sortBy,
                sortOrder: sortOrder,
                currentPage: currentPage,
                pageSize: pageSize);

            return this.OkOrError(await _mediator.Send(command));
        }

        [HttpGet]
        [Route("pageSize")]
        [ProducesResponseType(typeof(PageSizeDto), (int)HttpStatusCode.Accepted)]
        public async Task<IActionResult> GetProductPageSize(
            [FromQuery] int pageSize)
        {
            var command = new GetProductPageSizeQuery(
                pageSize: pageSize);

            return this.OkOrError(await _mediator.Send(command));
        }

        [HttpGet]
        [Route("detail")]
        [ProducesResponseType(typeof(ProductDetailDto), (int)HttpStatusCode.Accepted)]
        public async Task<IActionResult> GetProduct(
            [FromQuery] string productId)
        {
            var command = new GetProductQuery(
                productId: productId);

            return this.OkOrError(await _mediator.Send(command));
        }

        [HttpPatch]
        [ProducesResponseType(typeof(UpdatedProductDto), (int)HttpStatusCode.Accepted)]
        public async Task<IActionResult> UpdateProduct(
            [FromBody] UpdateProductCommand command)
        {
            return this.OkOrError(await _mediator.Send(command));
        }
    }
}
