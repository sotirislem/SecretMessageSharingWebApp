using FastEndpoints;
using FluentValidation;
using SecretMessageSharingWebApp.Models.Api.Requests;

namespace SecretMessageSharingWebApp.Validators;

public sealed class GetSecretMessageRequestValidator : Validator<GetSecretMessageRequest>
{
	private const string Sha256HexPattern = "^[a-fA-F0-9]{64}$";

	public GetSecretMessageRequestValidator()
	{
		RuleFor(request => request.EncryptionKeySha256)
			.NotEmpty()
			.WithMessage("The 'X-Key-Hash' header is required.");

		RuleFor(request => request.EncryptionKeySha256)
			.Matches(Sha256HexPattern)
			.When(request => string.IsNullOrWhiteSpace(request.EncryptionKeySha256) is false)
			.WithMessage("The 'X-Key-Hash' header must be a valid SHA-256 hexadecimal string.");
	}
}
