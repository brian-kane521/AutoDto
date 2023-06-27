using System;
using System.Collections.Generic;
using System.Text;

namespace AutoDtoConfig.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoDtoConfigurationAttribute : Attribute
    {
        public ClassDiscoveryBehavior ClassDiscoveryBehavior { get; set; }
        public string RequestDtoNamingTemplate { get; set; }
        public string ResponseDtoNamingTemplate { get; set; }
        public GeneratedRequestType GenerateRequestTypes { get; set; }
        public GeneratedResponseType GenerateResponseTypes { get; set; }
        public GeneratedRequestType RequestTypesIncludingAllPropertiesByDefault { get; set; }
        public GeneratedResponseType ResponseTypesIncludingAllPropertiesByDefault { get; set; }

        public AutoDtoConfigurationAttribute()
        {
            ClassDiscoveryBehavior = ClassDiscoveryBehavior.Default;
            RequestDtoNamingTemplate = "{RequestType}{BaseClassName}Request";
            ResponseDtoNamingTemplate = "{ResponseType}{BaseClassName}Response";
            GenerateRequestTypes = GeneratedRequestType.Default;
            GenerateResponseTypes = GeneratedResponseType.Default;
            RequestTypesIncludingAllPropertiesByDefault = GeneratedRequestType.All;
            ResponseTypesIncludingAllPropertiesByDefault = GeneratedResponseType.All;
        }
    }
}
