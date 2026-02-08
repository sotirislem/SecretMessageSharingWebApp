using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using SecretMessageSharingWebApp;
using SecretMessageSharingWebApp.ConfigurationSettings;
using SecretMessageSharingWebApp.Data;
using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Hubs;
using SecretMessageSharingWebApp.Middlewares;

// builder
var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddServices();
builder.Services.AddBackgroundServices();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(Constants.SessionIdleTimeoutInMinutes);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument(o =>
{
	o.DocumentSettings = s =>
	{
		s.Title = Constants.AppName;
		s.Version = "v1";
		s.Description = $"API documentation for {Constants.AppName} (using FastEndpoints)";
	};
});

builder.Services.AddSignalR(hubOptions =>
{
	hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(10);
	hubOptions.ClientTimeoutInterval = TimeSpan.FromSeconds(15);
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
	options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
	
	options.KnownIPNetworks.Clear();
	options.KnownProxies.Clear();
	
	options.ForwardLimit = null; 
});

builder.Services.AddMiddlewares();
builder.Services.AddProblemDetails();

builder.Services.AddHttpContextAccessor();
builder.Services.SetRateLimiter();

// app
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var context = scope.ServiceProvider.GetService<SecretMessagesDbContext>()!;
	await context.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
	app.UseSwaggerGen();
}

app.UseForwardedHeaders();
app.UseHsts();

app.UseRateLimiter();
app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseMiddleware<HttpRequestTimeMiddleware>();
app.UseExceptionHandler();

app.MapHub<SecretMessageDeliveryNotificationHub>(SecretMessageDeliveryNotificationHub.Url);

app.UseFastEndpoints();
app.UseAuthorization();
app.MapFallbackToFile("index.html");

app.Run();