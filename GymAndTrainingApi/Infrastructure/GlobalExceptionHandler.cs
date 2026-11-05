using FluentValidation;
using Gym.Application.Common.Exceptions;
using Gym.Domain.Common.Errors;
using Gym.Domain.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gym.Api.Infrastructure
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (httpContext.Response.HasStarted)
                return false;

            if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "Request cancelled for {Method} {Path}",
                    httpContext.Request.Method,
                    httpContext.Request.Path);

                return true;
            }

            var (problemDetails, statusCode, code, logLevel) = Map(exception, httpContext);
            if (problemDetails is null)
                return false;

            httpContext.Response.StatusCode = statusCode;
            problemDetails.Status = statusCode;
            problemDetails.Instance = httpContext.Request.Path;
            problemDetails.Extensions["traceId"] = System.Diagnostics.Activity.Current?.Id
                ?? httpContext.TraceIdentifier;

            if (!string.IsNullOrWhiteSpace(code))
                problemDetails.Extensions["code"] = code;

            Log(exception, statusCode, code, logLevel, httpContext);

            var problemDetailsService = httpContext.RequestServices.GetService<IProblemDetailsService>();

            if (problemDetailsService is not null)
            {
                var written = await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
                {
                    HttpContext = httpContext,
                    Exception = exception,
                    ProblemDetails = problemDetails
                });

                if (written)
                    return true;
            }

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }

        private static (ProblemDetails? Problem, int StatusCode, string? Code, LogLevel LogLevel) Map(
            Exception exception,
            HttpContext httpContext)
        {
            switch (exception)
            {
                case ValidationException validationException:
                {
                    var errors = validationException.Errors
                        .GroupBy(failure => failure.PropertyName, failure => failure.ErrorMessage)
                        .ToDictionary(group => group.Key, group => group.ToArray());

                    var problem = new ValidationProblemDetails(errors)
                    {
                        Title = ErrorCatalog.Get(ErrorCodes.ValidationFailed).Title,
                        Type = ErrorCatalog.TypeFor(ErrorCodes.ValidationFailed),
                        Detail = "One or more fields failed validation."
                    };

                    return (problem, 400, ErrorCodes.ValidationFailed, LogLevel.Information);
                }

                case DomainException domainException:
                {
                    var descriptor = ErrorCatalog.Get(domainException.Code);
                    return (
                        CreateProblem(domainException.Code, descriptor.Title, domainException.Message),
                        descriptor.StatusCode,
                        domainException.Code,
                        descriptor.StatusCode >= 500 ? LogLevel.Error : LogLevel.Warning);
                }

                case AppException appException:
                {
                    var descriptor = ErrorCatalog.Get(appException.Code);
                    return (
                        CreateProblem(appException.Code, descriptor.Title, appException.Message),
                        descriptor.StatusCode,
                        appException.Code,
                        descriptor.StatusCode >= 500 ? LogLevel.Error : LogLevel.Warning);
                }

                case KeyNotFoundException keyNotFoundException:
                {
                    var descriptor = ErrorCatalog.Get(ErrorCodes.ResourceNotFound);
                    return (
                        CreateProblem(ErrorCodes.ResourceNotFound, descriptor.Title, keyNotFoundException.Message),
                        descriptor.StatusCode,
                        ErrorCodes.ResourceNotFound,
                        LogLevel.Warning);
                }

                case DbUpdateConcurrencyException:
                {
                    var descriptor = ErrorCatalog.Get(ErrorCodes.ConcurrencyConflict);
                    return (
                        CreateProblem(
                            ErrorCodes.ConcurrencyConflict,
                            descriptor.Title,
                            "The data was modified by another operation. Please retry."),
                        descriptor.StatusCode,
                        ErrorCodes.ConcurrencyConflict,
                        LogLevel.Warning);
                }

                case BadHttpRequestException:
                {
                    var descriptor = ErrorCatalog.Get(ErrorCodes.RequestMalformed);
                    return (
                        CreateProblem(ErrorCodes.RequestMalformed, descriptor.Title, "The request body is malformed."),
                        descriptor.StatusCode,
                        ErrorCodes.RequestMalformed,
                        LogLevel.Warning);
                }

                case UnauthorizedAccessException:
                {
                    var descriptor = ErrorCatalog.Get(ErrorCodes.AuthUnauthorized);
                    return (
                        CreateProblem(ErrorCodes.AuthUnauthorized, descriptor.Title, "Authentication is required."),
                        descriptor.StatusCode,
                        ErrorCodes.AuthUnauthorized,
                        LogLevel.Warning);
                }

                case OperationCanceledException:
                {
                    var descriptor = ErrorCatalog.Get(ErrorCodes.RequestCancelled);
                    return (
                        CreateProblem(ErrorCodes.RequestCancelled, descriptor.Title, "The request was cancelled."),
                        descriptor.StatusCode,
                        ErrorCodes.RequestCancelled,
                        LogLevel.Warning);
                }

                case ArgumentException argumentException:
                {
                    var descriptor = ErrorCatalog.Get(ErrorCodes.RequestInvalid);
                    return (
                        CreateProblem(ErrorCodes.RequestInvalid, descriptor.Title, argumentException.Message),
                        descriptor.StatusCode,
                        ErrorCodes.RequestInvalid,
                        LogLevel.Information);
                }

                default:
                {
                    var descriptor = ErrorCatalog.Get(ErrorCodes.ServerError);
                    return (
                        CreateProblem(
                            ErrorCodes.ServerError,
                            descriptor.Title,
                            "An unexpected error occurred."),
                        descriptor.StatusCode,
                        ErrorCodes.ServerError,
                        LogLevel.Error);
                }
            }
        }

        private static ProblemDetails CreateProblem(string code, string title, string detail)
        {
            return new ProblemDetails
            {
                Title = title,
                Type = ErrorCatalog.TypeFor(code),
                Detail = detail
            };
        }

        private void Log(
            Exception exception,
            int statusCode,
            string? code,
            LogLevel logLevel,
            HttpContext httpContext)
        {
            if (statusCode >= 500)
            {
                _logger.LogError(
                    exception,
                    "Unhandled exception {Code} for {Method} {Path}",
                    code,
                    httpContext.Request.Method,
                    httpContext.Request.Path);
                return;
            }

            _logger.Log(
                logLevel,
                "Handled exception {Code} for {Method} {Path}: {Message}",
                code,
                httpContext.Request.Method,
                httpContext.Request.Path,
                exception.Message);
        }
    }
}
