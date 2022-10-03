using System.Text;

namespace SecretMessageSharingWebApp.Extensions;

public static class ExceptionExtensions
{
	public static string GetAllErrorMessages(this Exception ex)
	{
		if (ex is null)
			throw new ArgumentNullException(nameof(ex));

		StringBuilder sb = new StringBuilder();

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
