using Diplom.WPF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Diplom.WPF.Data.Configurations;

public class CrewMemberFlightConfiguration : IEntityTypeConfiguration<CrewMemberFlight>
{
    public void Configure(EntityTypeBuilder<CrewMemberFlight> builder)
    {
        builder.HasKey(e => new { e.CrewMemberId, e.FlightId });

        builder.HasOne(e => e.CrewMember).WithMany().HasForeignKey(e => e.CrewMemberId);

        builder.HasOne(e => e.Flight).WithMany().HasForeignKey(e => e.FlightId);
    }
}
