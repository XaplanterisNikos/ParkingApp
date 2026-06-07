namespace ParkingApp.Api.Data.Entities
{
	/// <summary>
	/// Entity αντικείμενο που θα αντιστοιχεί αργότερα σε πίνακα της βάσης
	/// εσωτερικό database model
	/// </summary>
	public class ParkingEntry
	{
		public int Id { get; set; }
		public int RegisteredByEmployeeId { get; set; }
		public string ParkingPositionJson { get; set; } = string.Empty;
		public string Car { get; set; } = string.Empty;

		public string DriverName { get; set; } = string.Empty;

		public DateTime EntryDateTime { get; set; }

		public DateTime? ExitDateTime { get; set; }

		public bool IsDeleted { get; set; }

		public DateTime CreatedAt { get; set; }

		public DateTime? UpdatedAt { get; set; }

		public DateTime? DeletedAt { get; set; }
	}
}
