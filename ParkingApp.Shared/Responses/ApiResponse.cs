namespace ParkingApp.Shared.Responses
{
	/// <summary>
	/// Wrapper class 
	/// To response απο API θα έχει την ίδια μορφή
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ApiResponse
	{
		public bool Success { get; set; }

		public string? Message { get; set; }

		public List<string> Errors { get; set; } = new();
	}

	public class ApiResponse<T> : ApiResponse
	{
		public T? Value { get; set; }
	}
}
