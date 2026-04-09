using Microsoft.AspNetCore.Authorization;

namespace MultiTenantInventory.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "User")]
public class OrdersController(IOrderService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetAll()
    {
        try
        {
            var orders = await service.GetAllAsync();
            return Ok(ApiResponse<List<OrderDto>>.Ok(orders));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<OrderDto>>.Fail(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetById(Guid id)
    {
        try
        {
            var order = await service.GetByIdAsync(id);
            if (order == null)
                return NotFound(ApiResponse<OrderDto>.Fail("Order not found."));
            return Ok(ApiResponse<OrderDto>.Ok(order));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<OrderDto>.Fail(ex.Message));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderDto>>> Create([FromBody] CreateOrderDto dto)
    {
        try
        {
            var order = await service.CreateAsync(dto);
            return Ok(ApiResponse<OrderDto>.Ok(order));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<OrderDto>.Fail(ex.Message));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<OrderDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}/status")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusDto dto)
    {
        try
        {
            var result = await service.UpdateStatusAsync(id, dto);
            if (!result)
                return NotFound(ApiResponse<bool>.Fail("Order not found."));
            return Ok(ApiResponse<bool>.Ok(true, $"Order status updated to {dto.Status}."));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<bool>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
        }
    }
}
