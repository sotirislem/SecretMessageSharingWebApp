namespace SecretsManagerWebApp.Models.DbContext
{
	public class SecretMessage
	{
		public string Id { get; set; } = Guid.NewGuid().ToString("N");

		public DateTime CreatedDateTime { get; set; } = DateTime.Now;

		public bool DeleteOnRetrieve { get; set; } = true;

		public string JsonData { get; set; }

		public string? CreatorIP { get; set; }

		public string? CreatorClientInfo { get; set; }
	}
}
