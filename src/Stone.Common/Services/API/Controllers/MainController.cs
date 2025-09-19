using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Stone.Common.Core.DTOs.Support;
using Stone.Common.Core.Notifications;

using System.Net;

namespace Stone.Common.Services.API.Controllers
{
    [ApiController]
    public abstract class MainController : ControllerBase
    {
        protected readonly NotificationContext _notificationContext;

        public MainController(NotificationContext notificationContext)
        {
            _notificationContext = notificationContext;
        }
        protected ActionResult CustomResponse(object result = null)
        {
            if (OperacaoValida())
            {
                if (result == null) return Ok();

                return Ok(result);
            }

            var modelState = new ModelStateDictionary();
            foreach (var notification in _notificationContext.Notifications)
            {
                modelState.AddModelError("Mensagens", notification.Message);
            }

            return BadRequest(new ValidationProblemDetails(modelState)
            {
                Title = "Ocorreram erros na validação",
                Status = StatusCodes.Status400BadRequest
            });
        }

        protected ActionResult CustomErrorResponse(string erro, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            AdicionarErroProcessamento(erro);

            var modelState = new ModelStateDictionary();
            foreach (var notification in _notificationContext.Notifications)
            {
                modelState.AddModelError("Mensagens", notification.Message);
            }

            return BadRequest(new ValidationProblemDetails(modelState)
            {
                Title = "Ocorreram erros na validação",
                Status = (int)statusCode
            });
        }

        protected ActionResult CustomResponse(ValidationResult validationResult)
        {
            foreach (var erro in validationResult.Errors)
            {
                AdicionarErroProcessamento(erro.ErrorMessage);
            }

            return CustomResponse();
        }

        protected ActionResult CustomResponse(ResponseResult resposta)
        {
            ResponsePossuiErros(resposta);

            return CustomResponse();
        }

        protected bool ResponsePossuiErros(ResponseResult resposta)
        {
            if (resposta == null || !resposta.Errors.Mensagens.Any()) return false;

            foreach (var mensagem in resposta.Errors.Mensagens)
            {
                AdicionarErroProcessamento(mensagem);
            }

            return true;
        }

        protected bool OperacaoValida()
        {
            return !_notificationContext.ExistNotifications;
        }

        protected void AdicionarErroProcessamento(string erro)
        {
            _notificationContext.AddNotification(erro);
        }
    }
}
