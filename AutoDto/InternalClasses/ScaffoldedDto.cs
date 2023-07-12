using AutoDtoConfig.Attributes;
using System.Collections.Generic;

namespace AutoDto
{
    public partial class AutoDtoSourceGenerator
    {
        internal class ScaffoldedDto
        {
            public bool IsRequestType { get; set; }
            public GeneratedRequestType? RequestType { get; set; }
            public GeneratedResponseType ResponseType { get; set; }
            public string BaseClassName { get; set; } = null!;
            public string ClassName { get; set; } = null!;
            public string ClassFileName { get; set; } = null!;
            public List<ScaffoldedMember> Properties { get; set; } = new List<ScaffoldedMember>();
        }
    }
}