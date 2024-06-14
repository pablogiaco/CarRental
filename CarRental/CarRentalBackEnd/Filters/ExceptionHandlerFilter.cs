using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;


namespace CarRentalBackEnd.Filters
{
    public class ExceptionHandlerFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {            
            context.ExceptionHandled = true;
            context.Result = new ObjectResult("An error occurred")
            {
                StatusCode = 500
            };
        }
    }
}
