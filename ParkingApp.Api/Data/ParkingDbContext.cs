using Microsoft.EntityFrameworkCore;
using ParkingApp.Api.Data.Entities;

namespace ParkingApp.Api.Data
{
	public class ParkingDbContext : DbContext
	{
		public ParkingDbContext(DbContextOptions<ParkingDbContext> options) : base(options) { }
		public DbSet<ParkingEntry> ParkingEntries => Set<ParkingEntry>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
				base.OnModelCreating(modelBuilder);
			modelBuilder.Entity<ParkingEntry>(entity =>
			{
				entity.ToTable("ParkingEntries");

				entity.HasKey(parkingEntry => parkingEntry.Id);

				entity.Property(parkingEntry => parkingEntry.RegisteredByEmployeeId)
					.IsRequired();

				entity.Property(parkingEntry => parkingEntry.ParkingPositionJson)
					.IsRequired();

				entity.Property(parkingEntry => parkingEntry.Car)
					.IsRequired()
					.HasMaxLength(50);

				entity.Property(parkingEntry => parkingEntry.DriverName)
					.IsRequired()
					.HasMaxLength(100);

				entity.Property(parkingEntry => parkingEntry.EntryDateTime)
					.IsRequired();

				entity.Property(parkingEntry => parkingEntry.CreatedAt)
					.IsRequired();
			});

		}
	}
}
