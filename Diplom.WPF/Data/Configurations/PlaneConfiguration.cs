using Diplom.WPF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Diplom.WPF.Data.Configurations;

public class PlaneConfiguration : IEntityTypeConfiguration<Plane>
{
    public void Configure(EntityTypeBuilder<Plane> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.RegistrationNumber).IsUnique();

        builder.Property(e => e.Type).HasConversion(e => e.ToString(), i => Enum.Parse<PlaneType>(i));
    }
}
