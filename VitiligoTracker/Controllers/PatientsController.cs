using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitiligoTracker.Data;
using VitiligoTracker.Models;

namespace VitiligoTracker.Controllers
{
    [Authorize]
    public class PatientsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PatientsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Patients
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                return View(await _context.Patients.ToListAsync());
            }
            else
            {
                var user = await _userManager.GetUserAsync(User);
                var userName = user?.UserName;
                if (string.IsNullOrEmpty(userName)) return View(new List<Patient>());

                return View(await _context.Patients.Where(p => p.OwnerPhone == userName).ToListAsync());
            }
        }

        // GET: Patients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients
                .Include(p => p.TreatmentRecords)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (patient == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (patient.OwnerPhone != user?.UserName)
                {
                    return Forbid();
                }
            }

            // Sort records by date descending
            patient.TreatmentRecords = patient.TreatmentRecords.OrderByDescending(r => r.Date).ToList();

            return View(patient);
        }

        // GET: Patients/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Patients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Name,OwnerPhone,StartDate,Notes")] Patient patient)
        {
            if (ModelState.IsValid)
            {
                _context.Add(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        // POST: Patients/AddRecord
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRecord([Bind("PatientId,Date,BodyPart,IrradiationDose,DurationSeconds,Reaction")] TreatmentRecord record)
        {
            // Check authorization
            var patient = await _context.Patients.FindAsync(record.PatientId);
            if (patient == null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (patient.OwnerPhone != user?.UserName)
                {
                    return Forbid();
                }
            }

            // Remove CumulativeDose from ModelState validation as it is calculated server-side
            ModelState.Remove("CumulativeDose");

            if (ModelState.IsValid)
            {
                // Calculate Cumulative Dose
                var lastRecord = await _context.TreatmentRecords
                    .Where(t => t.PatientId == record.PatientId)
                    .OrderByDescending(t => t.Date)
                    .ThenByDescending(t => t.Id)
                    .FirstOrDefaultAsync();

                double previousCumulative = lastRecord?.CumulativeDose ?? 0;
                record.CumulativeDose = Math.Round(previousCumulative + record.IrradiationDose, 2);

                _context.TreatmentRecords.Add(record);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = record.PatientId });
            }
            // If invalid, redirect back to details (simplified error handling for this demo)
            return RedirectToAction(nameof(Details), new { id = record.PatientId });
        }

        // GET: Patients/EditRecord/5
        public async Task<IActionResult> EditRecord(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var record = await _context.TreatmentRecords.Include(r => r.Patient).FirstOrDefaultAsync(r => r.Id == id);
            if (record == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (record.Patient?.OwnerPhone != user?.UserName)
                {
                    return Forbid();
                }
            }

            return View(record);
        }

        // POST: Patients/EditRecord/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRecord(int id, [Bind("Id,PatientId,Date,BodyPart,IrradiationDose,DurationSeconds,Reaction")] TreatmentRecord record)
        {
            if (id != record.Id)
            {
                return NotFound();
            }

            // Check authorization
            var patient = await _context.Patients.FindAsync(record.PatientId);
            if (patient == null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (patient.OwnerPhone != user?.UserName)
                {
                    return Forbid();
                }
            }

            ModelState.Remove("CumulativeDose");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(record);
                    await _context.SaveChangesAsync();

                    // Recalculate all doses for this patient
                    await RecalculateDoses(record.PatientId);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TreatmentRecordExists(record.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = record.PatientId });
            }
            return View(record);
        }

        // POST: Patients/DeleteRecord/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRecord(int id)
        {
            var record = await _context.TreatmentRecords.Include(r => r.Patient).FirstOrDefaultAsync(r => r.Id == id);
            if (record != null)
            {
                if (!User.IsInRole("Admin"))
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (record.Patient?.OwnerPhone != user?.UserName)
                    {
                        return Forbid();
                    }
                }

                int patientId = record.PatientId;
                _context.TreatmentRecords.Remove(record);
                await _context.SaveChangesAsync();

                // Recalculate all doses for this patient
                await RecalculateDoses(patientId);

                return RedirectToAction(nameof(Details), new { id = patientId });
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task RecalculateDoses(int patientId)
        {
            var records = await _context.TreatmentRecords
                .Where(r => r.PatientId == patientId)
                .OrderBy(r => r.Date)
                .ThenBy(r => r.Id)
                .ToListAsync();

            double cumulative = 0;
            foreach (var r in records)
            {
                cumulative += r.IrradiationDose;
                r.CumulativeDose = Math.Round(cumulative, 2);
            }
            _context.UpdateRange(records);
            await _context.SaveChangesAsync();
        }

        private bool TreatmentRecordExists(int id)
        {
            return _context.TreatmentRecords.Any(e => e.Id == id);
        }

        // GET: Patients/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
             if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients
                .FirstOrDefaultAsync(m => m.Id == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // POST: Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
