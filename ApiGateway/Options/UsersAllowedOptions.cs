using ApiGateway.Models;

namespace ApiGateway.Options;

public class UsersAllowedOptions
{
    public IList<LoginInformation> UsersAllowed { get; set; } = [];
}