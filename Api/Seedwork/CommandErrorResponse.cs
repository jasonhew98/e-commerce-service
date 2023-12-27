using CSharpFunctionalExtensions;
using Domain;
using Newtonsoft.Json.Linq;
using System;
using System.Net;

namespace Api.Seedwork
{
    public class CommandErrorResponse
    {
        public readonly string ErrorCode;
        public readonly HttpStatusCode HttpStatusCode;
        public readonly string Message;
        public readonly JObject Context;

        public CommandErrorResponse(
            string errorCode,
            HttpStatusCode httpStatusCode,
            string message,
            JObject context = null)
        {
            ErrorCode = errorCode;
            HttpStatusCode = httpStatusCode;
            Message = message;
            Context = context;
        }

        public CommandErrorResponse(string errorCode, string message, HttpStatusCode httpStatusCode, dynamic context)
        {
            ErrorCode = errorCode;
            HttpStatusCode = httpStatusCode;
            Message = message;
            Context = (JObject)(context != null ? JObject.FromObject(context) : null);
        }

        public static CommandErrorResponse UnknownError(
            string message, string errorCodes = "UnknownError")
        {
            return new CommandErrorResponse(
                errorCode: errorCodes,
                httpStatusCode: HttpStatusCode.InternalServerError,
                message: message);
        }

        public static CommandErrorResponse BusinessError(
            DomainError domainError)
        {
            return new CommandErrorResponse(
                errorCode: domainError.Code,
                httpStatusCode: HttpStatusCode.BadRequest,
                message: domainError.Message);
        }

        public static CommandErrorResponse BusinessError(
            string message, string errorCodes = "ym-error")
        {
            return new CommandErrorResponse(
                errorCode: errorCodes,
                httpStatusCode: HttpStatusCode.BadRequest,
                message: message);
        }

        public static CommandErrorResponse NotFound(
            string message, string errorCodes = "NotFound")
        {
            return new CommandErrorResponse(
                errorCode: errorCodes,
                httpStatusCode: HttpStatusCode.NotFound,
                message: message);
        }

        public static CommandErrorResponse NotAuthorized(
            string message, string errorCodes = "NotAuthorized")
        {
            return new CommandErrorResponse(
                errorCode: errorCodes,
                httpStatusCode: HttpStatusCode.Unauthorized,
                message: message);
        }

        public static CommandErrorResponse Parse(string responseContent, HttpStatusCode statusCode)
        {
            if (string.IsNullOrEmpty(responseContent))
            {
                return new CommandErrorResponse(
                    errorCode: statusCode.ToString(),
                    message: "",
                    httpStatusCode: statusCode,
                    context: null);
            }

            var jo = JObject.Parse(responseContent);

            return new CommandErrorResponse(
                errorCode: jo["errorCode"]?.ToString(),
                message: jo["message"]?.ToString(),
                httpStatusCode: statusCode,
                context: jo["context"] as JObject);
        }

        public static implicit operator CommandErrorResponse(string errorMessage) => CommandErrorResponse.BusinessError(errorMessage);
    }

    public static class ResultYm
    {
        public static Result<T, CommandErrorResponse> Success<T>(T value)
        {
            return Result.Success<T, CommandErrorResponse>(value);
        }

        public static Result<T, CommandErrorResponse> NotAurthorized<T>(string message,
            string errorCodes = "NotAuthorized")
        {
            return Result.Failure<T, CommandErrorResponse>(CommandErrorResponse.NotAuthorized(message,
                errorCodes));
        }

        public static Result<T, CommandErrorResponse> NotFound<T>(string message,
            string errorCodes = "NotFound")
        {
            return Result.Failure<T, CommandErrorResponse>(CommandErrorResponse.NotFound(message,
                errorCodes));
        }

        public static Result<T, CommandErrorResponse> Error<T>(CommandErrorResponse error)
        {
            return Result.Failure<T, CommandErrorResponse>(error);
        }

        public static Result<T, CommandErrorResponse> Error<T>(DomainError domainError)
        {
            return Result.Failure<T, CommandErrorResponse>(
                CommandErrorResponse.BusinessError(domainError.Message, domainError.Code));
        }

        public static Result<T, CommandErrorResponse> Error<T>(Exception ex)
        {
            var innerException = ex.InnerException;

            if (innerException == null)
                return Result.Failure<T, CommandErrorResponse>(
                    CommandErrorResponse.UnknownError(ex.Message, ex.GetType().ToString()));
            else
            {
                var cer = new CommandErrorResponse(
                    errorCode: ex.GetType().ToString(),
                    message: ex.Message,
                    httpStatusCode: HttpStatusCode.InternalServerError,
                    context: new JObject
                    {
                        { "innerException", innerException.GetType().ToString() },
                        { "innerExceptionMessage", innerException.Message }
                    });
                return Result.Failure<T, CommandErrorResponse>(cer);
            }
        }

        public static Result<T, CommandErrorResponse> Error<T>(
            string businessMessage,
            Exception ex,
            string errorCode = "ym-error")
        {
            var context = new JObject
            {
                { "exceptionMessage", ex.Message },
                { "exception", ex.GetType().ToString() }
            };

            var innerException = ex.InnerException;
            if (innerException != null)
            {
                context.Add("innerException", innerException.GetType().ToString());
                context.Add("innerExceptionMessage", innerException.Message);
            }

            var cer = new CommandErrorResponse(
                errorCode: errorCode,
                message: $"{businessMessage} See context for more details.",
                httpStatusCode: HttpStatusCode.InternalServerError,
                context: context);

            return Result.Failure<T, CommandErrorResponse>(cer);
        }
    }

    public static class ResultForCommandErrorResponseExtension
    {
        public static Result<T, CommandErrorResponse> ConvertToCommandFailure<T>(this Result<T, DomainError> result)
        {
            return Result.Failure<T, CommandErrorResponse>(CommandErrorResponse.BusinessError(result.Error));
        }

        public static Result<TResults, CommandErrorResponse> ConvertToCommandFailure<T, TResults>(this Result<T, DomainError> result)
        {
            return Result.Failure<TResults, CommandErrorResponse>(CommandErrorResponse.BusinessError(result.Error));
        }
    }
}
