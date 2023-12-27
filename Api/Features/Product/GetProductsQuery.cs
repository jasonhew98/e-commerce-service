using Api.Seedwork;
using CSharpFunctionalExtensions;
using Domain.AggregatesModel.ProductAggregate;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Product
{
    public class GetProductsQuery : IRequest<Result<List<ProductDto>, CommandErrorResponse>>
    {
        public string SortBy { get; }
        public int SortOrder { get; }
        public int PageSize { get; }
        public int CurrentPage { get; }

        public GetProductsQuery(
            string sortBy,
            int sortOrder,
            int pageSize,
            int currentPage)
        {
            SortBy = sortBy;
            SortOrder = sortOrder;
            PageSize = pageSize;
            CurrentPage = currentPage;
        }
    }

    public class ProductDto
    {
        public string ProductId { get; }
        public string ProductName { get; }
        public DateTime? ModifiedUTCDateTime { get; }

        public ProductDto(
            string productId,
            string productName,
            DateTime? modifiedUTCDateTime)
        {
            ProductId = productId;
            ProductName = productName;
            ModifiedUTCDateTime = modifiedUTCDateTime;
        }
    }

    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, Result<List<ProductDto>, CommandErrorResponse>>
    {
        private readonly ILogger _logger;
        private readonly IProductRepository _productRepository;

        public GetProductsQueryHandler(
            ILogger<GetProductsQueryHandler> logger,
            IProductRepository productRepository)
        {
            _logger = logger;
            _productRepository = productRepository;
        }

        public async Task<Result<List<ProductDto>, CommandErrorResponse>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var products = await _productRepository.GetProducts(
                    limit: request.PageSize,
                    offset: request.PageSize * (request.CurrentPage < 1 ? 0 : request.CurrentPage - 1),
                    sortBy: request.SortBy,
                    sortOrder: request.SortOrder);

                if (products == null)
                    return ResultYm.Success(new List<ProductDto>());

                var result = products.Select(p => CreateFromDomain(p)).ToList();

                return ResultYm.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error has occured while trying to get product list.");
                return ResultYm.Error<List<ProductDto>>(ex);
            }
        }

        public ProductDto CreateFromDomain(Domain.AggregatesModel.ProductAggregate.Product product)
        {
            return new ProductDto(
                productId: product.ProductId,
                productName: product.ProductName,
                modifiedUTCDateTime: product.ModifiedUTCDateTime);
        }
    }
}
