﻿// <auto-generated />
using System;
using BonusService.Common.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BonusService.Migrations
{
    [DbContext(typeof(BonusDbContext))]
    [Migration("20231211134345_Init")]
    partial class Init
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("BonusService.Common.Postgres.Entity.BonusProgram", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("BankId")
                        .HasColumnType("integer");

                    b.Property<int>("BonusProgramType")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("DateStart")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("DateStop")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ExecutionCron")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("FrequencyType")
                        .HasColumnType("integer");

                    b.Property<int>("FrequencyValue")
                        .HasColumnType("integer");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset>("LastUpdated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("BonusPrograms");
                });

            modelBuilder.Entity("BonusService.Common.Postgres.Entity.BonusProgramHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("BankId")
                        .HasColumnType("integer");

                    b.Property<int>("BonusProgramId")
                        .HasColumnType("integer");

                    b.Property<int>("ClientBalancesCount")
                        .HasColumnType("integer");

                    b.Property<long>("DurationMilliseconds")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("ExecTimeEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("ExecTimeStart")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("LastUpdated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("TotalBonusSum")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("BonusProgramId");

                    b.ToTable("BonusProgramHistory");
                });

            modelBuilder.Entity("BonusService.Common.Postgres.Entity.BonusProgramLevel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AwardPercent")
                        .HasColumnType("integer");

                    b.Property<int>("AwardSum")
                        .HasColumnType("integer");

                    b.Property<int>("BonusProgramId")
                        .HasColumnType("integer");

                    b.Property<long>("Condition")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("LastUpdated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Level")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("BonusProgramId");

                    b.ToTable("BonusProgramsLevels");
                });

            modelBuilder.Entity("BonusService.Common.Postgres.Entity.OwnerMaxBonusPay", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("LastUpdated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("MaxBonusPayPercentages")
                        .HasColumnType("integer");

                    b.Property<int>("OwnerId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId")
                        .IsUnique();

                    b.ToTable("OwnerMaxBonusPays");
                });

            modelBuilder.Entity("BonusService.Common.Postgres.Entity.Transaction", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<int>("BankId")
                        .HasColumnType("integer");

                    b.Property<long?>("BonusBase")
                        .HasColumnType("bigint");

                    b.Property<int?>("BonusProgramId")
                        .HasColumnType("integer");

                    b.Property<long>("BonusSum")
                        .HasColumnType("bigint");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("EzsId")
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("LastUpdated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("OwnerId")
                        .HasColumnType("integer");

                    b.Property<string>("PersonId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TransactionId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<string>("UserName")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("TransactionId")
                        .IsUnique();

                    NpgsqlIndexBuilderExtensions.IncludeProperties(b.HasIndex("TransactionId"), new[] { "BonusSum" });

                    b.HasIndex("PersonId", "BankId");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("BonusService.Common.Postgres.Entity.TransactionHistory", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<int>("BankId")
                        .HasColumnType("integer");

                    b.Property<long?>("BonusBase")
                        .HasColumnType("bigint");

                    b.Property<int?>("BonusProgramId")
                        .HasColumnType("integer");

                    b.Property<long>("BonusSum")
                        .HasColumnType("bigint");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("EzsId")
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("LastUpdated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("OwnerId")
                        .HasColumnType("integer");

                    b.Property<string>("PersonId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TransactionId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<string>("UserName")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("TransactionHistory");
                });

            modelBuilder.Entity("BonusService.Common.Postgres.Entity.BonusProgramHistory", b =>
                {
                    b.HasOne("BonusService.Common.Postgres.Entity.BonusProgram", "BonusProgram")
                        .WithMany("BonusProgramHistory")
                        .HasForeignKey("BonusProgramId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BonusProgram");
                });

            modelBuilder.Entity("BonusService.Common.Postgres.Entity.BonusProgramLevel", b =>
                {
                    b.HasOne("BonusService.Common.Postgres.Entity.BonusProgram", "BonusProgram")
                        .WithMany("ProgramLevels")
                        .HasForeignKey("BonusProgramId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BonusProgram");
                });

            modelBuilder.Entity("BonusService.Common.Postgres.Entity.BonusProgram", b =>
                {
                    b.Navigation("BonusProgramHistory");

                    b.Navigation("ProgramLevels");
                });
#pragma warning restore 612, 618
        }
    }
}
