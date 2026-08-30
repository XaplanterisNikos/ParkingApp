using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ParkingApp.Api.Data.Entities;

namespace ParkingApp.Api.Data
{
	/// <summary>
	/// EF Core database context. Extends <see cref="IdentityDbContext{TUser}"/> so that
	/// ASP.NET Core Identity (users, roles, claims, tokens) is persisted alongside the
	/// application's own entities such as <see cref="Company"/>.
	/// </summary>
	public class ParkingDbContext : IdentityDbContext<ApplicationUser>
	{
		public ParkingDbContext(DbContextOptions<ParkingDbContext> options) : base(options) { }

		/// <summary>Tenant companies. The root of the multi-tenant model.</summary>
		public DbSet<Company> Companies => Set<Company>();
		public DbSet<ParkingEntry> ParkingEntries => Set<ParkingEntry>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			// MUST run first: configures all the Identity tables. Skipping this
			// (or calling it after our own config) breaks the Identity schema.
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Company>(entity =>
			{
				entity.ToTable("Companies");

				entity.HasKey(company => company.Id);

				// SQL Server generates a sequential GUID on insert: keeps ids
				// non-guessable while avoiding the index fragmentation that
				// random client-side GUIDs cause on a clustered primary key.
				entity.Property(company => company.Id)
					.HasDefaultValueSql("NEWSEQUENTIALID()");

				entity.Property(company => company.Name)
					.IsRequired()
					.HasMaxLength(200);

				// One company has many users; each user belongs to exactly one company.
				entity.HasMany(company => company.Users)
					.WithOne(user => user.Company)
					.HasForeignKey(user => user.CompanyId)
					.OnDelete(DeleteBehavior.Restrict);
			});

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
