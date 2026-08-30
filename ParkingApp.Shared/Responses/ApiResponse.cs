namespace ParkingApp.Shared.Responses
{
	/// <summary>
	/// Base wrapper for all API responses, so every endpoint returns the same shape:
	/// a success flag, an optional message, and an optional list of errors.
	/// </summary>
	public class ApiResponse
	{
		/// <summary>True if the operation succeeded.</summary>
		public bool Success { get; set; }

		/// <summary>Optional human-readable message (e.g. an error summary).</summary>
		public string? Message { get; set; }

		/// <summary>Detailed error descriptions, when the operation failed.</summary>
		public List<string> Errors { get; set; } = new();
	}


	/// <summary>
	/// Generic API response that also carries a payload of type <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">The type of the returned value.</typeparam>
	public class ApiResponse<T> : ApiResponse
	{
		/// <summary>The returned value on success; default when the operation failed.</summary>
		public T? Value { get; set; }

		/// <summary>Builds a successful response wrapping the given value.</summary>
		/// <param name="value">The payload to return.</param>
		/// <param name="message">An optional success message.</param>
		public static ApiResponse<T> Ok(T value, string? message = null) =>
			new() { Success = true, Value = value, Message = message };

		/// <summary>Builds a failed response with a message and optional detailed errors.</summary>
		/// <param name="message">A short description of what went wrong.</param>
		/// <param name="errors">Optional detailed error descriptions.</param>
		public static ApiResponse<T> Fail(string message, List<string>? errors = null) =>
			new()
			{
				Success = false,
				Message = message,
				Errors = errors ?? new List<string>()
			};
	}
}
