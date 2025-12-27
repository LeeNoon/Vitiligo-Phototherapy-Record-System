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

        // Ajax接口：根据部位返回最新建议剂量
        [HttpGet]
        public async Task<IActionResult> GetSuggestDose(int patientId, string bodyPart)
        {
            var lastRecord = await _context.TreatmentRecords
                .Where(r => r.PatientId == patientId && r.BodyPart == bodyPart)
                .OrderByDescending(r => r.Date)
                .FirstOrDefaultAsync();
            double suggestDose = 0;
            if (lastRecord != null)
            {
                double lastDose = await GetLastNonZeroDose(patientId, bodyPart, lastRecord.Date);
                switch (lastRecord.Reaction)
                {
                    case Models.ReactionType.None:
                        suggestDose = Math.Round(lastDose + 0.1, 2);
                        break;
                    case Models.ReactionType.MildErythema:
                        suggestDose = Math.Round(lastDose + 0.05, 2);
                        break;
                    case Models.ReactionType.ModerateErythema:
                        suggestDose = lastDose;
                        break;
                    case Models.ReactionType.SevereErythema:
                        suggestDose = Math.Round(lastDose * 0.8, 2);
                        break;
                    case Models.ReactionType.VerySevereErythema:
                        suggestDose = Math.Round(lastDose * 0.5, 2);
                        break;
                    case Models.ReactionType.Blister:
                        suggestDose = Math.Round(lastDose * 0.5, 2);
                        break;
                    default:
                        suggestDose = 0;
                        break;
                }
            }
            return Json(new { suggestDose = suggestDose.ToString("F2") });
        }

        // 管理员刷新所有历史治疗记录的建议剂量
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> RefreshAllSuggestDose()
        {
            var allPatients = await _context.Patients.Include(p => p.TreatmentRecords).ToListAsync();
            foreach (var patient in allPatients)
            {
                var records = patient.TreatmentRecords
                    .OrderBy(r => r.BodyPart)
                    .ThenBy(r => r.Date)
                    .ThenBy(r => r.Id)
                    .ToList();
                string? lastPart = null;
                double lastNonZeroDose = 0;
                foreach (var r in records)
                {
                    if (lastPart != r.BodyPart)
                    {
                        lastNonZeroDose = 0;
                        lastPart = r.BodyPart;
                    }
                    switch (r.Reaction)
                    {
                        case Models.ReactionType.None:
                            r.SuggestDose = Math.Round(lastNonZeroDose + 0.1, 2);
                            break;
                        case Models.ReactionType.MildErythema:
                            r.SuggestDose = Math.Round(lastNonZeroDose + 0.05, 2);
                            break;
                        case Models.ReactionType.ModerateErythema:
                            r.SuggestDose = lastNonZeroDose;
                            break;
                        case Models.ReactionType.SevereErythema:
                            r.SuggestDose = Math.Round(lastNonZeroDose * 0.8, 2);
                            break;
                        case Models.ReactionType.VerySevereErythema:
                            r.SuggestDose = Math.Round(lastNonZeroDose * 0.5, 2);
                            break;
                        case Models.ReactionType.Blister:
                            r.SuggestDose = Math.Round(lastNonZeroDose * 0.5, 2);
                            break;
                        default:
                            r.SuggestDose = 0;
                            break;
                    }
                    if (r.IrradiationDose > 0)
                    {
                        lastNonZeroDose = r.IrradiationDose;
                    }
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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

            // Sort records by date ascending
            patient.TreatmentRecords = patient.TreatmentRecords.OrderBy(r => r.Date).ToList();

            // 查询部位字典
            var bodyParts = await _context.BodyPartDicts.OrderBy(b => b.Id).ToListAsync();
            ViewBag.BodyPartDict = bodyParts;

            // 计算建议剂量（默认取最近一条记录的部位和剂量）
            double? suggestDose = 0;
            var lastRecord = patient.TreatmentRecords.LastOrDefault();
            if (lastRecord != null)
            {
                // 查找同部位的最近一条记录
                var lastPartRecord = patient.TreatmentRecords
                    .Where(r => r.BodyPart == lastRecord.BodyPart)
                    .OrderByDescending(r => r.Date)
                    .FirstOrDefault();
                suggestDose = lastPartRecord?.SuggestDose ?? 0;
            }
            ViewBag.SuggestDose = suggestDose;

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
    public async Task<IActionResult> AddRecord([Bind("PatientId,Date,BodyPart,IrradiationDose,SuggestDose,Reaction")] TreatmentRecord record)
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

            // 自动计算建议剂量（同一部位，取前一次同部位非0照射剂量）
            if (string.IsNullOrEmpty(record.BodyPart))
            {
                ModelState.AddModelError("BodyPart", "治疗部位不能为空");
                return RedirectToAction(nameof(Details), new { id = record.PatientId });
            }
            double lastDose = await GetLastNonZeroDose(record.PatientId, record.BodyPart, record.Date);
            switch (record.Reaction)
            {
                case Models.ReactionType.None:
                    record.SuggestDose = Math.Round(lastDose + 0.1, 2);
                    break;
                case Models.ReactionType.MildErythema:
                    record.SuggestDose = Math.Round(lastDose + 0.05, 2);
                    break;
                case Models.ReactionType.ModerateErythema:
                    record.SuggestDose = lastDose;
                    break;
                case Models.ReactionType.SevereErythema:
                    record.SuggestDose = Math.Round(lastDose * 0.8, 2);
                    break;
                case Models.ReactionType.VerySevereErythema:
                    record.SuggestDose = Math.Round(lastDose * 0.5, 2);
                    break;
                case Models.ReactionType.Blister:
                    record.SuggestDose = Math.Round(lastDose * 0.5, 2);
                    break;
                default:
                    record.SuggestDose = 0;
                    break;
            }

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

            // 获取原始记录
            var original = await _context.TreatmentRecords.FindAsync(id);
            if (original == null) return NotFound();

            // 部分更新：只更新有效的字段，无效字段清空或保持原值
            original.Date = ModelState["Date"].Errors.Count == 0 ? record.Date : original.Date;
            original.BodyPart = ModelState["BodyPart"].Errors.Count == 0 ? record.BodyPart : null; // 清空
            original.IrradiationDose = ModelState["IrradiationDose"].Errors.Count == 0 ? record.IrradiationDose : 0; // 清空
            original.Reaction = ModelState["Reaction"].Errors.Count == 0 ? record.Reaction : original.Reaction; // 保持原值

            try
            {
                _context.Update(original);
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

        private async Task<double> GetLastNonZeroDose(int patientId, string bodyPart, DateTime beforeDate)
        {
            var record = await _context.TreatmentRecords
                .Where(r => r.PatientId == patientId && r.BodyPart == bodyPart && r.Date < beforeDate && r.IrradiationDose > 0)
                .OrderByDescending(r => r.Date)
                .FirstOrDefaultAsync();
            return record?.IrradiationDose ?? 0;
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
