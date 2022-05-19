﻿namespace SecretMessageSharingWebApp.Models.Api
{
	public class RecentlyStoredMessage
	{
		public string Id { get; set; }
		
		public DateTime CreatedDateTime { get; set; }

		public DeliveryDetails? DeliveryDetails { get; set; }
	}

	public class DeliveryDetails
	{
		public DateTime DeliveredAt { get; set; }

		public string RecipientIP { get; set; }

		public string RecipientClientInfo { get; set; }
	}
}
