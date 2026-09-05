using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ParkingApp.Api.Data;
using ParkingApp.Api.Data.Entities;
using ParkingApp.Api.MultiTenancy;
using ParkingApp.Api.Services.Auth;
using ParkingApp.Api.Services.Branches;
using ParkingApp.Api.Services.Companies;
using ParkingApp.Api.Services.Floors;
using ParkingApp.Api.Services.Parking;
using ParkingApp.Api.Services.Spots;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- Persistence ---
// Register the EF Core context, backed by SQL Server. The connection string
// lives in configuration so it can differ per environment.
builder.Services.AddDbContext<ParkingDbContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("ParkingDb"));
});

// --- Identity ---
// Registers user/role management and password hashing on top of ParkingDbContext.
// AddIdentityCore (rather than AddIdentity) deliberately omits cookie auth,
// so JWT bearer remains the single authentication scheme.
builder.Services.AddIdentityCore<ApplicationUser>()
	.AddRoles<IdentityRole>()                  // enables RoleManager (used by the seeder)
	.AddEntityFrameworkStores<ParkingDbContext>() // persist users/roles via EF Core
	.AddSignInManager();                       // enables password checks at login

// --- Authentication (JWT) ---
// Defines how the API validates a bearer token on every incoming request.
// These parameters MUST mirror what TokenService writes into the token,
// otherwise every token is rejected with 401.
builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
	.AddJwtBearer(options =>
	{
		var jwt = builder.Configuration.GetSection("Jwt");
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,              // the token was issued by us
			ValidIssuer = jwt["Issuer"],
			ValidateAudience = true,            // the token is meant for our client
			ValidAudience = jwt["Audience"],
			ValidateLifetime = true,            // the token has not expired
			ValidateIssuerSigningKey = true,    // the token has not been tampered with
			IssuerSigningKey = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes(jwt["Key"]!))
		};
	});

// --- CORS ---
// The Blazor WASM client runs on a different origin than the API, so the
// browser needs explicit permission to call it. Restricted to the dev origins.
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

// --- MVC / application services ---
builder.Services.AddControllers();

// Application services, registered as scoped (one instance per request).
builder.Services.AddScoped<ITokenService, TokenService>();          // issues JWTs at login
builder.Services.AddScoped<IParkingEntryService, ParkingEntryService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IFloorService, FloorService>();
builder.Services.AddScoped<ISpotService, SpotService>();

// Multi-tenancy: resolves the current tenant from the request's token.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, TenantProvider>();

// --- API documentation (Swagger, Development only) ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed baseline data (roles + demo owner) once at startup.
// A manual scope is needed because UserManager/RoleManager are scoped services
// and there is no HTTP request in scope during startup.
using (var scope = app.Services.CreateScope())
{
	await DbSeeder.SeedAsync(scope.ServiceProvider);
}

// Swagger UI is only exposed while developing.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("ParkingClientPolicy");

// Authentication must come before authorization: first work out WHO the caller
// is (read/validate the token), then decide WHAT they are allowed to do (roles).
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
