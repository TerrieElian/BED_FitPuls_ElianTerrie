namespace FitPulse.Endpoints;

public static class TrainingSessionEndpoints
{
    public static RouteGroupBuilder MapTrainingSessionEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (
            RequestSessionDto dto,
            ClaimsPrincipal user,
            IValidator<RequestSessionDto> validator,
            IMemberService memberService,
            ITrainingSessionService sessionService,
            IMapper mapper) =>
        {
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            try
            {
                var auth0Subject = user.FindFirst("sub")!.Value;
                var member = await memberService.GetOrCreateCurrentMemberAsync(auth0Subject);
                var session = await sessionService.RequestSessionAsync(member.Id, dto.DeviceType);
                return Results.Created($"/trainingsessions/{session.Id}", mapper.Map<TrainingSessionDto>(session));
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        });

        group.MapGet("/", async (ClaimsPrincipal user, IMemberService memberService, ITrainingSessionService sessionService, IMapper mapper) =>
        {
            var auth0Subject = user.FindFirst("sub")!.Value;
            var member = await memberService.GetOrCreateCurrentMemberAsync(auth0Subject);
            var sessions = await sessionService.GetSessionsForMemberAsync(member.Id);
            return Results.Ok(mapper.Map<List<TrainingSessionDto>>(sessions));
        });

        group.MapGet("/{id}", async (int id, ITrainingSessionService sessionService, IMapper mapper) =>
        {
            var session = await sessionService.GetSessionByIdAsync(id);
            return session is not null ? Results.Ok(mapper.Map<TrainingSessionDto>(session)) : Results.NotFound();
        });

        group.MapPut("/{id}/start", async (int id, ITrainingSessionService sessionService, IMapper mapper) =>
        {
            try
            {
                var session = await sessionService.StartSessionAsync(id);
                return Results.Ok(mapper.Map<TrainingSessionDto>(session));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        });

        group.MapPut("/{id}/complete", async (
            int id,
            CompleteSessionDto dto,
            IValidator<CompleteSessionDto> validator,
            ITrainingSessionService sessionService,
            IPaymentService paymentService,
            IInvoiceService invoiceService,
            IEmailService emailService,
            IMapper mapper) =>
        {
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            try
            {
                var session = await sessionService.CompleteSessionAsync(id, dto.DurationMinutes, dto.CaloriesBurned);
                var payment = await paymentService.GenerateForSessionAsync(id, dto.DiscountCode);

                var pdfBytes = invoiceService.GenerateInvoicePdf(payment, session, session.Member);
                if (!string.IsNullOrWhiteSpace(session.Member.Email))
                {
                    await emailService.SendInvoiceAsync(session.Member.Email, pdfBytes, $"factuur-{session.Id}.pdf");
                }

                return Results.Ok(new
                {
                    session = mapper.Map<TrainingSessionDto>(session),
                    payment = mapper.Map<PaymentDto>(payment)
                });
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        });

        group.MapPut("/{id}/cancel", async (int id, ITrainingSessionService sessionService, IMapper mapper) =>
        {
            try
            {
                var session = await sessionService.CancelSessionAsync(id);
                return Results.Ok(mapper.Map<TrainingSessionDto>(session));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        });

        group.MapGet("/{id}/invoice", async (
    int id,
    ITrainingSessionService sessionService,
    IPaymentRepository paymentRepository,
    IInvoiceService invoiceService) =>
{
    var session = await sessionService.GetSessionByIdAsync(id);
    if (session is null)
    {
        return Results.NotFound();
    }

    var payment = await paymentRepository.GetBySessionIdAsync(id);
    if (payment is null)
    {
        return Results.NotFound(new { error = "Nog geen betaling voor deze sessie." });
    }

    var pdfBytes = invoiceService.GenerateInvoicePdf(payment, session, session.Member);
    return Results.File(pdfBytes, "application/pdf", $"factuur-{id}.pdf");
});

        return group;
    }
}