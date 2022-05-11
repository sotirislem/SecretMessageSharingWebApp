namespace SecretMessageSharingWebApp.Models.Api
{
	public class ErrorResponse
	{
		public bool Error { get; }

		public string Message { get; set; }

		public ErrorResponse()
		{
			Error = true;
			Message = "An unexpected error occurred while trying to process your request.";
		}
	}
}
