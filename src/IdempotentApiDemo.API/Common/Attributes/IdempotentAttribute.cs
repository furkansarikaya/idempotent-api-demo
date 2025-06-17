using System.Text.Json;
using IdempotentApiDemo.API.Models.Enums;
using IdempotentApiDemo.API.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace IdempotentApiDemo.API.Common.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class IdempotentAttribute(IdempotentBehavior behavior = IdempotentBehavior.ReturnFromCache, int ttlMinutes = 60)
    : Attribute, IAsyncActionFilter
{
    private readonly TimeSpan _ttl = TimeSpan.FromMinutes(ttlMinutes);

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var request = context.HttpContext.Request;

        if (!request.Headers.TryGetValue("Idempotency-Key", out var keyVal) || !Guid.TryParse(keyVal, out var key))
        {
            context.Result = new BadRequestObjectResult("Missing or invalid Idempotency-Key");
            return;
        }

        var cache = context.HttpContext.RequestServices.GetRequiredService<IDistributedCache>();
        var redis = context.HttpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>().GetDatabase();

        var cacheKey = $"idem:{key}";
        var lockKey = $"lock:{cacheKey}";

        var acquired = await redis.StringSetAsync(lockKey, "1", TimeSpan.FromSeconds(5), When.NotExists);
        if (!acquired)
        {
            context.Result = new ConflictObjectResult("Another request is processing this key.");
            return;
        }

        try
        {
            var existing = await cache.GetStringAsync(cacheKey);
            if (existing != null)
            {
                if (behavior == IdempotentBehavior.ThrowErrorIfExists)
                {
                    context.Result = new ConflictObjectResult("Request already processed.");
                    return;
                }

                var saved = JsonSerializer.Deserialize<IdempotentResponse>(existing)!;
                context.Result = new ObjectResult(saved.Value) { StatusCode = saved.StatusCode };
                return;
            }

            var result = await next();

            if (result.Result is ObjectResult { StatusCode: >= 200 and < 300 } obj)
            {
                var toCache = new IdempotentResponse
                {
                    StatusCode = obj.StatusCode.Value,
                    Value = obj.Value
                };

                var json = JsonSerializer.Serialize(toCache);
                await cache.SetStringAsync(cacheKey, json, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _ttl
                });
            }
        }
        finally
        {
            await redis.KeyDeleteAsync(lockKey);
        }
    }
}