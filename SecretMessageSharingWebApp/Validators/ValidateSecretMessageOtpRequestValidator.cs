using FastEndpoints;
using FluentValidation;
using SecretMessageSharingWebApp.Models.Api.Requests;

namespace SecretMessageSharingWebApp.Validators;

public sealed class ValidateSecretMessageOtpRequestValidator : Validator<ValidateSecretMessageOtpRequest>
{
	public ValidateSecretMessageOtpRequestValidator()
	{
		RuleLevelCascadeMode = CascadeMode.Stop;

		RuleFor(x => x.OtpCode)
			.NotEmpty()
			.Length(Constants.OtpSize);
	}
}