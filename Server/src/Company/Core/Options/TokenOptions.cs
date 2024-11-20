using Company.Core.Enums;

namespace Company.Core.Options;

public sealed class TokenOptions
{
    public Dictionary<TokenType, TokenInfo> TokenInfos { get; init; } = new();
}