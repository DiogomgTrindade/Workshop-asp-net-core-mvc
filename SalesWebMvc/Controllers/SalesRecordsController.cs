using Microsoft.AspNetCore.Mvc;
using SalesWebMvc.Models;
using SalesWebMvc.Models.ViewModels;
using SalesWebMvc.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SalesWebMvc.Controllers
{
    public class SalesRecordsController : Controller
    {
        private readonly SalesRecordService _salesRecordService;
        private readonly SellerService _sellerService;

        public SalesRecordsController(SalesRecordService salesRecordService, SellerService sellerService)
        {
            _salesRecordService = salesRecordService;
            _sellerService = sellerService;
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
                return RedirectToAction(nameof(Error), new { message = "Sales records not found" });

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

            var viewModel = new SellerSalesFormViewModel
            {
                SalesRecord = saleRecord,
                Seller = seller,
                SellerId = sellerId.Value
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SalesRecord salesRecord, int? sellerId)
        {
            if (!ModelState.IsValid)
            {
                return View(salesRecord);
            }

            if (salesRecord.Date > DateTime.UtcNow.Date)
            {
                ModelState.AddModelError("Date", "The date cannot be in the future");
                return View(salesRecord);
            }

            if (sellerId == null)
                return RedirectToAction(nameof(Error), "Seller id not found");

            salesRecord.Seller = await _sellerService.FindByIdAsync(sellerId.Value);
            await _salesRecordService.InsertAsync(salesRecord);
            return RedirectToAction(nameof(SellerSales), new {sellerId = sellerId});
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
