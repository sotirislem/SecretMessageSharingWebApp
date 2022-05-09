using CSharpVitamins;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SecretsManagerWebApp.Helpers;
using SecretsManagerWebApp.Middlewares;
using SecretsManagerWebApp.Models.Api;
using SecretsManagerWebApp.Repositories;
using System.Diagnostics;

namespace SecretsManagerWebApp.Controllers
{
	[ApiController]
	[Route("api/secret-messages")]
	public class SecretMessagesController : ControllerBase
	{
		private readonly ISecretMessagesRepository _secretMessagesRepository;
		private readonly IGetLogsRepository _getLoggerRepository;

		public SecretMessagesController(ISecretMessagesRepository secretMessagesRepository, IGetLogsRepository getLoggerRepository)
		{
			this._secretMessagesRepository = secretMessagesRepository;
			this._getLoggerRepository = getLoggerRepository;
		}

		[HttpPost("store")]
		public string Store(SecretMessage secretMessageData)
		{
			var secretMessage = new Models.DbContext.SecretMessage
			{
				JsonData = JsonConvert.SerializeObject(secretMessageData),
				CreatorIP = HttpContextHelper.GetClientIP(HttpContext),
				CreatorClientInfo = HttpContextHelper.GetClientInfo(HttpContext)
			};
			_secretMessagesRepository.Store(secretMessage);

			return secretMessage.Id;
		}

		[HttpGet("get/{id}")]
		public async Task<SecretMessage?> Get(string id)
		{
			var secretMessage = await _secretMessagesRepository.Get(id);
			var getLog = new Models.DbContext.GetLog
			{
				RequestDateTime = HttpContextHelper.GetRequestDateTime(HttpContext),
				RequestCreatorIP = HttpContextHelper.GetClientIP(HttpContext),
				RequestClientInfo = HttpContextHelper.GetClientInfo(HttpContext),
				SecretMessageId = id,
				SecretMessageExisted = (secretMessage is not null),
				SecretMessageCreatedDateTime = secretMessage?.CreatedDateTime,
				SecretMessageCreatorIP = secretMessage?.CreatorIP,
				SecretMessageCreatorClientInfo = secretMessage?.CreatorClientInfo
			};
			_getLoggerRepository.Add(getLog);

			if (secretMessage is not null)
			{
				return JsonConvert.DeserializeObject<SecretMessage>(secretMessage.JsonData);
			}
			return null;
		}
	}
}