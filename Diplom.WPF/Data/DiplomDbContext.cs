using Diplom.WPF.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Diplom.WPF.Data;

public class DiplomDbContext(DbContextOptions<DiplomDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Flight> Flights { get; set; }
    public DbSet<CrewMember> CrewMembers { get; set; }
    public DbSet<CrewMemberFlight> CrewMemberFlights { get; set; }
    public DbSet<FlightNote> FlightNotes { get; set; }
    public DbSet<Plane> Planes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
