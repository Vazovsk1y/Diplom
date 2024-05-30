using Diplom.WPF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Diplom.WPF.Data.Configurations;

public class CrewMemberConfiguration : IEntityTypeConfiguration<CrewMember>
{
    public void Configure(EntityTypeBuilder<CrewMember> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Type).HasConversion(e => e.ToString(), i => Enum.Parse<CrewMemberType>(i));
    }
}
