using AutoDtoConfig.Attributes;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoDto
{
    internal class TypeScaffoldingInfo
    {
        public ITypeSymbol BaseType { get; set; } = null!;
        public AutoDtoConfigurationAttribute? ClassConfiguration { get; set; }
        public AutoDtoIgnoreAttribute? IgnoreAttribute { get; set; }
        public AutoDtoIncludeAttribute? IncludeAttribute { get; set; }
        public List<PropertyScaffoldingInfo> Properties { get; set; } = new List<PropertyScaffoldingInfo>();
    }
}
