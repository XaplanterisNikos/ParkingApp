using ParkingApp.Shared.Parking;
using ParkingApp.Shared.Responses;
using System.Net.Http.Json;

namespace ParkingApp.Client.Consumers.Parking
{
	public class ParkingEntriesConsumer : IParkingEntriesConsumer
	{
		#region Fields
		private readonly HttpClient _httpclient;

		public ParkingEntriesConsumer(HttpClient httpclient)
		{
			_httpclient = httpclient;
		}

		#endregion

		#region Create
		public async Task<ApiResponse<ParkingEntryDto>> CreateParkingEntryAsync(CreateParkingEntryRequest request)
		{
			var httpResponse = await _httpclient.PostAsJsonAsync(
							   "api/parking-entries",
							   request);

			return await ReadApiResponseAsync<ParkingEntryDto>(httpResponse);

		}
		#endregion

		#region Delete
		public async Task<ApiResponse<bool>> DeleteParkingEntryAsync(int id)
		{
			var httpResponse = await _httpclient.DeleteAsync(
				$"api/parking-entries/{id}");

			return await ReadApiResponseAsync<bool>(httpResponse);

		}
		#endregion

		#region GetMethods
		public async Task<ApiResponse<List<ParkingEntryDto>>> GetAllParkingEntriesAsync(bool includeDeleted = false)
		{
			var endPoint = includeDeleted
				? "api/parking-entries?includeDeleted=true"
				: "api/parking-entries";
			var HttpResponse = await _httpclient.GetAsync(endPoint);
			return await ReadApiResponseAsync<List<ParkingEntryDto>>(HttpResponse);
		}

		public async Task<ApiResponse<ParkingEntryDto>> GetParkingEntryByIdAsync(int id, bool includeDeleted = false)
		{
			var endpoint = includeDeleted
			   ? $"api/parking-entries/{id}?includeDeleted=true"
			   : $"api/parking-entries/{id}";

			var httpResponse = await _httpclient.GetAsync(endpoint);

			return await ReadApiResponseAsync<ParkingEntryDto>(httpResponse);

		}
		#endregion

		#region Update
		public async Task<ApiResponse<ParkingEntryDto>> UpdateParkingEntryAsync(int id, UpdateParkingEntryRequest request)
		{
			var httpResponse = await _httpclient.PutAsJsonAsync(
				$"api/parking-entries/{id}",
				request);

			return await ReadApiResponseAsync<ParkingEntryDto>(httpResponse);

		}
		#endregion

		#region Methods
		private static async Task<ApiResponse<T>> ReadApiResponseAsync<T>(
		HttpResponseMessage httpResponse)
		{
			var apiResponse = await httpResponse.Content
				.ReadFromJsonAsync<ApiResponse<T>>();

			if (apiResponse is not null)
			{
				return apiResponse;
			}

			return new ApiResponse<T>
			{
				Success = false,
				Message = "The API response could not be read.",
				Errors =
				[
					$"HTTP status code: {(int)httpResponse.StatusCode} {httpResponse.StatusCode}"
				]
			};
		}
		#endregion

	}
}
