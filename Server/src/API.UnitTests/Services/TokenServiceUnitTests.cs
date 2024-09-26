using System.IdentityModel.Tokens.Jwt;
using API.Core.Options;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace API.UnitTests.Controllers;

public class TokenServiceUnitTests
{
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly IOptions<TokenOptions> _options;
    public TokenServiceUnitTests()
    {
        _tokenHandler = Substitute.For<JwtSecurityTokenHandler>();
        _options = Substitute.For<IOptions<TokenOptions>>();
    }
}