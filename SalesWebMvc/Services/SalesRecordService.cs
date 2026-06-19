using Microsoft.EntityFrameworkCore;
using SalesWebMvc.Data;
using SalesWebMvc.Models;
using SalesWebMvc.Models.Enums;
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
                .Include(x => x.Items)
                    .ThenInclude(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task AddItems(SalesRecord salesRecord, List<ItemCart> items, int? salesRecordId)
        {
            if (salesRecordId != null)
            {
                foreach (var item in items)
                {
                    item.SalesRecordId = salesRecordId.Value;
                }
            }

            salesRecord.Items = items;
            await _context.ItemCart.AddRangeAsync(items);

            await _context.SaveChangesAsync();
        }

        public async Task ReplaceItemsAsync(SalesRecord salesRecord, int[] itemIds, int[] quantity)
        {
            for (int i = 0; i < itemIds.Length && i < quantity.Length; i++)
            {

                ItemCart item = new ItemCart
                {
                    SalesRecordId = salesRecord.Id,
                    ItemId = itemIds[i],
                    Item = await _context.Item.FirstOrDefaultAsync(x => x.Id == itemIds[i]),
                    Quantity = quantity[i]
                };
                salesRecord.Items.Add(item);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateSaleWithItemsAsync(int salesRecordId, DateTime date,SaleStatus status,
            int sellerId,int[] itemIds, int[] quantity)
        {
            var existingSale = await _context.SalesRecord
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == salesRecordId);

            if (existingSale == null)
            {
                throw new Exception("Sale record not found.");
            }

            var seller = await _context.Seller.FirstOrDefaultAsync(x => x.Id == sellerId);

            if (seller == null)
            {
                throw new Exception("Seller not found.");
            }

            existingSale.Date = date;
            existingSale.Status = status;
            existingSale.Seller = seller;

            _context.ItemCart.RemoveRange(existingSale.Items);

            existingSale.Items = new List<ItemCart>();

            double total = 0;

            for (int i = 0; i < itemIds.Length && i < quantity.Length; i++)
            {
                if (itemIds[i] <= 0 || quantity[i] <= 0)
                {
                    continue;
                }

                var item = await _context.Item.FirstOrDefaultAsync(x => x.Id == itemIds[i]);

                if (item == null)
                {
                    continue;
                }

                var itemCart = new ItemCart
                {
                    SalesRecordId = existingSale.Id,
                    ItemId = item.Id,
                    Quantity = quantity[i]
                };

                existingSale.Items.Add(itemCart);

                total += item.Price * quantity[i];
            }

            existingSale.Amount = total;

            await _context.SaveChangesAsync();
        }

    }
}
