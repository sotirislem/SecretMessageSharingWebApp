namespace SecretMessageSharingWebApp.Providers;

public interface ICancellationTokenProvider
{
	CancellationToken Token { get; }
}

public sealed class CancellationTokenProvider(IHttpContextAccessor httpContextAccessor) : ICancellationTokenProvider
{
	public CancellationToken Token => httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;
}