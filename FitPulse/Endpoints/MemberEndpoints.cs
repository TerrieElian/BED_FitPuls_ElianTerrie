
namespace FitPulse.Endpoints;

public static class MemberEndpoints
{
    public static RouteGroupBuilder MapMemberEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/me", async (ClaimsPrincipal user, IMemberService service, IMapper mapper) =>
        {
            var auth0Subject = user.FindFirst("sub")?.Value;
            if (auth0Subject is null)
            {
                return Results.Unauthorized();
            }

            var member = await service.GetOrCreateCurrentMemberAsync(auth0Subject);
            return Results.Ok(mapper.Map<MemberDto>(member));
        });

        return group;
    }
}