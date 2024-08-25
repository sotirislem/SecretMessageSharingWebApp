using FastEndpoints;
using FluentValidation;
using SecretMessageSharingWebApp.Models.Api.Requests;

namespace SecretMessageSharingWebApp.Validators;

public sealed class ValidateSecretMessageOtpRequestValidator : Validator<ValidateSecretMessageOtpRequest>
{
	public ValidateSecretMessageOtpRequestValidator()
	{
		RuleFor(request => request.OtpCode)
			.NotEmpty()
			.Length(Constants.OtpSize)
			.Matches(@"^\d+$").WithMessage("The value must contain only numeric digits."); ;
	}
}