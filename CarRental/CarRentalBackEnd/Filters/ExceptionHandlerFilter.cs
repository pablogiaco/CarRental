using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;


namespace CarRentalBackEnd.Filters
{
    public class ExceptionHandlerFilter : IExceptionFilter
    {
        private readonly ILog _log;
        public ExceptionHandlerFilter(ILog log)
        {
            _log = log;
        }
        public void OnException(ExceptionContext context)
        {
            _log.Error($"An error ocurred: {context.Exception}");
            context.ExceptionHandled = true;
            context.Result = new ObjectResult("An error occurred")
            {
                StatusCode = 500
            };
        }
    }
}
