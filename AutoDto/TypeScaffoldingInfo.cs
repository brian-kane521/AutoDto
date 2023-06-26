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
        public AutoDtoIgnoreAttribute? IgnoreAttribute { get; set; }
        public AutoDtoIncludeAttribute? IncludeAttribute { get; set; }
        public List<PropertyScaffoldingInfo> Properties { get; set; } = new List<PropertyScaffoldingInfo>();
    }

    internal class ScaffoldedMember
    {
        public string BaseTypeDeclaration { get; set; } = null!;
        public string TypeDeclarationSuffix { get; set; } = string.Empty;
        public string BaseName { get; set; } = null!;
        public string ValueInitializer { get; set; } = string.Empty;
    }
}
