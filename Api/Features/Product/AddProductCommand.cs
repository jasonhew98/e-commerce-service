using Api.Seedwork;
using CSharpFunctionalExtensions;
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
    public class AddProductCommand : IRequest<Result<bool, CommandErrorResponse>>
    {
        public string ProductId { get; }
        public string ProductName { get; }
        public double ProductPrice { get; }

        public AddProductCommand(
            string productName,
            double productPrice)
        {
            ProductId = Guid.NewGuid().ToString("N");
            ProductName = productName;
            ProductPrice = productPrice;
        }
    }

    public class AddProductCommandValidator : AbstractValidator<AddProductCommand>
    {
        public AddProductCommandValidator()
        {
            RuleFor(a => a.ProductName).NotEmpty();
        }
    }

    public class AddProductCommandHandler : IRequestHandler<AddProductCommand, Result<bool, CommandErrorResponse>>
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductRepository _productRepository;

        public AddProductCommandHandler(
            ILogger<AddProductCommandHandler> logger,
            IUnitOfWork unitOfWork,
            IProductRepository productRepository)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _productRepository = productRepository;
        }

        public async Task<Result<bool, CommandErrorResponse>> Handle(AddProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var product = new Domain.AggregatesModel.ProductAggregate.Product(
                    productId: request.ProductId,
                    productName: request.ProductName,
                    productPrice: request.ProductPrice,
                    createdBy: "System",
                    createdByName: "System",
                    createdUTCDateTime: DateTime.Now,
                    modifiedBy: "System",
                    modifiedByName: "System",
                    modifiedUTCDateTime: DateTime.Now);

                _productRepository.Add(product);

                await _unitOfWork.Commit();

                return ResultYm.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unknown error has occured while trying to add new product.");
                return ResultYm.Error<bool>(ex);
            }
        }
    }
}
