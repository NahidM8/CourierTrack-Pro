namespace CourierTrack.API.Controllers;

[ApiController]
[Route("api/v1/payments")]
[Authorize]
public class PaymentController(
    IPaymentService paymentService,
    IValidator<CreatePaymentIntentDto> createPaymentIntentValidator,
    IValidator<ConfirmPaymentDto> confirmPaymentValidator,
    IValidator<RefundPaymentDto> refundPaymentValidator
    ) : ControllerBase
{
    [HttpPost("create-intent")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentDto dto)
    {
        var validationResult = await createPaymentIntentValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var payment = await paymentService.CreatePaymentIntentAsync(dto);
        return Ok(ApiResponse<PaymentDto>.SuccessResult(payment));
    }

    [HttpPost("confirm")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentDto dto)
    {
        var validationResult = await confirmPaymentValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var payment = await paymentService.ConfirmPaymentAsync(dto);
        return Ok(ApiResponse<PaymentDto>.SuccessResult(payment));
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook()
    {
        var payload = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var stripeSignature = Request.Headers["Stripe-Signature"].ToString();

        await paymentService.HandleWebhookAsync(payload, stripeSignature);
        return Ok();
    }

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetByOrderId(Guid orderId)
    {
        var payment = await paymentService.GetByOrderIdAsync(orderId);
        return Ok(ApiResponse<PaymentDto>.SuccessResult(payment));
    }

    [HttpPost("{id:guid}/refund")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Refund(Guid id, [FromBody] RefundPaymentDto dto)
    {
        var validationResult = await refundPaymentValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var payment = await paymentService.RefundAsync(id, dto);
        return Ok(ApiResponse<PaymentDto>.SuccessResult(payment));
    }
}