namespace SecretMessageSharingWebApp.Models.Api
{
	public class ErrorResponse
	{
		public bool Error { get; } = true;

		public string Message { get; set; } = "An unexpected error occurred while trying to process your request.";
	}
}
