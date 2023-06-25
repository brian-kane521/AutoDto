using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDto.Test
{
    internal class TestDbContext : DbContext
    {


        public virtual DbSet<TestClass1> TestClass1 { get; set; }

        public virtual DbSet<TestChildClass1> TestChildClass1 { get; set; }

        public virtual DbSet<TestClass2> TestClass2 { get; set; }
    }
}
