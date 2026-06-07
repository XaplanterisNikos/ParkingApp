using System.Security.Cryptography.X509Certificates;

namespace ParkingApp.Shared.Parking
{
	public class ParkingEntryDto
	{
		public int Id { get; set; }
		public int RegisteredByEmployeeId { get; set; }
		public ParkingPositionData ParkingPosition { get; set; } = new();
		public string Car { get; set; } = string.Empty;
		public string DriverName { get; set;  } = string.Empty;
		public DateTime EntryDateTime { get; set; }
		public DateTime? ExitDateTime { get; set; }
	}
}
