using Microsoft.EntityFrameworkCore;
using SalesWebMvc.Data;
using SalesWebMvc.Models;
using SalesWebMvc.Models.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalesWebMvc.Services
{
    public class ItemService
    {
        private readonly SalesWebMvcContext _context;

        public ItemService(SalesWebMvcContext context)
        {
            _context = context;
        }

        public async Task<List<Item>> FindAllByDepartmentId(int departmentId)
        {
            return await _context.Item.Where(x => x.DepartmentId == departmentId)
                .OrderBy(x => x.Status)
                .ThenBy(x => x.Name)
                .ThenBy(x => x.Price)
                .ToListAsync();
        }

        public async Task Insert(Item obj)
        {
            _context.Add(obj);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> NameExists(string name)
        {
            return await _context.Item.AnyAsync(x => x.Name == name);
        }

        public async Task<Item> FindByIdAsync(int id)
        {
            return await _context.Item.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task Update(Item obj)
        {
            _context.Update(obj);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> NameExistsForAnotherItem(string name, int id)
        {
            return await _context.Item.AnyAsync(x => x.Name == name && x.Id != id);
        }

        public async Task Remove(Item obj)
        {
            _context.Remove(obj);

            await _context.SaveChangesAsync();
        }

        public async Task<List<Item>> GetAllItemsByDepartment(int departmentId)
        {
            return await _context.Item.Where(x => x.DepartmentId == departmentId && x.Status == ItemStatus.Stock)
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Price)
                .ToListAsync();
        }

        public async Task<List<ItemCart>> CreateItemCartListAsync(int[] itemIds, int[] quantity)
        {
            List<ItemCart> list = new List<ItemCart>();

            for (int i = 0; i < itemIds.Length && i < quantity.Length; i++)
            {
                ItemCart itemCart = new ItemCart
                {
                    Item = await FindByIdAsync(itemIds[i]),
                    ItemId = itemIds[i],
                    Quantity = quantity[i],
                };
                list.Add(itemCart);
            }
            return list;
        }

        public double SumTotalCart(List<ItemCart> itemCarts)
        {
            return itemCarts.Sum(x => x.Quantity * x.Item.Price);
        }
    }
}
