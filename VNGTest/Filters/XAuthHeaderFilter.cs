using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using VNGTest.Models;
using System.Text.Json;

namespace VNGTest.Filters
{
    public class XAuthHeaderFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Check if the "xAuth" header is present and not empty
            if (!context.HttpContext.Request.Headers.TryGetValue("xAuth", out var xAuthHeader) || string.IsNullOrEmpty(xAuthHeader))
            {
                // Create a response with ApiResponse
                var response = new ApiResponse<object>(
                    success: false,
                    message: "Unauthorized: Missing or empty xAuth header.",
                    data: null,
                    statusCode: StatusCodes.Status401Unauthorized
                );

                // Set the response content type to application/json
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

                // Write the response as JSON
                context.Result = new ContentResult
                {
                    Content = JsonSerializer.Serialize(response),
                    ContentType = "application/json",
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No implementation needed for this scenario
        }
    }
}
