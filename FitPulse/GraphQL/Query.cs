using HotChocolate.Authorization;

namespace FitPulse.GraphQL;

public class Query
{
    [Authorize(Policy = "ManageDevices")]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Device> GetDevices(FitPulseDbContext context) => context.Devices;

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<TrainingSession>> GetTrainingSessions(
        ClaimsPrincipal user,
        FitPulseDbContext context,
        IMemberService memberService)
    {
        var auth0Subject = user.FindFirst("sub")!.Value;
        var member = await memberService.GetOrCreateCurrentMemberAsync(auth0Subject);
        return context.TrainingSessions.Where(s => s.MemberId == member.Id);
    }
}