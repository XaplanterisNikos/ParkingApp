using Microsoft.EntityFrameworkCore;
using ParkingApp.Api.Data;
using ParkingApp.Api.Services.Parking;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ParkingDbContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("ParkingDb"));
});

builder.Services.AddCors(options =>
{
	options.AddPolicy("ParkingClientPolicy", policy =>
	{
		policy
			.WithOrigins(
				"https://localhost:7065",
				"http://localhost:5192")
			.AllowAnyHeader()
			.AllowAnyMethod();
	});
});
// Add services to the container.

builder.Services.AddControllers();


builder.Services.AddScoped<IParkingEntryService, ParkingEntryService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("ParkingClientPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
