namespace SecretMessageSharingWebApp.Models.Api.Responses
{
	public class ApiErrorResponse
	{
		public bool Error => true;

		public string Message { get; init; }

		public ApiErrorResponse()
		{
			Message = "An unexpected error occurred while trying to process your request.";
		}
	}
}
