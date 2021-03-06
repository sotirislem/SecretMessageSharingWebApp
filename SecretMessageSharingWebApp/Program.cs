using FastEndpoints;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.ApplicationInsights;
using SecretMessageSharingWebApp;
using SecretMessageSharingWebApp.Data;
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
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);
#endif

builder.Services.AddDbContext<SecretMessagesDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IMemoryCacheService, MemoryCacheService>();

builder.Services.AddSingleton<ISecretMessageDeliveryNotificationHubService, SecretMessageDeliveryNotificationHubService>();

builder.Services.AddScoped<ISecretMessagesRepository, SecretMessagesRepository>();
builder.Services.AddScoped<ISecretMessagesService, SecretMessagesService>();

builder.Services.AddScoped<IGetLogsRepository, GetLogsRepository>();
builder.Services.AddScoped<IGetLogsService, GetLogsService>();

builder.Services.AddHostedService<SecretMessagesAutoCleanerBackgroundService>();

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

builder.Services.AddSignalR();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
	options.ForwardedHeaders =
		ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// app
var app = builder.Build();

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