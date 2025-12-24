using Microsoft.EntityFrameworkCore;
using VitiligoTracker.Models;

namespace VitiligoTracker.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<TreatmentRecord> TreatmentRecords { get; set; }
    }
}
