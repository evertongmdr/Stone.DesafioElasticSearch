using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Stone.Common.Core.DTOs.Support;
using System.Text.Json;

namespace Stone.Common.Services.API.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro inesperado no sistema, detalhes foram registrados.");

                await ProcessErrorResponse(httpContext, ex);

            }
        }

        private async Task ProcessErrorResponse(HttpContext httpContext, Exception exception)
        {
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "application/json";

            var response = new ResponseResult
            {
                Title = "Ocorreu um erro inesperado no sistema",
                Status = StatusCodes.Status500InternalServerError,
                Errors = new ResponseErrorMessages
                {
                    Mensagens = new List<string> { "Ocorreu um erro inesperado no sistema, tente novamente mais tarde." }
                }
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);

            await httpContext.Response.WriteAsync(jsonResponse);
        }

    }
}
