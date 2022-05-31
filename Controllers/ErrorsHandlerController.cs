using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Models.Api.Responses;

namespace SecretMessageSharingWebApp.Controllers
{
	[AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorsHandlerController : ControllerBase
    {
        [Route("error")]
        public ApiErrorResponse Error([FromServices] ILogger<ErrorsHandlerController> logger)
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = context!.Error;

            logger.LogError(exception.GetAllErrorMessages());

            return new ApiErrorResponse();
        }
    }
}
