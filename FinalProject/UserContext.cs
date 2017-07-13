using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject
{
    public class UserContext : DbContext
    {
        internal DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source = Users.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Make UserId required
            modelBuilder.Entity<User>().Property(u => u.userId).IsRequired();
            //Make SpeakerId required
            modelBuilder.Entity<User>().Property(u => u.speakerId).IsRequired();
            //Make First Name required
            modelBuilder.Entity<User>().Property(u => u.firstName).IsRequired();
            //Make Last Name required
            modelBuilder.Entity<User>().Property(u => u.lastName).IsRequired();
            //Make Phrase required
            modelBuilder.Entity<User>().Property(u => u.phrase).IsRequired();
            //Make Access required
            modelBuilder.Entity<User>().Property(u => u.access).IsRequired();
        }
    }
}
