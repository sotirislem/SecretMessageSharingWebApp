using Microsoft.AspNet.SignalR;
using Microsoft.EntityFrameworkCore;
using SecretMessageSharingWebApp.Data;
using SecretMessageSharingWebApp.Hubs;
using SecretMessageSharingWebApp.Middlewares;
using SecretMessageSharingWebApp.Repositories;
using SecretMessageSharingWebApp.Services;

// builder
var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureLogging(logging =>
{
	logging.ClearProviders();
	logging.AddConsole();
});

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

GlobalHost.DependencyResolver.Register(
		typeof(SecretMessageDeliveryNotificationHub),
		() => new SecretMessageDeliveryNotificationHub(
			LoggerFactory
				.Create(logging => logging.AddConsole())
				.CreateLogger<SecretMessageDeliveryNotificationHub>()
			)
		);

builder.Services.AddSignalR();

// app
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}
else
{
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