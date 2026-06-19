using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;
using SalesWebMvc.Models;
using SalesWebMvc.Models.ViewModels;
using SalesWebMvc.Services;
using SalesWebMvc.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SalesWebMvc.Controllers
{
    public class SalesRecordsController : Controller
    {
        private readonly SalesRecordService _salesRecordService;
        private readonly SellerService _sellerService;
        private readonly ItemService _itemService;
        private readonly DepartmentService _departmentService;

        public SalesRecordsController(SalesRecordService salesRecordService, SellerService sellerService, ItemService itemService, DepartmentService departmentService)
        {
            _salesRecordService = salesRecordService;
            _sellerService = sellerService;
            _itemService = itemService;
            _departmentService = departmentService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> SimpleSearch(DateTime? minDate, DateTime? maxDate)
        {
            if (!minDate.HasValue)
            {
                minDate = new DateTime(DateTime.Now.Year, 1, 1);
            }

            if (!maxDate.HasValue)
            {
                maxDate = DateTime.Now;
            }

            ViewData["minDate"] = minDate.Value.ToString("yyyy-MM-dd");
            ViewData["maxDate"] = maxDate.Value.ToString("yyyy-MM-dd");

            var result = await _salesRecordService.FindByDateAsync(minDate, maxDate);
            return View(result);
        }

        public async Task<IActionResult> GroupingSearch(DateTime? minDate, DateTime? maxDate)
        {
            if (!minDate.HasValue)
            {
                minDate = new DateTime(DateTime.Now.Year, 1, 1);
            }

            if (!maxDate.HasValue)
            {
                maxDate = DateTime.Now;
            }

            ViewData["minDate"] = minDate.Value.ToString("yyyy-MM-dd");
            ViewData["maxDate"] = maxDate.Value.ToString("yyyy-MM-dd");

            var result = await _salesRecordService.FindByDateGroupingAsync(minDate, maxDate);
            return View(result);
        }

        public async Task<IActionResult> SellerSales(int? sellerId)
        {
            if (sellerId == null)
                return RedirectToAction(nameof(Error), new { message = "Id not found" });

            var salesRecords = await _salesRecordService.FindAllBySellerIdAsync(sellerId.Value);
            if (salesRecords.Count == 0 || !salesRecords.Any())
                salesRecords = new List<SalesRecord>();

            var seller = await _sellerService.FindByIdAsync(sellerId.Value);
            if (seller == null)
                return RedirectToAction(nameof(Error), new { message = "Seller id not found" });

            var viewModel = new SellerSalesViewModel
            {
                SalesRecords = salesRecords,
                Seller = seller,
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Create(int? sellerId)
        {
            if (sellerId == null)
                return RedirectToAction(nameof(Error), new { message = "Id not found" });

            var seller = await _sellerService.FindByIdAsync(sellerId.Value);
            if (seller == null)
                return RedirectToAction(nameof(Error), new { message = "Seller not exists" });

            var saleRecord = new SalesRecord
            {
                Date = DateTime.UtcNow,
                Seller = seller
            };

            var itemList = await _itemService.GetAllItemsByDepartment(seller.DepartmentId);
            if (itemList == null)
                return RedirectToAction(nameof(Error), new { message = "No items availables" });

            var viewModel = new SellerSalesFormViewModel
            {
                SalesRecord = saleRecord,
                Seller = seller,
                SellerId = sellerId.Value,
                Sellers = null,
                Items = itemList,
                Quantity = null
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SalesRecord salesRecord, int? sellerId, int[] itemIds, int[] quantity)
        {
            if (sellerId == null)
                return RedirectToAction(nameof(Error), "Seller id not found");

            var seller = await _sellerService.FindByIdAsync(sellerId.Value);
            var department = await _departmentService.FindByIdAsync(seller.DepartmentId);
            var items = await _itemService.GetAllItemsByDepartment(department.Id);

            if (itemIds == null || quantity == null)
            {
                ModelState.AddModelError("", "Please fill the items and the quantity.");

                var viewModel = new SellerSalesFormViewModel
                {
                    SalesRecord = salesRecord,
                    Seller = seller,
                    SellerId = sellerId.Value,
                    Sellers = null,
                    Items = items,
                    ItemIds = itemIds,
                    Quantity = quantity
                };

                return View(viewModel);
            }

            if (itemIds.Length != quantity.Length)
            {
                ModelState.AddModelError("", "Please fill the items and the quantity.");

                var viewModel = new SellerSalesFormViewModel
                {
                    SalesRecord = salesRecord,
                    Seller = seller,
                    SellerId = sellerId.Value,
                    Sellers = null,
                    Items = items,
                    ItemIds = itemIds,
                    Quantity = quantity
                };

                return View(viewModel);
            }

            ModelState.Remove("SalesRecord.Amount");
            if (!ModelState.IsValid)
            {
                var viewModel = new SellerSalesFormViewModel
                {
                    SalesRecord = salesRecord,
                    Seller = seller,
                    SellerId = sellerId.Value,
                    Sellers = null,
                    Items = items,
                    ItemIds = itemIds,
                    Quantity = quantity
                };

                return View(viewModel);
            }

            if (salesRecord.Date > DateTime.UtcNow.Date)
            {
                ModelState.AddModelError("SalesRecord.Date", "The date cannot be in the future");
                var viewModel = new SellerSalesFormViewModel
                {
                    SalesRecord = salesRecord,
                    Seller = seller,
                    SellerId = sellerId.Value,
                    Sellers = null,
                    Items = items,
                    ItemIds = itemIds,
                    Quantity = quantity
                };
                return View(viewModel);
            }

            var itemCartList = await _itemService.CreateItemCartListAsync(itemIds, quantity);
            salesRecord.Amount = _itemService.SumTotalCart(itemCartList);
            salesRecord.Seller = seller;
            await _salesRecordService.InsertAsync(salesRecord);
            await _salesRecordService.AddItems(salesRecord, itemCartList, null);
            return RedirectToAction(nameof(SellerSales), new {sellerId = sellerId});
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return RedirectToAction(nameof(Error), "Id not found");

            var saleRecord = await _salesRecordService.FindByIdAsync(id.Value);
            if (saleRecord == null)
                return RedirectToAction(nameof(Error), "SaleRecord not found");

            var seller = await _sellerService.FindBySaleRecordId(saleRecord.Id);
            if (seller == null)
                return RedirectToAction(nameof(Error), "Seller not found");

            var department = await _departmentService.FindByIdAsync(seller.DepartmentId);
            if (department == null)
                return RedirectToAction(nameof(Error), "Department not found");

            var items = await _itemService.GetAllItemsByDepartment(department.Id);
            if (items == null)
                return RedirectToAction(nameof(Error), $"Item list for {department.Name} not found");

            int[] itemsId = new int[saleRecord.Items.Count];
            int[] quantity = new int[saleRecord.Items.Count];


            for (int i = 0; i < saleRecord.Items.Count; i++)
            {
                itemsId[i] = saleRecord.Items[i].ItemId;
                quantity[i] = saleRecord.Items[i].Quantity;
            }

            var viewModel = new SellerSalesFormViewModel
            {
                SalesRecord = saleRecord,
                Seller = seller,
                SellerId = seller.Id,
                Sellers = await _sellerService.FindAllByDepartmentAsync(department.Id),
                Items = items,
                ItemIds = itemsId,
                Quantity = quantity,
             };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SalesRecord salesRecord, int sellerId, int[] itemIds, int[] quantity)
        {
            var seller = await _sellerService.FindBySaleRecordId(salesRecord.Id);
            if (seller == null)
                return RedirectToAction(nameof(Error), "Seller id not found");

            var sellerById = await _sellerService.FindByIdAsync(sellerId);
            if (sellerById == null)
                return RedirectToAction(nameof(Error), "Seller id not found");

            if (sellerById.Id != seller.Id)
                salesRecord.Seller = sellerById;

            var department = await _departmentService.FindByIdAsync(seller.DepartmentId);
            if (department == null)
                return RedirectToAction(nameof(Error), "Department not found");

            var items = await _itemService.GetAllItemsByDepartment(department.Id);
            if (items == null)
                return RedirectToAction(nameof(Error), $"Item list for {department.Name} not found");

            if (itemIds == null || quantity == null)
            {
                ModelState.AddModelError("", "Please fill the items and the quantity.");

                var viewModel = new SellerSalesFormViewModel
                {
                    SalesRecord = salesRecord,
                    Seller = seller,
                    SellerId = sellerId,
                    Sellers = null,
                    Items = items,
                    ItemIds = itemIds,
                    Quantity = quantity
                };

                return View(viewModel);
            }

            if (itemIds.Length != quantity.Length)
            {
                ModelState.AddModelError("", "Please fill the items and the quantity.");

                var viewModel = new SellerSalesFormViewModel
                {
                    SalesRecord = salesRecord,
                    Seller = seller,
                    SellerId = sellerId,
                    Sellers = null,
                    Items = items,
                    ItemIds = itemIds,
                    Quantity = quantity
                };

                return View(viewModel);
            }

            ModelState.Remove("SalesRecord.Amount");
            ModelState.Remove("SalesRecord.Seller");
            ModelState.Remove("SalesRecord.Items");
            if (!ModelState.IsValid)
            {
                var viewModel = new SellerSalesFormViewModel
                {
                    SalesRecord = salesRecord,
                    Seller = seller,
                    SellerId = seller.Id,
                    Sellers = await _sellerService.FindAllByDepartmentAsync(department.Id)
                };

                return View(viewModel);
            }

            if (salesRecord.Date > DateTime.UtcNow.Date)
            {
                ModelState.AddModelError("SalesRecord.Date", "The date cannot be in the future");

                var viewModel = new SellerSalesFormViewModel
                {
                    SalesRecord = salesRecord,
                    Seller = seller,
                    SellerId = seller.Id,
                    Sellers = await _sellerService.FindAllAsync(),
                    Items = items,
                    ItemIds = itemIds,
                    Quantity = quantity
                };

                return View(viewModel);
            }

            await _salesRecordService.UpdateSaleWithItemsAsync(salesRecord.Id, salesRecord.Date, salesRecord.Status,
                sellerId, itemIds, quantity);

            return RedirectToAction(nameof(SellerSales), new { sellerId = sellerId });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return RedirectToAction(nameof(Error), "Id not found");

            var saleRecord = await _salesRecordService.FindByIdAsync(id.Value);
            if (saleRecord == null)
                return RedirectToAction(nameof(Error), "Record not found");

            return View(saleRecord);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (id == null)
                return RedirectToAction(nameof(Error), "Id not found");

            var saleRecord = await _salesRecordService.FindByIdAsync(id);
            if (saleRecord == null)
                return RedirectToAction(nameof(Error), "Record not found");

            int sellerId = saleRecord.Seller.Id;

            try
            {
                await _salesRecordService.DeleteAsync(saleRecord);
                return RedirectToAction(nameof(SellerSales), new { sellerId = sellerId });
            }
            catch (IntegrityException e)
            {
                return RedirectToAction(nameof(Error), new { Message = e.Message});
            }
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
