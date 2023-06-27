using AutoDtoConfig.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDto.Test.Models
{
    [AutoDtoConfiguration(RequestTypesIncludingAllPropertiesByDefault=GeneratedRequestType.All)]
    internal class TestClass2
    {
        public int? TestNullableInt { get; set; }
        public string? TestNullableString { get; set; }
    }
}
