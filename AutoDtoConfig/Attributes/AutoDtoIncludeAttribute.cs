using System;
using System.Collections.Generic;
using System.Text;

namespace AutoDtoConfig.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class AutoDtoIncludeAttribute : Attribute
    {
        public GeneratedRequestType RequestTypesWherePropertyIsIncluded { get; set; } = GeneratedRequestType.All;
        public GeneratedResponseType ResponseTypesWherePropertyIsIncluded { get; set; } = GeneratedResponseType.All;
    }
}
