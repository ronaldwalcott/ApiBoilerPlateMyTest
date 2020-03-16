using ApiBoilerPlateMyTest.Data.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiBoilerPlateMyTest.Data
{
    public class TestDBContext : DbContext
    {
        public TestDBContext(DbContextOptions<TestDBContext> options)
                : base(options)
        {
        }

        public DbSet<Person> Persons { get; set; }
    }

}
