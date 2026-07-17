namespace FitPulse.Services;

public interface IMemberService
{
    Task<Member> GetOrCreateCurrentMemberAsync(string auth0Subject);
}

public class MemberService : IMemberService
{
    private readonly IMemberRepository _memberRepository;

    public MemberService(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task<Member> GetOrCreateCurrentMemberAsync(string auth0Subject)
    {
        var existingMember = await _memberRepository.GetByAuth0SubjectAsync(auth0Subject);
        if (existingMember is not null)
        {
            return existingMember;
        }

        var newMember = new Member
        {
            Auth0Subject = auth0Subject
        };

        await _memberRepository.AddAsync(newMember);
        return newMember;
    }
}