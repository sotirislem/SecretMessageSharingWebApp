using System.Text;

namespace SecretsManagerWebApp.Helpers
{
	public static class Extensions
	{
        public static string GetAllMessages(this Exception ex)
        {
            if (ex is null)
                throw new ArgumentNullException("ex");

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
            } while (ex is not null);

            return sb.ToString();
        }
    }
}
