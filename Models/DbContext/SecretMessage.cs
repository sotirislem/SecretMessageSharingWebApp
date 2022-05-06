namespace SecretsManagerWebApp.Models.DbContext
{
	public class SecretMessage
	{
		public string Id { get; set; } = Guid.NewGuid().ToString("N");

		public string JsonData { get; set; }

		public DateTime CreatedDateTime { get; set; }

		public string? CreatorIP { get; set; }

		public bool DeleteOnRetrieve { get; set; } = true;
	}
}
