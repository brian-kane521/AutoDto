using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDto.Test.Models
{
    internal class TestChildClass1
    {
        public int TestInt { get; set; }
        public bool TestBool { get; set; }
        public string TestString { get; set; } = null!;
        public float TestFloat { get; set; }
        public double TestDouble { get; set; }
        public decimal TestDecimal { get; set; }
    }
}
