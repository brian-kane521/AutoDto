using AutoDtoConfig.Attributes;
using System.Collections.Generic;

namespace AutoDto
{
    public partial class AutoDtoSourceGenerator
    {
        internal class RequestTypeNameMappings
        {
            public GeneratedRequestType RequestType { get; set; }
            public Dictionary<string, string> MappedTypeNames { get; set; } = new Dictionary<string, string>();
        }
    }
}