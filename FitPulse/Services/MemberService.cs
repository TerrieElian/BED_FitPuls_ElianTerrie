namespace FitPulse.Services;

public interface IMemberService
{
    Task<Member> GetOrCreateCurrentMemberAsync(string auth0Subject);
    Task<Member> UpdateEmailAsync(string auth0Subject, string email);
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

    public async Task<Member> UpdateEmailAsync(string auth0Subject, string email)
    {
        var member = await GetOrCreateCurrentMemberAsync(auth0Subject);
        member.Email = email;
        await _memberRepository.UpdateAsync(member);
        return member;
    }
}