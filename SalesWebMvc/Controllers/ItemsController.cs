using Microsoft.AspNetCore.Mvc;
using SalesWebMvc.Services;
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
                return NotFound();

            var department = await _departmentService.FindByIdAsync(departmentId);
            if (department == null)
                return NotFound();

            ViewData["departmentName"] = department.Name;

            var itemList = await _itemService.FindAllByDepartmentId(departmentId);
            ViewData["departmentId"] = departmentId;
            

            return View(itemList);
        }
    }
}
