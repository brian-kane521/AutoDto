using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDto.Test.Models
{
    internal class TestClass2
    {
        public int[] TestIntArray { get; set; } = new int[0];

        public string[] TestStringArray { get; set; } = new string[0];

        public int? TestNullableInt { get; set; }
        public string? TestNullableString { get; set; }
    }
}
