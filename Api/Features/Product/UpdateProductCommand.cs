using Api.Seedwork;
using CSharpFunctionalExtensions;
using Domain;
using Domain.AggregatesModel.ProductAggregate;
using FluentValidation;
using Infrastructure.Seedwork;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.Product
{
    public class UpdateProductCommand : IRequest<Result<UpdatedProductDto, CommandErrorResponse>>
    {
        public string ProductId { get; }
        public string ProductName { get; }
        public double ProductPrice { get; }
        public DateTime ModifiedUTCDateTime { get; }

        public UpdateProductCommand(
            string productId,
            string productName,
            double productPrice,
            DateTime modifiedUTCDateTime)
        {
            ProductId = productId;
            ProductName = productName;
            ProductPrice = productPrice;
            ModifiedUTCDateTime = modifiedUTCDateTime;
        }
    }

    public class UpdatedProductDto
    {
        public string ProductId { get; }

        public UpdatedProductDto(
            string productId)
        {
            ProductId = productId;
        }
    }

    public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(a => a.ProductName).NotEmpty();
            RuleFor(a => a.ProductPrice).NotEmpty();
            RuleFor(a => a.ModifiedUTCDateTime).NotNull().NotEmpty();
        }
    }

    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<UpdatedProductDto, CommandErrorResponse>>
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductRepository _productRepository;

        public UpdateProductCommandHandler(
            ILogger<UpdateProductCommandHandler> logger,
            IUnitOfWork unitOfWork,
            IProductRepository productRepository)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _productRepository = productRepository;
        }

        public async Task<Result<UpdatedProductDto, CommandErrorResponse>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var product = await _productRepository.GetProduct(productId: request.ProductId);

                if (product == null)
                    return ResultYm.NotFound<UpdatedProductDto>("Product not found.");

                if (!request.ModifiedUTCDateTime.Equals(product.ModifiedUTCDateTime))
                    return ResultYm.Error<UpdatedProductDto>(CommandErrorResponse.BusinessError(BusinessError.ConcurrencyUpdate.Error()));

                product.UpdateProductDetails(
                    new Domain.AggregatesModel.ProductAggregate.Product(
                        productName: request.ProductName,
                        productPrice: request.ProductPrice
                    )
                );

                await _productRepository.UpdateProduct(
                    product: product,
                    user: ("System", "System"),
                    productId: request.ProductId);

                await _unitOfWork.Commit();

                return ResultYm.Success(new UpdatedProductDto(product.ProductId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unknown error has occured while trying to update product details.");
                return ResultYm.Error<UpdatedProductDto>(ex);
            }
        }
    }
}
