using AutoDtoConfig.Attributes;
using Microsoft.CodeAnalysis;

namespace AutoDto
{
    internal class PropertyScaffoldingInfo
    {
        public IPropertySymbol BaseProperty { get; set; } = null!;
        public AutoDtoIgnoreAttribute? IgnoreAttribute { get; set; }
        public AutoDtoIncludeAttribute? IncludeAttribute { get; set; }
    }
}
