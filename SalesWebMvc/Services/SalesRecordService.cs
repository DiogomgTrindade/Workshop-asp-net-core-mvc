using Microsoft.EntityFrameworkCore;
using SalesWebMvc.Data;
using SalesWebMvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalesWebMvc.Services
{
    public class SalesRecordService
    {
        private readonly SalesWebMvcContext _context;

        public SalesRecordService(SalesWebMvcContext context)
        {
            _context = context;
        }

        public async Task<List<SalesRecord>> FindByDateAsync(DateTime? minDate, DateTime? maxDate)
        {
            var result = from obj in _context.SalesRecord select obj;
            if (minDate.HasValue)
            {
                result = result.Where(x => x.Date >= minDate.Value);
            }

            if (maxDate.HasValue)
            {
                result = result.Where(x => x.Date <= maxDate.Value);
            }

            return await result
                    .Include(x => x.Seller)
                    .Include(x => x.Seller.Department)
                    .OrderByDescending(x => x.Date)
                    .ToListAsync();
        }

        public async Task<List<IGrouping<Department, SalesRecord>>> FindByDateGroupingAsync(DateTime? minDate, DateTime? maxDate)
        {
            var result = from obj in _context.SalesRecord select obj;
            if (minDate.HasValue)
            {
                result = result.Where(x => x.Date >= minDate.Value);
            }

            if (maxDate.HasValue)
            {
                result = result.Where(x => x.Date <= maxDate.Value);
            }

            return await result
                    .Include(x => x.Seller)
                    .Include(x => x.Seller.Department)
                    .OrderByDescending (x => x.Date)
                    .GroupBy(x => x.Seller.Department)
                    .ToListAsync();
        }

        public async Task<List<SalesRecord>> FindAllBySellerIdAsync(int id)
        {
            return await _context.SalesRecord.Where(x => x.Seller.Id == id)
                .Include(x => x.Seller)
                .OrderBy(x => x.Status)
                .ThenBy(x => x.Date)
                .ToListAsync();
        }

        public async Task InsertAsync(SalesRecord obj)
        {
            _context.Add(obj);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SalesRecord obj)
        {
            _context.Update(obj);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(SalesRecord obj)
        {
            _context.Remove(obj);

            await _context.SaveChangesAsync();
        }

        public async Task<SalesRecord> FindBySellerIdAsync(int id)
        {
            return await _context.SalesRecord.Where(x => x.Seller.Id == id)
                .Include(x => x.Seller)
                .FirstOrDefaultAsync();
        }

        public async Task<SalesRecord> FindByIdAsync(int id)
        {
            return await _context.SalesRecord.Where(x => x.Id == id)
                .Include(x => x.Seller)
                .Include(x => x.Seller.Department)
                .FirstOrDefaultAsync();
        }

    }
}
