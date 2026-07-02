using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ApplicationAccountingSystem.Domain.Model;

namespace ApplicationAccountingSystem.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Tickets> Tickets { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<SLAPolicy> SLAPolicies { get; set; }
        public DbSet<TicketHistory> TicketHistories { get; set; }
        public DbSet<Attachment> Attachments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=helpdesk;Username=user;Password=pass");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Fluent API конфигурации будут здесь
            // modelBuilder.ApplyConfiguration(new TicketConfiguration());
            // modelBuilder.ApplyConfiguration(new CommentConfiguration());
            // modelBuilder.ApplyConfiguration(new UserConfiguration());
            // modelBuilder.ApplyConfiguration(new SLAPolicyConfiguration());
        }
    }
}
