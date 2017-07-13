using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using FinalProject;

namespace FinalProject.Migrations
{
    [DbContext(typeof(UserContext))]
    partial class UserContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("FinalProject.User", b =>
                {
                    b.Property<string>("userId")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("access");

                    b.Property<string>("firstName")
                        .IsRequired();

                    b.Property<string>("lastName")
                        .IsRequired();

                    b.Property<string>("phrase")
                        .IsRequired();

                    b.Property<string>("speakerId")
                        .IsRequired();

                    b.HasKey("userId");

                    b.ToTable("Users");
                });
        }
    }
}
