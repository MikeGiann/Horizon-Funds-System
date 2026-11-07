using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FundsApi.Filters
{
    public class IdempotencyFilter : IAsyncActionFilter
    {
        private static readonly HashSet<string> ProcessedKeys = new();

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue("X-Idempotency-Key", out var idempotencyKey))
            {
                context.Result = new BadRequestObjectResult("The X-Idempotency-Key header is required.");
                return;
            }

            var key = idempotencyKey.ToString();
            lock (ProcessedKeys)
            {
                if (ProcessedKeys.Contains(key))
                {
                    context.Result = new OkObjectResult("Transaction with this key has already been processed.");
                    return;
                }
            }

            var result = await next();

            // If execution was successful, add the key to the set
            if (result.Exception == null)
            {
                lock (ProcessedKeys)
                {
                    ProcessedKeys.Add(key);
                }
            }
        }
    }
}