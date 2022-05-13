using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.ApplicationInsights;
using SecretMessageSharingWebApp.Data;
using SecretMessageSharingWebApp.Hubs;
using SecretMessageSharingWebApp.Middlewares;
using SecretMessageSharingWebApp.Repositories;
using SecretMessageSharingWebApp.Services;

// builder
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);

builder.Services.AddDbContext<SecretMessageDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<GetLogsDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ISecretMessagesRepository, SecretMessagesRepository>();
builder.Services.AddScoped<IGetLogsRepository, GetLogsRepository>();

builder.Services.AddHostedService<SecretMessagesAutoCleanerBackgroundService>();

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<MemoryCacheService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
	options.ForwardedHeaders =
		ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

#pragma warning disable ASP0000
GlobalHost.DependencyResolver.Register(
		typeof(SecretMessageDeliveryNotificationHub),
		() => new SecretMessageDeliveryNotificationHub(
			builder.Services.BuildServiceProvider().GetService<ILoggerFactory>()!
				.CreateLogger<SecretMessageDeliveryNotificationHub>()
			)
		);
#pragma warning restore ASP0000

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
	app.UseHsts();  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseExceptionHandler("/error");

app.UseMiddleware<HttpRequestTimeMiddleware>();

app.MapHub<SecretMessageDeliveryNotificationHub>("/signalR/secret-message-delivery-notification-hub");

app.UseEndpoints(configure => configure.MapControllers());

app.MapFallbackToFile("index.html");

app.Run();