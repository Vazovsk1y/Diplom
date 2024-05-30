using Diplom.WPF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Diplom.WPF.Data.Configurations;

public class FlightConfiguration : IEntityTypeConfiguration<Flight>
{
    public void Configure(EntityTypeBuilder<Flight> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.Number).IsUnique();

        builder.Property(e => e.Status).HasConversion(e => e.ToString(), i => Enum.Parse<FlightStatus>(i));

        builder.HasMany(e => e.CrewMembers).WithOne(e => e.Flight).HasForeignKey(e => e.FlightId);

        builder.HasMany(e => e.Notes).WithOne().HasForeignKey(e => e.FlightId);

        builder.HasOne(e => e.Plane).WithMany().HasForeignKey(e => e.PlaneId);
    }
}
