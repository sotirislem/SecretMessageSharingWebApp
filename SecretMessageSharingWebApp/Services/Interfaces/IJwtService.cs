namespace SecretMessageSharingWebApp.Services.Interfaces;

public interface IJwtService
{
	string GenerateToken(string messageId);

	bool ValidateToken(string? token, string messageId);
}