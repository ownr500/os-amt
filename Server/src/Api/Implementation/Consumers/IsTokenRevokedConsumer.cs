using API.Core.Services;
using AuthContractMessages.Tokens;
using MassTransit;

namespace API.Implementation.Consumers;

public class IsTokenRevokedConsumer : IConsumer<IsTokenRevokedRequest>
{
    private readonly ITokenService _tokenService;

    public IsTokenRevokedConsumer(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public async Task Consume(ConsumeContext<IsTokenRevokedRequest> context)
    {
        var isRevoked = await _tokenService.IsTokenRevokedAsync(context.Message.Token);
        await context.RespondAsync(new IsTokenRevokedResponse(isRevoked));
    }
}