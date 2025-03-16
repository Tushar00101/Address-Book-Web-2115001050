using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog;

namespace Middleware.GlobalExceptionHandler
{
    public class ExceptionHandler
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public static string HandleException(Exception ex, out object errorResponse)
        {
            errorResponse = CreateErrorResponse(ex);
            return JsonConvert.SerializeObject(errorResponse);
        }

        public static object CreateErrorResponse(Exception ex)
        {
            _logger.Error(ex, $"Exception Type: {ex.GetType().Name} | Message: {ex.Message}");

            return new
            {
                Success = false,
                Message = "An unexpected error occurred. Please try again later.",
                Error = ex.Message
            };
        }
    }
}
