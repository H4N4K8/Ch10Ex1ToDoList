using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoList.Models;

namespace ToDoList.Controllers
{
    public class HomeController : Controller
    {
        private ToDoContext context;
        public HomeController(ToDoContext ctx) => context = ctx;

        public ViewResult Index(string id)
        {
            var model = new tOdOvIEWmODEL
            {
                Filters = new Filters(id),
                Categories = context.Categories.ToList(),
                Statuses = context.Statuses.ToList(),
                DueFilters = Filters.DueFilterValues
            };

            IQueryable<ToDo> query = context.ToDos
                .Include(t => t.Category)
                .Include(t => t.Status);

            if (model.Filters.HasCategory)
            {
                query = query.Where(t => t.CategoryId == model.Filters.CategoryId);
            }
            if (model.Filters.HasStatus)
            {
                query = query.Where(t => t.StatusId == model.Filters.StatusId);
            }
            if (model.Filters.HasDue)
            {
                var today = DateTime.Today;
                if (model.Filters.IsPast)
                    query = query.Where(t => t.DueDate < today);
                else if (model.Filters.IsFuture)
                    query = query.Where(t => t.DueDate > today);
                else if (model.Filters.IsToday)
                    query = query.Where(t => t.DueDate == today);
            }
            model.Tasks = query.OrderBy(t => t.DueDate).ToList();

            return View(model);
        }

        [HttpGet]
        public ViewResult Add()
        {
            //ViewBag.Categories = context.Categories.ToList();
            //ViewBag.Statuses = context.Statuses.ToList();
            //var task = new ToDo { StatusId = "open" };  // set default value for drop-down
            //return View(task);

            var model = new tOdOvIEWmODEL
            {
                Categories = context.Categories.ToList(),
                Statuses = context.Statuses.ToList(),
                DueFilters = Filters.DueFilterValues,
                CurrentTask = new ToDo { StatusId = "open" }  
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Add(tOdOvIEWmODEL model)
        {
            if (ModelState.IsValid)
            {
                //context.ToDos.Add(task);
                //context.SaveChanges();
                //return RedirectToAction("Index");

                context.ToDos.Add(model.CurrentTask);
                context.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                //ViewBag.Categories = context.Categories.ToList();
                //ViewBag.Statuses = context.Statuses.ToList();
                //return View(task);

                model.Categories = context.Categories.ToList();
                model.Statuses = context.Statuses.ToList();
                model.DueFilters = Filters.DueFilterValues;

                return View(model);
            }
        }

        [HttpPost]
        public IActionResult Filter(string[] filter)
        {
            string id = string.Join('-', filter);
            return RedirectToAction("Index", new { ID = id });
        }

        [HttpPost]
        public IActionResult MarkComplete([FromRoute] string id, ToDo selected)
        {
            selected = context.ToDos.Find(selected.Id)!;  
            if (selected != null)
            {
                selected.StatusId = "closed";
                context.SaveChanges();
            }

            return RedirectToAction("Index", new { ID = id });
        }

        [HttpPost]
        public IActionResult DeleteComplete(string id)
        {
            var toDelete = context.ToDos
                .Where(t => t.StatusId == "closed").ToList();

            foreach (var task in toDelete)
            {
                context.ToDos.Remove(task);
            }
            context.SaveChanges();

            return RedirectToAction("Index", new { ID = id });
        }
    }
}