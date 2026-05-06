namespace CourierTrack.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouriersController(ICourierService courierService,ICourierRatingService courierRatingService) : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPaged([FromQuery] CourierFilterDto filter)
        {
            var couriers = await courierService.GetAllPagedAsync(filter);
            return Ok(ApiResponse<PagedResult<CourierDto>>.SuccessResult(couriers));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var courier = await courierService.GetByIdAsync(id);
            return Ok(ApiResponse<CourierDto>.SuccessResult(courier));
        }

        [HttpPut("{id:guid}/availability")]
        [Authorize(Roles = "Courier")]
        public async Task<IActionResult> UpdateAvailability(Guid id, [FromBody] UpdateAvailabilityDto dto)
        {
            await courierService.UpdateAvailabilityAsync(id, dto.IsAvailable);
            return Ok(ApiResponse<object>.SuccessResult(null));
        }

        [HttpPut("{id:guid}/location")]
        [Authorize(Roles = "Courier")]
        public async Task<IActionResult> UpdateLocation(Guid id, [FromBody] UpdateLocationDto dto)
        {
            await courierService.UpdateLocationAsync(id, dto.Latitude, dto.Longitude);
            return Ok(ApiResponse<object>.SuccessResult(null));
        }

        [HttpGet("{id:guid}/orders")]
        [Authorize(Roles = "Courier,Admin")]
        public async Task<IActionResult> GetCourierOrders(Guid id)
        {
            var orders = await courierService.GetCourierOrdersAsync(id);
            return Ok(ApiResponse<IEnumerable<OrderDto>>.SuccessResult(orders));
        }

        [HttpPut("orders/{orderId:guid}/accept")]
        [Authorize(Roles = "Courier")]
        public async Task<IActionResult> AcceptOrder(Guid orderId)
        {
            var courierId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await courierService.AcceptOrderAsync(courierId, orderId);
            return Ok(ApiResponse<object>.SuccessResult(200));
        }

        [HttpPut("orders/{orderId:guid}/reject")]
        [Authorize(Roles = "Courier")]
        public async Task<IActionResult> RejectOrder(Guid orderId)
        {
            var courierId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await courierService.RejectOrderAsync(courierId, orderId);
            return Ok(ApiResponse<object>.SuccessResult(200));
        }

        [HttpGet("{id:guid}/ratings")]
        [Authorize]
        public async Task<IActionResult> GetCourierRatings(Guid id)
        {
            var ratings = await courierRatingService.GetCourierRatingsAsync(id);
            return Ok(ApiResponse<IEnumerable<CourierRatingDto>>.SuccessResult(ratings));
        }
    }
}
