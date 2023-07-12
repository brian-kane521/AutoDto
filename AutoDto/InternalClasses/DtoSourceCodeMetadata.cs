using System.Collections.Generic;

namespace AutoDto
{
    public partial class AutoDtoSourceGenerator
    {
        internal class DtoSourceCodeMetadata
        {
            public ScaffoldedDto ScaffoldedDto { get; set; } = null!;
            public List<RequestTypeNameMappings> RequestTypeNameMappings { get; set; } = new List<RequestTypeNameMappings>();
            public List<ResponseTypeNameMappings> ResponseTypeNameMappings { get; set; } = new List<ResponseTypeNameMappings>();
        }
    }
}