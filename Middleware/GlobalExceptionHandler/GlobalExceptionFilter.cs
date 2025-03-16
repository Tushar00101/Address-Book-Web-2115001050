using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NLog;

namespace Middleware.GlobalExceptionHandler
{
    public class GlobalExceptionHandler : ExceptionFilterAttribute
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public override void OnException(ExceptionContext context)
        {
            _logger.Error(context.Exception, "Unhandled exception occurred!");

            var errorResponse = ExceptionHandler.CreateErrorResponse(context.Exception);

            int statusCode = context.Exception is ArgumentException ? 400 : 500;

            context.Result = new ObjectResult(errorResponse)
            {
                StatusCode = statusCode
            };

            context.ExceptionHandled = true;
        }
    }
}
