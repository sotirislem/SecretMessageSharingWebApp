﻿namespace SecretMessageSharingWebApp.Data.Entities;

public sealed record GetLogEntity : IDbEntity
{
	public string Id { get; private init; }

	public DateTime RequestDateTime { get; init; }

	public string? RequestCreatorIP { get; init; }

	public string? RequestClientInfo { get; init; }

	public string SecretMessageId { get; init; }

	public DateTime? SecretMessageCreatedDateTime { get; init; }

	public string? SecretMessageCreatorIP { get; init; }

	public string? SecretMessageCreatorClientInfo { get; init; }


	private GetLogEntity() { }

	public GetLogEntity(string guid, DateTime timestamp)
	{
		Id = timestamp.ToString("dd_MM_yyyy|HH:mm:ss|ffff") + '|' + guid;
	}
}
