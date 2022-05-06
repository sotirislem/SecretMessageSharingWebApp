using CSharpVitamins;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SecretsManagerWebApp.Models.Api;
using SecretsManagerWebApp.Repositories;

namespace SecretsManagerWebApp.Controllers
{
	[ApiController]
	[Route("api/secret-messages")]
	public class SecretMessagesController : ControllerBase
	{
		private readonly ISecretMessagesRepository _secretMessagesRepository;

		public SecretMessagesController(ISecretMessagesRepository secretMessagesRepository)
		{
			this._secretMessagesRepository = secretMessagesRepository;
		}

		[HttpPost("store")]
		public string Store(SecretMessage secretMessageData)
		{
			var secretMessage = new Models.DbContext.SecretMessage
			{
				JsonData = JsonConvert.SerializeObject(secretMessageData),
				CreatedDateTime = DateTime.Now,
				CreatorIP = HttpContext.Connection.RemoteIpAddress?.ToString()
			};
			_secretMessagesRepository.Store(secretMessage);
			return secretMessage.Id;
		}

		[HttpGet("get/{id}")]
		public async Task<SecretMessage?> Get(string id)
		{
			var res = await _secretMessagesRepository.Get(id);
			if (res is not null)
			{
				return JsonConvert.DeserializeObject<SecretMessage>(res.JsonData);
			}

			return null;
		}
	}
}