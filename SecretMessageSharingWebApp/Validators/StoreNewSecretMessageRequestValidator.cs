using FastEndpoints;
using FluentValidation;
using SecretMessageSharingWebApp.Models.Api.Requests;

namespace SecretMessageSharingWebApp.Validators;

public sealed class StoreNewSecretMessageRequestValidator : Validator<StoreNewSecretMessageRequest>
{
	public StoreNewSecretMessageRequestValidator()
	{
		RuleLevelCascadeMode = CascadeMode.Stop;

		RuleFor(x => x.SecretMessageData).NotNull();
		When(x => x.SecretMessageData is not null, () =>
		{
			RuleFor(x => x.SecretMessageData.IV).NotEmpty();
			RuleFor(x => x.SecretMessageData.Salt).NotEmpty();
			RuleFor(x => x.SecretMessageData.CT).NotEmpty();
		});

		RuleFor(x => x.Otp).NotNull();
		When(x => x.Otp is not null, () =>
		{
			RuleFor(x => x.Otp.RecipientsEmail)
				.NotEmpty()
				.EmailAddress()
				.When(x => x.Otp.Required);
		});

		RuleFor(x => x.EncryptionKeySha256).NotEmpty();
	}
}