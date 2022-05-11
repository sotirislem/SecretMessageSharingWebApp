using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SecretMessageSharingWebApp.Helpers;
using SecretMessageSharingWebApp.Models.Api;

namespace SecretMessageSharingWebApp.Controllers
{
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorsController : ControllerBase
    {
        [Route("error")]
        public ErrorResponse Error([FromServices] ILogger<ErrorsController> logger)
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = context!.Error;

            logger.LogError(exception.GetAllErrorMessages());

            return new ErrorResponse();
        }
    }
}
