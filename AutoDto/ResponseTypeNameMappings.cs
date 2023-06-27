using AutoDtoConfig.Attributes;
using System.Collections.Generic;

namespace AutoDto
{
    public partial class AutoDtoSourceGenerator
    {
        internal class ResponseTypeNameMappings
        {
            public GeneratedResponseType ResponseType { get; set; }
            public Dictionary<string, string> MappedTypeNames { get; set; } = new Dictionary<string, string>();
        }
    }
}