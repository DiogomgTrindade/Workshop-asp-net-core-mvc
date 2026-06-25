using Microsoft.AspNetCore.Mvc;
using SalesWebMvc.Data;
using SalesWebMvc.Models;
using SalesWebMvc.Models.Dto;
using SalesWebMvc.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SalesWebMvc.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class SellersController : ControllerBase
    {
        private readonly SalesWebMvcContext _context;
        private readonly SellerService _sellerService;

        public SellersController(SalesWebMvcContext context, SellerService sellerService)
        {
            _context = context;
            _sellerService = sellerService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Seller>>> Sellers()
        {
            return await _sellerService.FindAllAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<SellerResponseDto>> GetSellerById(int id)
        {
            if (id == null)
                return NotFound();

            var seller = await _sellerService.FindByIdAsync(id);

            if (seller == null)
                return NotFound();

            var response = new SellerResponseDto
            {
                Id = seller.Id,
                Name = seller.Name,
                Email = seller.Email,
                BirthDate = seller.BirthDate,
                BaseSalary = seller.BaseSalary,
                DepartmentId = seller.DepartmentId,
                DepartmentName = seller.Department.Name
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<Seller>> CreateSeller(CreateSellerDto createSellerDto)
        {
            var seller = new Seller
            {
                Name = createSellerDto.Name,
                Email = createSellerDto.Email,
                BirthDate = createSellerDto.BirthDate,
                BaseSalary = createSellerDto.BaseSalary,
                DepartmentId = createSellerDto.DepartmentId,
            };

            await _sellerService.InsertAsync(seller);

            return CreatedAtAction(nameof(GetSellerById),
                new { id = seller.Id },
                seller
            );
        }

    }
}
