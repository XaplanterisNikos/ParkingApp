using System.Net.Http.Headers;

namespace ParkingApp.Client.Services.Auth;

/// <summary>
/// A message handler that attaches the stored JWT as a Bearer token to every
/// outgoing request. Registered on the HttpClient, so consumers never deal with tokens.
/// </summary>
public class AuthTokenHandler : DelegatingHandler
{
	private readonly ITokenStore _tokenStore;

	public AuthTokenHandler(ITokenStore tokenStore)
	{
		_tokenStore = tokenStore;
	}

	protected override async Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request, CancellationToken cancellationToken)
	{
		// Attach the token to this request before it goes out.
		var token = await _tokenStore.GetTokenAsync();
		if (!string.IsNullOrWhiteSpace(token))
		{
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
		}

		// Pass the request along the chain (eventually to the network).
		return await base.SendAsync(request, cancellationToken);
	}
}
