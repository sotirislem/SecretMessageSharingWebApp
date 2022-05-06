using Microsoft.EntityFrameworkCore;
using SecretsManagerWebApp.Data;
using SecretsManagerWebApp.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SecretMessageDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ISecretMessagesRepository, SecretMessagesRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}
else {
	app.UseHsts();	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
} 

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllers();
//app.MapControllerRoute(
//	name: "default",
//	pattern: "api/{controller}/{action}/{id?}");

app.Run();
