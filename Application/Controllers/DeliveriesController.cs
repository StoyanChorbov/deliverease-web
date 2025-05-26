using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.DTO.Delivery;
using Service;

namespace Application.Controllers;

[ApiController]
[Route("/deliveries")]
public class DeliveriesController(DeliveryService deliveryService) : ControllerBase
{
    // Add a new delivery to the database
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddDelivery([FromBody] DeliveryAddDto deliveryAddDto)
    {
        if (!User.Identity?.IsAuthenticated ?? false)
        {
            return Unauthorized();
        }

        var deliveryId = await deliveryService.AddDeliveryAsync(
            deliveryAddDto,
            User.Identity?.Name ?? throw new Exception("User not found")
        );
        
        return Ok(deliveryId);
    }

    // Get delivery by id
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<DeliveryDto>> GetDelivery(Guid id)
    {
        return Ok(await deliveryService.GetDeliveryAsync(id));
    }

    // Get the deliveries that have been created by the logged-in user
    [HttpGet("past")]
    [Authorize]
    public async Task<ActionResult<List<UserDeliveryDto>>> GetPastDeliveries()
    {
        var username = User.Identity?.Name;
        if (username == null)
            return Unauthorized();

        var deliveries = await deliveryService.GetPastDeliveriesAsync(username);
        return Ok(deliveries);
    }

    // Get deliveries by starting and ending location
    [HttpGet("options/{startingLocationRegion}/{endingLocationRegion}")]
    [Authorize]
    public async Task<ActionResult<List<DeliveryListDto>>> GetDeliveriesByLocations(
        string startingLocationRegion, string endingLocationRegion)
    {
        var deliveries =
            await deliveryService.GetAllByStartingAndEndingLocation(startingLocationRegion, endingLocationRegion);
        return Ok(deliveries);
    }
    
    // Get packages to be delivered by the user and to be received by the user
    [HttpGet("current")]
    [Authorize]
    public async Task<IActionResult> GetCurrentDeliveriesAsync()
    {
        var username = User.Identity?.Name;
        if (username == null)
            return Unauthorized();
        
        var deliveries = await deliveryService.GetCurrentDeliveriesAsync(username);
        
        return Ok(new
        {
            toDeliver = deliveries.Item1,
            toReceive = deliveries.Item2
        });
    }
    
    [HttpPost("deliver")]
    [Authorize]
    public async Task<IActionResult> SetDeliveryDeliverer([FromBody] DeliveryRequestDto request)
    {
        var username = User.Identity?.Name;
        if (username == null)
            return Unauthorized();

        await deliveryService.SetDeliveryDelivererAsync(request.DeliveryId, username);
        return Ok();
    }

    // Update the delivery using id and dto
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdateDelivery([FromRoute] Guid id, [FromBody] DeliveryDto deliveryDto)
    {
        await deliveryService.UpdateDeliveryAsync(id, deliveryDto);
        return Ok();
    }
    
    [HttpPut("completed")]
    [Authorize]
    public async Task<IActionResult> MarkDeliveryAsCompleted([FromBody] DeliveryRequestDto request)
    {
        var username = User.Identity?.Name;
        if (username == null)
            return Unauthorized();

        await deliveryService.MarkDeliveryAsCompletedAsync(request.DeliveryId);
        return Ok();
    }

    // Delete the delivery
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteDelivery(Guid id)
    {
        await deliveryService.DeleteDeliveryAsync(id);
        return NoContent();
    }
}