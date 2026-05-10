using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Controllers
{
	[Authorize(Roles = "Admin")]
	public class StudentController : Controller
	{
		private readonly ApplicationDbContext _context;

		public StudentController(ApplicationDbContext context)
		{
			_context = context;
		}

		// GET: Student
		public async Task<IActionResult> Index(string searchTerm, string classFilter)
		{
			var students = _context.Students
				.Include(s => s.Class)
				.Include(s => s.Faculty)
				.AsQueryable();

			if (!string.IsNullOrEmpty(searchTerm))
			{
				students = students.Where(s => s.MSSV.Contains(searchTerm) ||
											  s.FullName.Contains(searchTerm));
			}

			if (!string.IsNullOrEmpty(classFilter))
			{
				students = students.Where(s => s.ClassId == classFilter);
			}

			ViewBag.Classes = await _context.Classes.ToListAsync();
			return View(await students.ToListAsync());
		}

		// GET: Student/Detail/18710205
		public async Task<IActionResult> Detail(string id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var student = await _context.Students
				.Include(s => s.Class)
				.Include(s => s.Faculty)
				.FirstOrDefaultAsync(m => m.MSSV == id);

			if (student == null)
			{
				return NotFound();
			}

			ViewBag.Classes = await _context.Classes.ToListAsync();
			ViewBag.Faculties = await _context.Faculties.ToListAsync();

			return View(student);
		}

		// POST: Student/Detail/18710205
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Detail(string id, Student student)
		{
			if (id != student.MSSV)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(student);
					await _context.SaveChangesAsync();
					TempData["Success"] = "Đã lưu thông tin sinh viên thành công!";
					return RedirectToAction(nameof(Index));
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!StudentExists(student.MSSV))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
			}

			ViewBag.Classes = await _context.Classes.ToListAsync();
			ViewBag.Faculties = await _context.Faculties.ToListAsync();
			return View(student);
		}

		// POST: Student/Delete/18710205
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(string id)
		{
			var student = await _context.Students.FindAsync(id);
			if (student != null)
			{
				_context.Students.Remove(student);
				await _context.SaveChangesAsync();
				TempData["Success"] = "Đã xóa sinh viên thành công!";
			}

			return RedirectToAction(nameof(Index));
		}

		private bool StudentExists(string id)
		{
			return _context.Students.Any(e => e.MSSV == id);
		}
	}
}
