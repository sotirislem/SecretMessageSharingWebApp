namespace SecretMessageSharingWebApp.Models.DbContext
{
	public class SecretMessage
	{
		public string Id { get; set; }

		public DateTime CreatedDateTime { get; set; }

		public bool DeleteOnRetrieve { get; set; }

		public string JsonData { get; set; }

		public string? CreatorIP { get; set; }

		public string? CreatorClientInfo { get; set; }

		public SecretMessage()
		{
			Id = Guid.NewGuid().ToString("N");
			CreatedDateTime = DateTime.Now;
			DeleteOnRetrieve = true;
		}
	}
}
