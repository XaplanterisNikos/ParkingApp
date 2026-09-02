using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ParkingApp.Client;
using ParkingApp.Client.Consumers.Auth;
using ParkingApp.Client.Consumers.Branches;
using ParkingApp.Client.Consumers.Companies;
using ParkingApp.Client.Consumers.Parking;
using ParkingApp.Client.Services.Auth;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
	BaseAddress = new Uri("https://localhost:7005/")
});

builder.Services.AddScoped<IParkingEntriesConsumer, ParkingEntriesConsumer>();

// Company API client (pure HTTP communication): calls the protected /api/companies
// endpoints, attaching the stored JWT as a Bearer token.
builder.Services.AddScoped<ICompaniesConsumer, CompaniesConsumer>();

// Branches API client (pure HTTP communication): calls the protected /api/branches endpoints.
builder.Services.AddScoped<IBranchesConsumer, BranchesConsumer>();


// --- Authentication / authorization (client-side) ---

// Browser localStorage access — where we persist the JWT between requests/refreshes.
builder.Services.AddBlazoredLocalStorage();

// Turns on Blazor's authorization system (AuthenticationState, AuthorizeView, [Authorize]).
builder.Services.AddAuthorizationCore();

// Owns "where the token lives" (read/write/clear in localStorage).
builder.Services.AddScoped<ITokenStore, TokenStore>();

// Register the concrete provider once...
builder.Services.AddScoped<JwtAuthenticationStateProvider>();

// ...and expose that SAME instance as THE AuthenticationStateProvider for Blazor.
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
	sp.GetRequiredService<JwtAuthenticationStateProvider>());

// Auth API client: sends login requests to the API and reads the response.
// Pure HTTP communication — no token storage, no state.
builder.Services.AddScoped<IAuthConsumer, AuthConsumer>();

// Auth orchestration: coordinates the login/logout flow — delegates the HTTP call
// to IAuthConsumer, persists the token, and refreshes the authentication state.
builder.Services.AddScoped<IAuthService, AuthService>();

await builder.Build().RunAsync();
