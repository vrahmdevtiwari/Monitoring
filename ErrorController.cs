using Microsoft.AspNetCore.Mvc;

namespace Monitoring.Controllers
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode?}")]
        public IActionResult Index(int? statusCode)
        {
            if (statusCode.HasValue)
            {
                Response.Clear();
                Response.StatusCode = statusCode.Value;
                switch (statusCode.Value)
                {
                    case 400:
                        return View("~/Views/Shared/ErrorPages/BadRequestError.cshtml");
                    case 401:
                        return View("~/Views/Shared/ErrorPages/UnauthorizedError.cshtml");
                    case 403:
                        return View("~/Views/Shared/ErrorPages/ForbiddenError.cshtml");
                    case 404:
                        return View("~/Views/Shared/ErrorPages/PageNotFoundError.cshtml");
                    case 405:
                        return View("~/Views/Shared/ErrorPages/MethodNotAllowedError.cshtml");
                    case 500:
                        return View("~/Views/Shared/ErrorPages/InternalServerError.cshtml");
                    case 503:
                        return View("~/Views/Shared/ErrorPages/ServiceUnavailableError.cshtml");
                    case 504:
                        return View("~/Views/Shared/ErrorPages/GatewayTimeoutError.cshtml");
                    default:
                        return View("~/Views/Shared/ErrorPages/GenericError.cshtml");
                }
            }
            else
            {
                // No status code specified, show the generic error view
                return View("~/Views/Shared/ErrorPages/GenericError.cshtml");
            }
        }
    }
}
