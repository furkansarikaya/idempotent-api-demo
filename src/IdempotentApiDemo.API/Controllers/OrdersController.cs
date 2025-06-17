using IdempotentApiDemo.API.Common.Attributes;
using IdempotentApiDemo.API.Models.Enums;
using IdempotentApiDemo.API.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace IdempotentApiDemo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpPost]
    [Idempotent(IdempotentBehavior.ReturnFromCache)]
    public IActionResult Create([FromBody] OrderRequest request)
    {
        var order = new
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            Created = DateTime.UtcNow
        };

        return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    }

    [HttpPost("strict")]
    [Idempotent(IdempotentBehavior.ThrowErrorIfExists)]
    public IActionResult CreateStrict([FromBody] OrderRequest request)
    {
        var order = new
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            Created = DateTime.UtcNow
        };

        return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    }

    [HttpGet("{id}")]
    public IActionResult Get(Guid id) => Ok(new { id, status = "mocked" });
}
