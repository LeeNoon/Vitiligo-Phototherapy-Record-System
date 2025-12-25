using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VitiligoTracker.Models;
using Microsoft.AspNetCore.Identity;

namespace VitiligoTracker.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

    public DbSet<Patient> Patients { get; set; }
    public DbSet<TreatmentRecord> TreatmentRecords { get; set; }
    public DbSet<BodyPartDict> BodyPartDicts { get; set; }
    }
}
