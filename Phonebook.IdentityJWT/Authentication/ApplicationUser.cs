using Microsoft.AspNetCore.Identity;

namespace Phonebook.IdentityJWT.Authentication;

public class ApplicationUser : IdentityUser
{
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
}