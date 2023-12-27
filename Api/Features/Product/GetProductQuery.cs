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
    public class GetProductQuery : IRequest<Result<ProductDetailDto, CommandErrorResponse>>
    {
        public string ProductId { get; }

        public GetProductQuery(
            string productId)
        {
            ProductId = productId;
        }
    }

    public class ProductDetailDto
    {
        public string ProductId { get; }
        public string ProductName { get; }
        public DateTime? ModifiedUTCDateTime { get; }

        public ProductDetailDto(
            string productId,
            string productName,
            DateTime? modifiedUTCDateTime)
        {
            ProductId = productId;
            ProductName = productName;
            ModifiedUTCDateTime = modifiedUTCDateTime;
        }
    }

    public class GetProductQueryHandler : IRequestHandler<GetProductQuery, Result<ProductDetailDto, CommandErrorResponse>>
    {
        private readonly ILogger _logger;
        private readonly IProductRepository _productRepository;

        public GetProductQueryHandler(
            ILogger<GetProductQueryHandler> logger,
            IProductRepository productRepository)
        {
            _logger = logger;
            _productRepository = productRepository;
        }

        public async Task<Result<ProductDetailDto, CommandErrorResponse>> Handle(GetProductQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var product = await _productRepository.GetProduct(
                    productId: request.ProductId);

                if (product == null)
                    return ResultYm.NotFound<ProductDetailDto>("Product not found.");

                var result = CreateFromDomain(product);

                return ResultYm.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error has occured while trying to get product detail.");
                return ResultYm.Error<ProductDetailDto>(ex);
            }
        }

        public ProductDetailDto CreateFromDomain(Domain.AggregatesModel.ProductAggregate.Product product)
        {
            return new ProductDetailDto(
                productId: product.ProductId,
                productName: product.ProductName,
                modifiedUTCDateTime: product.ModifiedUTCDateTime);
        }
    }
}
