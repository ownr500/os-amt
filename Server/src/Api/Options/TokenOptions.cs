using API.Models.enums;

namespace API.Options;

public sealed class TokenOptions
{
    public Dictionary<TokenType, TokenInfo> TokenInfos { get; set; }
}