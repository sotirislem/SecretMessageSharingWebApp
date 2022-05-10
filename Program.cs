using Microsoft.EntityFrameworkCore;
using SecretsManagerWebApp.Data;
using SecretsManagerWebApp.Hubs;
using SecretsManagerWebApp.Middlewares;
using SecretsManagerWebApp.Repositories;
using SecretsManagerWebApp.Services;

// builder
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

builder.Services.AddDbContext<SecretMessageDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<GetLogsDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ISecretMessagesRepository, SecretMessagesRepository>();
builder.Services.AddScoped<IGetLogsRepository, GetLogsRepository>();

builder.Services.AddHostedService<SecretMessagesAutoCleanerBackgroundService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(
		policy =>
		{
			policy.WithOrigins("https://localhost:44490");
			policy.AllowAnyHeader();
			policy.AllowAnyMethod();
			policy.AllowCredentials();
		});
});

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
app.UseCors();

app.UseMiddleware<HttpRequestTimeMiddleware>();
app.UseExceptionHandler("/error");

app.MapHub<SecretMessageDeliveryNotificationHub>("signalR/secret-message-delivery-notification-hub");
app.MapControllers();

app.Run();
