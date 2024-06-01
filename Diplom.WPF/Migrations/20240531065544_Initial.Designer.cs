﻿// <auto-generated />
using System;
using Diplom.WPF.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Diplom.WPF.Migrations
{
    [DbContext(typeof(DiplomDbContext))]
    [Migration("20240531065544_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Diplom.WPF.Models.CrewMember", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("CrewMembers");
                });

            modelBuilder.Entity("Diplom.WPF.Models.CrewMemberFlight", b =>
                {
                    b.Property<Guid>("CrewMemberId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("FlightId")
                        .HasColumnType("uuid");

                    b.HasKey("CrewMemberId", "FlightId");

                    b.HasIndex("FlightId");

                    b.ToTable("CrewMemberFlights");
                });

            modelBuilder.Entity("Diplom.WPF.Models.Flight", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("ArrivalDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("DepartureDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("From")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Number")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("PlaneId")
                        .HasColumnType("uuid");

                    b.Property<double>("Range")
                        .HasColumnType("double precision");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("To")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Number")
                        .IsUnique();

                    b.HasIndex("PlaneId");

                    b.ToTable("Flights");
                });

            modelBuilder.Entity("Diplom.WPF.Models.FlightNote", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("FlightId")
                        .HasColumnType("uuid");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("FlightId");

                    b.ToTable("FlightNotes");
                });

            modelBuilder.Entity("Diplom.WPF.Models.Plane", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<double>("FuelCapacity")
                        .HasColumnType("double precision");

                    b.Property<double>("FuelConsumption")
                        .HasColumnType("double precision");

                    b.Property<string>("Manufacturer")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<double>("MaxSpeed")
                        .HasColumnType("double precision");

                    b.Property<string>("Model")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("PassengersCapacity")
                        .HasColumnType("integer");

                    b.Property<double>("Range")
                        .HasColumnType("double precision");

                    b.Property<string>("RegistrationNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RegistrationNumber")
                        .IsUnique();

                    b.ToTable("Planes");
                });

            modelBuilder.Entity("Diplom.WPF.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("NormalizedUserName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Diplom.WPF.Models.CrewMemberFlight", b =>
                {
                    b.HasOne("Diplom.WPF.Models.CrewMember", "CrewMember")
                        .WithMany()
                        .HasForeignKey("CrewMemberId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Diplom.WPF.Models.Flight", "Flight")
                        .WithMany("CrewMembers")
                        .HasForeignKey("FlightId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CrewMember");

                    b.Navigation("Flight");
                });

            modelBuilder.Entity("Diplom.WPF.Models.Flight", b =>
                {
                    b.HasOne("Diplom.WPF.Models.Plane", "Plane")
                        .WithMany()
                        .HasForeignKey("PlaneId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Plane");
                });

            modelBuilder.Entity("Diplom.WPF.Models.FlightNote", b =>
                {
                    b.HasOne("Diplom.WPF.Models.Flight", null)
                        .WithMany("Notes")
                        .HasForeignKey("FlightId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Diplom.WPF.Models.Flight", b =>
                {
                    b.Navigation("CrewMembers");

                    b.Navigation("Notes");
                });
#pragma warning restore 612, 618
        }
    }
}