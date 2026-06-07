using ParkingApp.Api.Data;
using ParkingApp.Shared.Parking;
using Microsoft.EntityFrameworkCore;
using ParkingApp.Shared.Responses;
using ParkingApp.Api.Data.Entities;
using System.Text.Json;

namespace ParkingApp.Api.Services.Parking
{
	public class ParkingEntryService : IParkingEntryService
	{
		#region private Fields - Constructor
		private readonly ParkingDbContext _dbContext;

		public ParkingEntryService(ParkingDbContext dbContext)
		{
			_dbContext = dbContext;
		}
		#endregion

		#region CREATE
		public async Task<ApiResponse<ParkingEntryDto>> CreateAsync(CreateParkingEntryRequest request)
		{
			var parkingEntry = new ParkingEntry
			{
				RegisteredByEmployeeId = request.RegtisteredByEmployeeId,
				ParkingPositionJson = SerializeParkingPosition(request.ParkingPosition),
				Car = request.Car,
				DriverName = request.DriverName,
				EntryDateTime = DateTime.Now,
				IsDeleted = false
			};
			await _dbContext.ParkingEntries.AddAsync(parkingEntry);
			await _dbContext.SaveChangesAsync();

			return new ApiResponse<ParkingEntryDto>
			{
				Success = true,
				Value = MapToDto(parkingEntry),
				Message = "Parking entry created successfully."
			};
		}
		#endregion

		#region DELETE
		public async Task<ApiResponse<bool>> DeleteAsync(int id)
		{
			var parkingEntry = await _dbContext.ParkingEntries
		   .FirstOrDefaultAsync(parkingEntry =>
			   parkingEntry.Id == id &&
			   !parkingEntry.IsDeleted);

			if (parkingEntry is null)
			{
				return new ApiResponse<bool>
				{
					Success = false,
					Value = false,
					Message = "Parking entry was not found."
				};
			}

			parkingEntry.IsDeleted = true;
			parkingEntry.DeletedAt = DateTime.Now;

			await _dbContext.SaveChangesAsync();

			return new ApiResponse<bool>
			{
				Success = true,
				Value = true,
				Message = "Parking entry deleted successfully."
			};
		}
		#endregion

		#region GET METHODS
		public async Task<ApiResponse<List<ParkingEntryDto>>> GetAllAsync(bool includeDeleted = false)
		{
			var query = _dbContext.ParkingEntries.AsQueryable();

			if (!includeDeleted)
			{
				query = query.Where(parkingEntry => !parkingEntry.IsDeleted);
			}

			var parkingEntries = await query
				.OrderByDescending(parkingEntry => parkingEntry.EntryDateTime)
				.ToListAsync();

			var parkingEntryDtos = parkingEntries
				.Select(MapToDto)
				.ToList();

			return new ApiResponse<List<ParkingEntryDto>>
			{
				Success = true,
				Value = parkingEntryDtos,
				Message = "Parking entries loaded successfully."
			};
		}

		public async Task<ApiResponse<ParkingEntryDto>> GetByIdAsync(int id, bool includeDeleted = false)
		{
			var query = _dbContext.ParkingEntries.AsQueryable();

			if (!includeDeleted)
			{
				query = query.Where(parkingEntry => !parkingEntry.IsDeleted);
			}

			var parkingEntry = await query
				.FirstOrDefaultAsync(parkingEntry => parkingEntry.Id == id);

			if (parkingEntry is null)
			{
				return new ApiResponse<ParkingEntryDto>
				{
					Success = false,
					Message = "Parking entry was not found."
				};
			}

			return new ApiResponse<ParkingEntryDto>
			{
				Success = true,
				Value = MapToDto(parkingEntry),
				Message = "Parking entry loaded successfully."
			};
		}
		#endregion

		#region UPDATE
		public async Task<ApiResponse<ParkingEntryDto>> UpdateAsync(int id, UpdateParkingEntryRequest request)
		{
			var parkingEntry = await _dbContext.ParkingEntries
				.FirstOrDefaultAsync(parkingEntry => parkingEntry.Id == id 
				&& !parkingEntry.IsDeleted);

			if(parkingEntry is null)
			{
				return new ApiResponse<ParkingEntryDto>
				{
					Success = false,
					Message = "Parking entrty was not found."
				};
			}

			parkingEntry.RegisteredByEmployeeId = request.RegtisteredByEmployeeId;
			parkingEntry.ParkingPositionJson = SerializeParkingPosition(request.ParkingPosition);
			parkingEntry.Car = request.Car;
			parkingEntry.DriverName = request.DriverName;
			parkingEntry.EntryDateTime = request.EntryDateTime;
			parkingEntry.ExitDateTime = request.ExitDateTime;
			parkingEntry.UpdatedAt = DateTime.Now;

			await _dbContext.SaveChangesAsync();

			return new ApiResponse<ParkingEntryDto>
			{
				Success = true,
				Value = MapToDto(parkingEntry),
				Message = "Parking entry updated succesfully."
			};

		}
		#endregion

		#region Private Methods
		private static ParkingEntryDto MapToDto(ParkingEntry parkingEntry)
		{
			return new ParkingEntryDto
			{
				Id = parkingEntry.Id,
				RegisteredByEmployeeId = parkingEntry.RegisteredByEmployeeId,
				ParkingPosition = DeserializeParkingPosition(parkingEntry.ParkingPositionJson),
				Car = parkingEntry.Car,
				DriverName = parkingEntry.DriverName,
				EntryDateTime = parkingEntry.EntryDateTime,
				ExitDateTime = parkingEntry.ExitDateTime,
				IsDeleted = parkingEntry.IsDeleted,
				DeletedAt = parkingEntry.DeletedAt

			};
		}

		private static string SerializeParkingPosition(ParkingPositionData parkingPosition)
		{
			return JsonSerializer.Serialize(parkingPosition);
		}

		private static ParkingPositionData DeserializeParkingPosition(string parkingPositionJson)
		{
			if (string.IsNullOrWhiteSpace(parkingPositionJson)) return new ParkingPositionData();

			return JsonSerializer.Deserialize<ParkingPositionData>(parkingPositionJson) ?? new ParkingPositionData();
		}
		#endregion
	}
}
