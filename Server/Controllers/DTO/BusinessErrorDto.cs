namespace API.Controllers.DTO;

internal sealed record BusinessErrorDto(
    List<string> Messages);