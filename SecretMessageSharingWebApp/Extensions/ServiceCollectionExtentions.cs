using Microsoft.Extensions.Options;

namespace SecretMessageSharingWebApp.Extensions;

public static class ServiceCollectionExtentions
{
	public static void BindConfigurationSettings<TOptions>(this IServiceCollection services, IConfiguration configuration) where TOptions : class
	{
		services.AddOptions<TOptions>()
			.Bind(configuration)
			.ValidateDataAnnotations();

		services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<TOptions>>().Value);
	}
}
