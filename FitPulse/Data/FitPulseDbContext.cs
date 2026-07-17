namespace FitPulse.Data
{
    using Microsoft.EntityFrameworkCore;
    using FitPulse.Models;

    public class FitPulseDbContext : DbContext
    {
        public FitPulseDbContext(DbContextOptions<FitPulseDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<MaintenanceLog> MaintenanceLogs { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<TrainingSession> TrainingSessions { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<DiscountCode> DiscountCodes { get; set; }
    }
}