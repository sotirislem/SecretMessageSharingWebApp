using FastEndpoints;
using FluentValidation;
using SecretMessageSharingWebApp.Models.Api.Requests;

namespace SecretMessageSharingWebApp.Validators;

public sealed class StoreNewSecretMessageRequestValidator : Validator<StoreNewSecretMessageRequest>
{
	public StoreNewSecretMessageRequestValidator()
	{
		RuleFor(request => request.ClientId)
			.Must(clientId => Guid.TryParse(clientId, out _))
			.WithMessage("'Client-Id' must be a valid Guid.");

		RuleFor(request => request.SecretMessageData).NotNull();
		When(request => request.SecretMessageData is not null, () =>
		{
			RuleFor(request => request.SecretMessageData.IV).NotEmpty();
			RuleFor(request => request.SecretMessageData.Salt).NotEmpty();
			RuleFor(request => request.SecretMessageData.CT).NotEmpty();
		});

		RuleFor(request => request.Otp).NotNull();
		When(request => request.Otp is not null, () =>
		{
			RuleFor(request => request.Otp.RecipientsEmail)
				.NotEmpty()
				.EmailAddress()
				.When(request => request.Otp.Required);
		});

		RuleFor(request => request.EncryptionKeySha256).NotEmpty();
	}
}