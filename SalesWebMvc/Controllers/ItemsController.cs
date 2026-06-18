using Microsoft.AspNetCore.Mvc;
using SalesWebMvc.Models;
using SalesWebMvc.Models.ViewModels;
using SalesWebMvc.Services;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SalesWebMvc.Controllers
{
    public class ItemsController : Controller
    {
        private readonly ItemService _itemService;
        private readonly DepartmentService _departmentService;

        public ItemsController(ItemService itemService, DepartmentService departmentService)
        {
            _itemService = itemService;
            _departmentService = departmentService;
        }

        public async Task<IActionResult> Index(int departmentId)
        {
            if (departmentId == null)
                return RedirectToAction(nameof(Error), "Department Id not found");

            var department = await _departmentService.FindByIdAsync(departmentId);
            if (department == null)
                return RedirectToAction(nameof(Error), "Department is null");


            ViewData["departmentName"] = department.Name;

            var itemList = await _itemService.FindAllByDepartmentId(departmentId);
            ViewData["departmentId"] = departmentId;
            
            return View(itemList);
        }

        public IActionResult Create(int? departmentId)
        {
            if (departmentId == null)
                return RedirectToAction(nameof(Error), "Department Id not found");

            ViewData["departmentId"] = departmentId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Item item)
        {
            if (!ModelState.IsValid)
            {
                return View(item);
            }

            if (await _itemService.NameExists(item.Name))
            {
                ModelState.AddModelError("Name", "Product with this name already exists");
                return View(item);
            }

            await _itemService.Insert(item);

            return RedirectToAction("Index", new { departmentId = item.DepartmentId });
        }

        public IActionResult Error(string message)
        {
            var viewModel = new ErrorViewModel
            {
                Message = message,
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };

            return View(viewModel);
        }
    }
}
