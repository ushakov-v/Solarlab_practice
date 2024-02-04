using Birthdays.Models;
using Microsoft.AspNetCore.Mvc;

namespace Birthdays.Controllers
{
    public class PeopleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PeopleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Действие для отображения всего списка ДР
        public IActionResult Index(string nameSortOrder, string dobSortOrder)
        {
            ViewBag.NameSortParam = string.IsNullOrEmpty(nameSortOrder) ? "asc" : "";
            ViewBag.DobSortParam = string.IsNullOrEmpty(dobSortOrder) ? "asc" : "";

            var viewList = _context.Birthdays.ToList();

            if (nameSortOrder == "asc")
            {
                viewList = viewList.OrderBy(b => b.Name).ToList();
                ViewBag.NameSortParam = "desc";
            }
            else if (nameSortOrder == "desc")
            {
                viewList = viewList.OrderByDescending(b => b.Name).ToList();
                ViewBag.NameSortParam = "asc";
            }
            if (dobSortOrder == "asc")
            {
                viewList = viewList.OrderBy(b => b.DateOfBirth).ToList();
                ViewBag.DobSortParam = "desc";
            }
            else if (dobSortOrder == "desc")
            {
                viewList = viewList.OrderByDescending(b => b.DateOfBirth).ToList();
                ViewBag.DobSortParam = "asc";
            }

            return View(viewList);
        }

        // Действие для отображения списка сегодняшних и ближайших ДР
        public IActionResult TodayAndUpcoming()
        {
            DateTime today = DateTime.Today;

            var model = new TodayAndUpcomingViewModel();

            model.TodayList = _context.Birthdays
                .Where(e => e.DateOfBirth.Day == today.Day && e.DateOfBirth.Month == today.Month && e.DateOfBirth.Year != today.Year)
                .OrderBy(e => e.DateOfBirth)
                .ToList();

            model.UpcomingList = _context.Birthdays
                .Where(b => b.DateOfBirth.Day > today.Day && b.DateOfBirth.Month == today.Month && b.DateOfBirth.Year != today.Year)
                .OrderBy(b => b.DateOfBirth)
                .ToList();

            return View(model);
        }

        // Действие для добавления записи в список ДР (GET-запрос)
        public IActionResult Add()
        {
            return View();
        }

        // Действие для добавления записи в список ДР (POST-запрос)
        [HttpPost]
        public IActionResult Add(People birthday, IFormFile photo)
        {
            if (photo != null && photo.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    photo.CopyTo(memoryStream);
                    birthday.Photo = memoryStream.ToArray();
                }
            }
            _context.Birthdays.Add(birthday);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // Действие для удаления записи из списка ДР
        public IActionResult Delete(int id)
        {
            var entryToDelete = _context.Birthdays.FirstOrDefault(e => e.Id == id);
            if (entryToDelete != null)
            {
                return View(entryToDelete);
            }
            return NotFound();
        }

        // Действие для удаления записи из списка ДР (POST-запрос)
        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            var entryToDelete = _context.Birthdays.FirstOrDefault(e => e.Id == id);
            if (entryToDelete != null)
            {
                _context.Birthdays.Remove(entryToDelete);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return NotFound();
        }

        // Действие для редактирования записи в списке ДР (GET-запрос)
        public IActionResult Edit(int id)
        {
            var entryToEdit = _context.Birthdays.FirstOrDefault(e => e.Id == id);
            if (entryToEdit != null)
            {
                return View(entryToEdit);
            }
            return RedirectToAction("Index");
        }

        // Действие для редактирования записи в списке ДР (POST-запрос)
        [HttpPost]
        public IActionResult Edit(int id, People updatedEntry, IFormFile photo)
        {
            var birthday = _context.Birthdays.Find(id);
            if (birthday == null)
            {
                return NotFound();
            }

            if (photo != null && photo.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    photo.CopyTo(memoryStream);
                    birthday.Photo = memoryStream.ToArray();
                }
            }

            birthday.Name = updatedEntry.Name;
            birthday.DateOfBirth = updatedEntry.DateOfBirth;

            _context.Birthdays.Update(birthday);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeletePhoto(int id)
        {
            var birthday = _context.Birthdays.Find(id);
            if (birthday == null)
            {
                return NotFound();
            }

            birthday.Photo = null;
            _context.SaveChanges();

            return RedirectToAction("Edit", new { id });
        }
    }
}