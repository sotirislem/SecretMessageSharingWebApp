﻿using SecretMessageSharingWebApp.Models.Domain;

namespace SecretMessageSharingWebApp.Services.Interfaces;

public interface IOtpService
{
	public OneTimePassword Generate();

	public (bool isValid, bool canRetry, bool hasExpired) Validate(string otpInputCode, OneTimePassword inMemoryOtp);
}
