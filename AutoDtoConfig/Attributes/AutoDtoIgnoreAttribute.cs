using System;
using System.Collections.Generic;
using System.Text;

namespace AutoDtoConfig.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class AutoDtoIgnoreAttribute : Attribute
    {
        public GeneratedRequestType RequestTypesWherePropertyIsIgnored { get; set; } = GeneratedRequestType.All;
        public GeneratedResponseType ResponseTypesWherePropertyIsIgnored { get; set; } = GeneratedResponseType.All;
    }
}
