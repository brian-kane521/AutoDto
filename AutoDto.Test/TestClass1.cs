using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDto.Test
{
    internal class TestClass1
    {
        public int TestInt { get; set; }
        public bool TestBool { get; set; }
        public string TestString { get; set; } = null!;
        public float TestFloat { get; set; }
        public double TestDouble { get; set; }
        public decimal TestDecimal { get; set; }
        public virtual TestChildClass1 TestChildClass1 { get; set; } = null!;
        public virtual ICollection<TestChildClass1> TestChildClass1Collection { get; set; } = new List<TestChildClass1>();
    }
}
