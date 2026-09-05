using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ParkingApp.Api.Data.Entities;
using ParkingApp.Api.MultiTenancy;

namespace ParkingApp.Api.Data
{
	/// <summary>
	/// EF Core database context. Extends <see cref="IdentityDbContext{TUser}"/> so that
	/// ASP.NET Core Identity (users, roles, claims, tokens) is persisted alongside the
	/// application's own entities such as <see cref="Company"/> and <see cref="Branch"/>.
	/// Tenant-owned entities are automatically filtered to the current tenant via a
	/// global query filter driven by <see cref="ITenantProvider"/>.
	/// </summary>
	public class ParkingDbContext : IdentityDbContext<ApplicationUser>
	{
		private readonly ITenantProvider _tenantProvider;
		public ParkingDbContext(DbContextOptions<ParkingDbContext> options, ITenantProvider tenantProvider) : base(options)
		{ 
			_tenantProvider = tenantProvider;
		}

		/// <summary>Tenant companies. The root of the multi-tenant model.</summary>
		public DbSet<Company> Companies => Set<Company>();
		/// <summary>Parking branches, each owned by a company.</summary>
		public DbSet<Branch> Branches => Set<Branch>();
		/// <summary>Floors within branches, each owned by a company.</summary>
		public DbSet<Floor> Floors => Set<Floor>();
		public DbSet<ParkingEntry> ParkingEntries => Set<ParkingEntry>();
		/// <summary>Parking spots on floors, each owned by a company.</summary>
		public DbSet<ParkingSpot> ParkingSpots => Set<ParkingSpot>();

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

			modelBuilder.Entity<Branch>(entity =>
			{
				entity.ToTable("Branches");

				entity.HasKey(branch => branch.Id);

				entity.Property(branch => branch.Id)
					.HasDefaultValueSql("NEWSEQUENTIALID()");

				entity.Property(branch => branch.Name)
					.IsRequired()
					.HasMaxLength(200);

				// Index on the tenant key: every query filters by CompanyId, so this keeps
				// that filtering fast as the table grows.
				entity.HasIndex(branch => branch.CompanyId);

				// GLOBAL QUERY FILTER — the heart of tenant isolation.
				// EF Core adds "WHERE CompanyId = <current tenant>" to EVERY query on Branch,
				// automatically. A query can never accidentally return another tenant's data.
				entity.HasQueryFilter(branch => branch.CompanyId == _tenantProvider.CurrentCompanyId);
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

			modelBuilder.Entity<Floor>(entity =>
			{
				entity.ToTable("Floors");
				entity.HasKey(floor => floor.Id);
				entity.Property(floor => floor.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
				entity.Property(floor => floor.Name).IsRequired().HasMaxLength(100);

				// Index on the tenant key — the global filter uses CompanyId on every query.
				entity.HasIndex(floor => floor.CompanyId);

				// A branch has many floors; a floor belongs to one branch.
				// Restrict delete: you can't delete a branch that still has floors.
				entity.HasOne<Branch>()
					.WithMany()
					.HasForeignKey(floor => floor.BranchId)
					.OnDelete(DeleteBehavior.Restrict);

				// Same global query filter pattern as Branch — automatic tenant isolation.
				entity.HasQueryFilter(floor => floor.CompanyId == _tenantProvider.CurrentCompanyId);
			});

			modelBuilder.Entity<ParkingSpot>(entity =>
			{
				entity.ToTable("ParkingSpots");

				entity.HasKey(spot => spot.Id);

				entity.Property(spot => spot.Id)
					.HasDefaultValueSql("NEWSEQUENTIALID()");

				entity.Property(spot => spot.Number)
					.IsRequired()
					.HasMaxLength(20);

				// Store the enum as int (the default) — compact and fast.
				entity.Property(spot => spot.Size)
					.HasConversion<int>();

				entity.HasIndex(spot => spot.CompanyId);

				// A floor has many spots; a spot belongs to one floor.
				entity.HasOne<Floor>()
					.WithMany()
					.HasForeignKey(spot => spot.FloorId)
					.OnDelete(DeleteBehavior.Restrict);

				// Same automatic tenant isolation as the other tenant entities.
				entity.HasQueryFilter(spot => spot.CompanyId == _tenantProvider.CurrentCompanyId);
			});

		}
	}
}
