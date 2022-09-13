using Microsoft.Extensions.Options;

namespace SecretMessageSharingWebApp.Extensions
{
	public static class ServiceCollectionExtentions
	{
        public static void BindConfigurationSettings<T>(this IServiceCollection services, IConfiguration configuration) where T : class
        {
            services.AddOptions<T>()
                .Bind(configuration)
                .ValidateDataAnnotations();

            services.AddTransient(resolver => resolver.GetRequiredService<IOptions<T>>().Value);
        }
    }
}
