using System.Text;

namespace SecretMessageSharingWebApp.Extensions;

public static class ExceptionExtensions
{
	public static string GetAllErrorMessages(this Exception ex)
	{
		ArgumentNullException.ThrowIfNull(ex);

		StringBuilder sb = new();

		do
		{
			if (!string.IsNullOrEmpty(ex.Message))
			{
				if (sb.Length > 0)
					sb.Append(" => ");

				sb.Append(ex.Message);
			}

			ex = ex.InnerException!;
		}
		while (ex is not null);

		return sb.ToString();
	}
}
