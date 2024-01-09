namespace Profiles.API.Infrastructure.Filters
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System.Net;

    public class HttpGlobalActionFilter : IActionFilter
    {
        private readonly IWebHostEnvironment env;
        private readonly ILogger<HttpGlobalActionFilter> logger;

        public HttpGlobalActionFilter(IWebHostEnvironment env, ILogger<HttpGlobalActionFilter> logger)
        {
            this.env = env;
            this.logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {

        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            //logger.LogInformation(new EventId(context.Exception.HResult),
             //   context.Exception,
              //  context.Exception.Message);

            if (context.Result is BadRequestObjectResult)
            {
                var badRequestObjectResult = context.Result as BadRequestObjectResult;
                string error = badRequestObjectResult.Value.ToString();
                var problemDetails = new ValidationProblemDetails()
                {
                    Instance = context.HttpContext.Request.Path,
                    Status = StatusCodes.Status400BadRequest,
                    Detail = error
                };

                context.Result = new BadRequestObjectResult(problemDetails);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            else if (context.Result is BadRequestResult)
            {
                var problemDetails = new ValidationProblemDetails()
                {
                    Instance = context.HttpContext.Request.Path,
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "Some error has occured. Please try again."
                };

                context.Result = new BadRequestObjectResult(problemDetails);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            else if (context.Result is NotFoundObjectResult)
            {
                var notfoundObjectResult = context.Result as NotFoundObjectResult;
                string error = notfoundObjectResult.Value.ToString();
                var problemDetails = new ValidationProblemDetails()
                {
                    Instance = context.HttpContext.Request.Path,
                    Status = StatusCodes.Status404NotFound,
                    Detail = error
                };

                context.Result = new NotFoundObjectResult(problemDetails);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
            else if (context.Result is NotFoundResult)
            {
                var problemDetails = new ValidationProblemDetails()
                {
                    Instance = context.HttpContext.Request.Path,
                    Status = StatusCodes.Status404NotFound,
                    Detail = "Not found"
                };

                context.Result = new NotFoundObjectResult(problemDetails);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
        }
    }
}
