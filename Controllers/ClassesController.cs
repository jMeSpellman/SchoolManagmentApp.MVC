using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagmentApp.MVC.Data;
using SchoolManagmentApp.MVC.Models;

namespace SchoolManagmentApp.MVC.Controllers
{
    public class ClassesController : Controller
    {
        private readonly SchoolManagementDbContext _context;

        public ClassesController(SchoolManagementDbContext context)
        {
            _context = context;
        }

        // GET: Classes
        public async Task<IActionResult> Index()
        {
            // SELECT * FROM Classes As cls
            // LEFT JOIN Courses As co On cls.CourseId = co.Id
            // LEFT JOIN Lecturers As l on cls.LecturerId = l.Id
            var schoolManagementDbContext = _context
                .Classes.Include(q => q.Course)
                .Include(q => q.Lecturer);
            return View(await schoolManagementDbContext.ToListAsync());
        }

        // GET: Classes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @class = await _context
                .Classes.Include(q => q.Course)
                .Include(q => q.Lecturer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@class == null)
            {
                return NotFound();
            }

            return View(@class);
        }

        // GET: Classes/Create
        public IActionResult Create()
        {
            CreateLists();
            return View();
        }

        // POST: Classes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,LecturerId,CourseId,Time")] Class @class)
        {
            if (ModelState.IsValid)
            {
                _context.Add(@class);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            CreateLists();

            return View(@class);
        }

        // GET: Classes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @class = await _context.Classes.FindAsync(id);
            if (@class == null)
            {
                return NotFound();
            }

            CreateLists();

            return View(@class);
        }

        // POST: Classes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,LecturerId,CourseId,Time")] Class @class
        )
        {
            if (id != @class.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@class);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassExists(@class.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            CreateLists();

            return View(@class);
        }

        // GET: Classes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @class = await _context
                .Classes.Include(q => q.Course)
                .Include(q => q.Lecturer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@class == null)
            {
                return NotFound();
            }

            return View(@class);
        }

        // POST: Classes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @class = await _context.Classes.FindAsync(id);
            if (@class != null)
            {
                _context.Classes.Remove(@class);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<ActionResult> ManageEnrollments(int classId)
        {
            var @class = await _context
                .Classes.Include(q => q.Course)
                .Include(q => q.Lecturer)
                .Include(q => q.Enrollments)
                .ThenInclude(q => q.Student)
                .FirstOrDefaultAsync(m => m.Id == classId);

            var students = await _context.Students.ToListAsync();
            var model = new ClassEnrollmentViewModel();
            model.Class = new ClassViewModel
            {
                Id = @class.Id,
                CourseName = $"{@class.Course.Code} - {@class.Course.Name}",
                LecturerName = $"{@class.Lecturer.FirstName} {@class.Lecturer.LastName}",
                Time = @class.Time.ToString()
            };

            foreach (var s in students)
            {
                model.Students.Add(
                    new StudentEnrollmentViewModel
                    {
                        Id = s.Id,
                        FirstName = s.FirstName,
                        LastName = s.LastName,
                        IsEnrolled = (
                            @class?.Enrollments?.Any(q => q.StudentId == s.Id)
                        ).GetValueOrDefault()
                    }
                );
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnrollStudent(int classId, int studentId, bool shouldEnroll)
        {
            var enrollment = new Enrollment();
            if (shouldEnroll == true)
            {
                enrollment.ClassId = classId;
                enrollment.StudentId = studentId;
                await _context.AddAsync(enrollment);
            }
            else
            {
                enrollment = await _context.Enrollments.FirstOrDefaultAsync(q =>
                    q.ClassId == classId && q.StudentId == studentId
                );
                if (enrollment != null)
                {
                    _context.Remove(enrollment);
                }
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageEnrollments), new { id = classId });
        }

        private bool ClassExists(int id)
        {
            return _context.Classes.Any(e => e.Id == id);
        }

        private void CreateLists()
        {
            var classes = _context.Courses.Select(q => new
            {
                CourseTitle = $"{q.Code} - {q.Name} ({q.Credits} Credits)",
                Id = q.Id
            });
            ViewData["CourseId"] = new SelectList(classes, "Id", "CourseTitle");

            var lecturers = _context.Lecturers.Select(q => new
            {
                Fullname = $"{q.FirstName} {q.LastName}",
                Id = q.Id
            });
            ViewData["LecturerId"] = new SelectList(lecturers, "Id", "Fullname");
        }
    }
}
