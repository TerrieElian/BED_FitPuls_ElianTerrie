namespace FitPulse.Endpoints;

public static class SupportTicketEndpoints
{
    public static RouteGroupBuilder MapSupportTicketEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (
            CreateTicketDto dto,
            ClaimsPrincipal user,
            IValidator<CreateTicketDto> validator,
            IMemberService memberService,
            ISupportTicketService ticketService,
            IMapper mapper) =>
        {
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var auth0Subject = user.FindFirst("sub")!.Value;
            var member = await memberService.GetOrCreateCurrentMemberAsync(auth0Subject);
            var ticket = await ticketService.CreateTicketAsync(member.Id, dto.Subject, dto.Description, dto.Priority);
            return Results.Created($"/supporttickets/{ticket.Id}", mapper.Map<SupportTicketDto>(ticket));
        });

        group.MapGet("/mine", async (ClaimsPrincipal user, IMemberService memberService, ISupportTicketService ticketService, IMapper mapper) =>
        {
            var auth0Subject = user.FindFirst("sub")!.Value;
            var member = await memberService.GetOrCreateCurrentMemberAsync(auth0Subject);
            var tickets = await ticketService.GetTicketsForMemberAsync(member.Id);
            return Results.Ok(mapper.Map<List<SupportTicketDto>>(tickets));
        });

        group.MapGet("/", async (ISupportTicketService ticketService, IMapper mapper) =>
        {
            var tickets = await ticketService.GetAllTicketsAsync();
            return Results.Ok(mapper.Map<List<SupportTicketDto>>(tickets));
        }).RequireAuthorization("ManageTickets");

        group.MapPut("/{id}/status", async (
            int id,
            UpdateTicketStatusDto dto,
            IValidator<UpdateTicketStatusDto> validator,
            ISupportTicketService ticketService,
            IMapper mapper) =>
        {
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            try
            {
                var ticket = await ticketService.UpdateStatusAsync(id, dto.Status);
                return Results.Ok(mapper.Map<SupportTicketDto>(ticket));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        }).RequireAuthorization("ManageTickets");

        return group;
    }
}