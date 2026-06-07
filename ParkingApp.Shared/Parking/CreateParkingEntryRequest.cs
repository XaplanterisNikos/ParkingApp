namespace ParkingApp.Shared.Parking
{
	public class CreateParkingEntryRequest
	{
		public int RegtisteredByEmployeeId { get; set; }
		public ParkingPositionData ParkingPosition { get; set; } = new();
		public string Car { get; set; } = string.Empty;
		public string DriverName { get; set; } = string.Empty;
	}
}
