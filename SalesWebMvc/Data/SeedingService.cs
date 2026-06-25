using SalesWebMvc.Models;
using SalesWebMvc.Models.Enums;
using System;
using System.Linq;

namespace SalesWebMvc.Data
{
    public class SeedingService
    {
        private readonly SalesWebMvcContext _context;

        public SeedingService(SalesWebMvcContext context)
        {
            _context = context;
        }

        public void Seed()
        {
            if (_context.Department.Any() || 
                _context.Seller.Any() || 
                _context.SalesRecord.Any())
            {
                return;
            }

            Department d1 = new Department("Computers");
            Department d2 = new Department("Electronics");
            Department d3 = new Department("Fashion");
            Department d4 = new Department("Books");

            Seller s1 = new Seller("Bob Brown", "bob@gmail.com", new DateTime(1998, 4, 21), 1000.0, d1);
            Seller s2 = new Seller("Maria Green", "maria@gmail.com", new DateTime(1979, 12, 31), 3500.0, d2);
            Seller s3 = new Seller("Alex Grey", "alex@gmail.com", new DateTime(1988, 1, 15), 2200.0, d1);
            Seller s4 = new Seller("Martha Red", "martha@gmail.com", new DateTime(1993, 11, 30), 3000.0, d4);
            Seller s5 = new Seller("Donald Blue", "donald@gmail.com", new DateTime(2000, 1, 9), 4000.0, d3);
            Seller s6 = new Seller("Alex Pink", "bob@gmail.com", new DateTime(1997, 3, 4), 3000.0, d2);

            _context.Department.AddRange(d1, d2, d3, d4);
            
            _context.Seller.AddRange(s1, s2, s3, s4, s5, s6);

            _context.SaveChanges();
        }
    }
}
