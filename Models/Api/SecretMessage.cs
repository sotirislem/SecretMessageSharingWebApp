namespace SecretMessageSharingWebApp.Models.Api
{
	public class SecretMessage
	{
		public string IV { get; set; }

		public string Salt { get; set; }

		public string CT { get; set; }
	}
}
