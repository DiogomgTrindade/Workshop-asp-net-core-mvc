using Microsoft.EntityFrameworkCore;
using SalesWebMvc.Data;
using SalesWebMvc.Models;
using SalesWebMvc.Services.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalesWebMvc.Services
{
    public class SellerService
    {
        private readonly SalesWebMvcContext _context;

        public SellerService(SalesWebMvcContext context)
        {
            _context = context;
        }

        public async Task<List<Seller>> FindAllAsync()
        {
            return await _context.Seller.ToListAsync();
        }

        public async Task<List<Seller>> FindAllByDepartmentAsync(int departmentId)
        {
            return await _context.Seller.Where(x => x.DepartmentId == departmentId).ToListAsync();
        }

        public async Task<Seller> FindByIdAsync(int id)
        {
            return await _context.Seller
                .Include(d => d.Department)
                .FirstOrDefaultAsync(obj => obj.Id == id);
        }

        public async Task<Seller> FindBySaleRecordId(int id)
        {
            return await _context.SalesRecord.Where(x => x.Id == id)
                .Select(x => x.Seller)
                .FirstOrDefaultAsync();
        }

        public async Task InsertAsync(Seller obj)
        {
            _context.Add(obj);

            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(int id)
        {

            try
            {
                var obj = await _context.Seller.FindAsync(id);
                _context.Remove(obj);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                throw new IntegrityException(e.Message);
            }
        }

        public async Task UpdateAsync(Seller obj)
        {
            bool hasAny = await _context.Seller.AnyAsync(x => x.Id == obj.Id);

            if (!hasAny)
            {
                throw new NotFoundException("Id not found");
            }

            try
            {
                _context.Seller.Update(obj);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException e)
            {
                throw new DbConcurrencyException(e.Message);
            }
        }
    }
}
