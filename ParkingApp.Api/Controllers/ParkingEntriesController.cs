using Microsoft.AspNetCore.Mvc;
using ParkingApp.Api.Services.Parking;

namespace ParkingApp.Api.Controllers
{
	[ApiController]
	[Route("api/parking-entries")]
	public class ParkingEntriesController :ControllerBase
	{
		private readonly IParkingEntryService _parkingEntryService;

		public ParkingEntriesController(IParkingEntryService parkingEntryService)
		{
			_parkingEntryService = parkingEntryService;
		}
		
	}
}
