namespace MessengerAPI.Application.Common.Interfaces.Auth;

public interface IRevokedTokenStore
{
    /// <summary>
    /// checks if token is valid
    /// </summary>
    /// <param name="tokenId">Id of token</param>
    /// <returns>Is token valid</returns>
    Task<bool> IsTokenValidAsync(string tokenId);
    /// <summary>
    /// revoke token
    /// </summary>
    /// <param name="tokenId">Id of token</param>
    /// <param name="expires">Time to expire in seconds</param>
    Task RevokeTokenAsync(string tokenId, int expires);
}
