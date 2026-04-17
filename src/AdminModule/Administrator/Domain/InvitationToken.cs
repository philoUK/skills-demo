using System.Security.Cryptography;

namespace AdminModule.Administrator.Domain;

internal record InvitationToken(string Value)
{
    internal static InvitationToken Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return new InvitationToken(
            Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=')
        );
    }
}
