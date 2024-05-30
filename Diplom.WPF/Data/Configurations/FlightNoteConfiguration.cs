using Diplom.WPF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Diplom.WPF.Data.Configurations;

public class FlightNoteConfiguration : IEntityTypeConfiguration<FlightNote>
{
    public void Configure(EntityTypeBuilder<FlightNote> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Type).HasConversion(e => e.ToString(), i => Enum.Parse<FlightNoteTypes>(i));
    }
}
