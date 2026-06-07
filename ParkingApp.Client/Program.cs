using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ParkingApp.Client;
using ParkingApp.Client.Consumers.Parking;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
	BaseAddress = new Uri("https://localhost:7005/")
});

builder.Services.AddScoped<IParkingEntriesConsumer, ParkingEntriesConsumer>();

await builder.Build().RunAsync();
