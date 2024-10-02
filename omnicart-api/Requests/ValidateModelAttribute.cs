
// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Fonseka M.M.N.H
// Student ID       : IT21156410
// Description      : Format the model binding validation response
// Tutorial         : https://code-maze.com/aspnetcore-modelstate-validation-web-api/
// ***********************************************************************
 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters; 
using omnicart_api.Models; 
namespace omnicart_api.Requests
{
    public class ValidateModelAttribute : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {

        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Check if the ModelState is invalid (model binding failed)
            if (!context.ModelState.IsValid)
            {
              
                // Return custom AppResponse with validation errors
                var errorResponse = new AppResponse<object>
                {
                    Success = false,
                    Message = "One or more validation errors occurred.",
                    Error = "Unprocessable Entity",
                    ErrorCode = 422,
                    ErrorData = new UnprocessableEntityObjectResult(context.ModelState)
                };

                context.Result = new UnprocessableEntityObjectResult(errorResponse);
            }
        }
    }
}