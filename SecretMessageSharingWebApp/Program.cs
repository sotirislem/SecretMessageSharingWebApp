using FastEndpoints;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.ApplicationInsights;
using SecretMessageSharingWebApp;
using SecretMessageSharingWebApp.Configuration;
using SecretMessageSharingWebApp.Data;
using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Hubs;
using SecretMessageSharingWebApp.Middlewares;
using SecretMessageSharingWebApp.Repositories;
using SecretMessageSharingWebApp.Repositories.Interfaces;
using SecretMessageSharingWebApp.Services;
using SecretMessageSharingWebApp.Services.BackgroundServices;
using SecretMessageSharingWebApp.Services.Interfaces;

// builder
var builder = WebApplication.CreateBuilder(args);

#if (!DEBUG)
if (builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"] is not null)
{
	builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);
}
#endif

builder.Services.BindConfigurationSettings<CosmosDbConfigurationSettings>(builder.Configuration.GetSection("CosmosDb"));
builder.Services.BindConfigurationSettings<SendGridConfigurationSettings>(builder.Configuration.GetSection("SendGrid"));
builder.Services.BindConfigurationSettings<JwtConfigurationSettings>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddDbContext<SecretMessagesDbContext>((serviceProvider, options) =>
	//options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
	options.UseCosmos(
		accountEndpoint: serviceProvider.GetRequiredService<CosmosDbConfigurationSettings>().Endpoint,
		accountKey: serviceProvider.GetRequiredService<CosmosDbConfigurationSettings>().ApiKey,
		databaseName: serviceProvider.GetRequiredService<CosmosDbConfigurationSettings>().DbName
	)
);

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IMemoryCacheService, MemoryCacheService>();

builder.Services.AddSingleton<IDateTimeProviderService, DateTimeProviderService>();
builder.Services.AddSingleton<ISecretMessageDeliveryNotificationHubService, SecretMessageDeliveryNotificationHubService>();
builder.Services.AddSingleton<IOtpService, OtpService>();
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddSingleton<ISendGridEmailService, SendGridEmailService>();

builder.Services.AddScoped<ISecretMessagesRepository, SecretMessagesRepository>();
builder.Services.AddScoped<ISecretMessagesService, SecretMessagesService>();

builder.Services.AddScoped<IGetLogsRepository, GetLogsRepository>();
builder.Services.AddScoped<IGetLogsService, GetLogsService>();

builder.Services.AddHostedService<DbAutoCleanerBackgroundService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(Constants.SessionIdleTimeoutInMinutes);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

builder.Services.AddFastEndpoints();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR(hubOptions =>
{
	hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(10);
	hubOptions.ClientTimeoutInterval = TimeSpan.FromSeconds(15);
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
	options.ForwardedHeaders =
		ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// app
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var context = scope.ServiceProvider.GetService<SecretMessagesDbContext>();
	context!.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}
else
{
	app.UseForwardedHeaders();
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<HttpRequestTimeMiddleware>();

app.MapHub<SecretMessageDeliveryNotificationHub>(SecretMessageDeliveryNotificationHub.Url);

app.UseAuthorization();
app.UseFastEndpoints();

app.MapFallbackToFile("index.html");

app.Run();