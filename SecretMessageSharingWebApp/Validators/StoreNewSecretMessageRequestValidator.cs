using FastEndpoints;
using FluentValidation;
using SecretMessageSharingWebApp.Models.Api.Requests;

namespace SecretMessageSharingWebApp.Validators;

public sealed class StoreNewSecretMessageRequestValidator : Validator<StoreNewSecretMessageRequest>
{
	private const int AesGcmIvLength = 12;
	private const int AesGcmTagLength = 16;
	private const int MaxCipherTextBytes = Constants.MaxSecretMessageCipherTextBytes;
	private const int MaxBase64Length = ((MaxCipherTextBytes + 2) / 3) * 4;

	public StoreNewSecretMessageRequestValidator()
	{
		RuleFor(request => request.ClientId)
			.Must(clientId => Guid.TryParse(clientId, out _))
			.WithMessage("'Client-Id' must be a valid Guid.");

		RuleFor(request => request.SecretMessageData).NotNull();
		When(request => request.SecretMessageData is not null, () =>
		{
			RuleFor(request => request.SecretMessageData!.IV)
				.NotEmpty()
				.Must(BeValidInitializationVector)
				.WithMessage($"'IV' must be a Base64-encoded {AesGcmIvLength}-byte initialization vector.");

			RuleFor(request => request.SecretMessageData!.CT)
				.NotEmpty()
				.Must(BeValidCipherText)
				.WithMessage($"'CT' must be a Base64-encoded AES-GCM ciphertext between {AesGcmTagLength} bytes and {MaxCipherTextBytes} bytes.");
		});

		RuleFor(request => request.Otp).NotNull();
		When(request => request.Otp is not null, () =>
		{
			RuleFor(request => request.Otp!.RecipientsEmail)
				.NotEmpty()
				.EmailAddress()
				.When(request => request.Otp!.Required);
		});

		RuleFor(request => request.NotificationEmail)
			.MaximumLength(320)
			.EmailAddress()
			.WithMessage($"'{nameof(StoreNewSecretMessageRequest.NotificationEmail)}' must be a valid email address.")
			.When(request => string.IsNullOrWhiteSpace(request.NotificationEmail) is false);

		RuleFor(request => request.EncryptionKeySha256)
			.Matches("^[a-fA-F0-9]{64}$")
			.WithMessage($"'{nameof(StoreNewSecretMessageRequest.EncryptionKeySha256)}' must be a valid SHA-256 hexadecimal string.");
	}

	private static bool BeValidInitializationVector(string? iv)
	{
		return TryGetBase64DecodedLength(iv, out var length)
			&& length == AesGcmIvLength;
	}

	private static bool BeValidCipherText(string? cipherText)
	{
		return TryGetBase64DecodedLength(cipherText, out var length)
			&& length >= AesGcmTagLength
			&& length <= MaxCipherTextBytes;
	}

	private static bool TryGetBase64DecodedLength(string? value, out int decodedLength)
	{
		decodedLength = 0;

		if (string.IsNullOrEmpty(value) || value.Length % 4 != 0 || value.Length > MaxBase64Length) return false;

		var paddingLength = 0;
		var paddingStarted = false;

		for (var i = 0; i < value.Length; i++)
		{
			var character = value[i];

			if (character == '=')
			{
				if (i < value.Length - 2) return false;

				paddingStarted = true;
				paddingLength++;
				continue;
			}

			if (paddingStarted) return false;

			var isBase64Character =
				character is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '+' or '/';

			if (!isBase64Character) return false;
		}

		decodedLength = (value.Length / 4 * 3) - paddingLength;

		return true;
	}
}
