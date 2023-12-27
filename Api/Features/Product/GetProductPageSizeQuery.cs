using Api.Model;
using Api.Seedwork;
using CSharpFunctionalExtensions;
using Domain.AggregatesModel.ProductAggregate;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Product
{
    public class GetProductPageSizeQuery : IRequest<Result<PageSizeDto, CommandErrorResponse>>
    {
        public int PageSize { get; }

        public GetProductPageSizeQuery(
            int pageSize)
        {
            PageSize = pageSize;
        }
    }

    public class GetProductPageSizeQueryHandler : IRequestHandler<GetProductPageSizeQuery, Result<PageSizeDto, CommandErrorResponse>>
    {
        private readonly ILogger _logger;
        private readonly IProductRepository _productRepository;

        public GetProductPageSizeQueryHandler(
            ILogger<GetProductPageSizeQueryHandler> logger,
            IProductRepository productRepository)
        {
            _logger = logger;
            _productRepository = productRepository;
        }

        public async Task<Result<PageSizeDto, CommandErrorResponse>> Handle(GetProductPageSizeQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var count = await _productRepository.GetProductCount();

                var result = new PageSizeDto(count, request.PageSize);

                return ResultYm.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error has occured while trying to get product page size.");
                return ResultYm.Error<PageSizeDto>(ex);
            }
        }
    }
}
