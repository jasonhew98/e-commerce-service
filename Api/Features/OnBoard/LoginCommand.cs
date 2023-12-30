using Api.Seedwork;
using Api.Seedwork.AesEncryption;
using CSharpFunctionalExtensions;
using Domain;
using Domain.AggregatesModel.UserAggregate;
using FluentValidation;
using Infrastructure.Seedwork;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Features.OnBoard
{
    public class LoginCommand : IRequest<Result<bool, CommandErrorResponse>>
    {
        public string UserName { get; }
        public string Password { get; }
    }

    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(a => a.UserName).NotEmpty();
            RuleFor(a => a.Password).NotEmpty();
        }
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<bool, CommandErrorResponse>>
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IAesSecurity _aes;

        public LoginCommandHandler(
            ILogger<LoginCommandHandler> logger,
            IUnitOfWork unitOfWork,
            IAesSecurity aes,
            IUserRepository userRepository)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _aes = aes;
            _userRepository = userRepository;
        }

        public async Task<Result<bool, CommandErrorResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userRepository.QueryOne(x => x.UserName == request.UserName);
                if (user == null)
                    return ResultYm.NotFound<bool>("User not found.");

                var encryptPassword = _aes.Encrypt(request.Password);
                if (!encryptPassword.Equals(user.Password))
                    return ResultYm.Error<bool>(CommandErrorResponse.BusinessError(BusinessError.FailToAuthenticate__IncorrectPassword.Error()));

                await _unitOfWork.Commit();

                return ResultYm.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unknown error has occured while trying to login.");
                return ResultYm.Error<bool>(ex);
            }
        }
    }
}
