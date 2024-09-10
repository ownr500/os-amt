using API.Models.Entities;

namespace API.Models;

public class CombinedTokenModel
{
    public RevokedTokenEntity Entity1 { get; set; }
    public RevokedTokenEntity Entity2 { get; set; }
}