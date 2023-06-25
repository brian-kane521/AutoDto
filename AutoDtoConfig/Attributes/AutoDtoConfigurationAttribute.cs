using AutoDtoConfig.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoDtoConfig.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoDtoConfigurationAttribute : Attribute
    {
        public ClassDiscoveryBehavior ClassDiscoveryBehavior { get; set; }
        public Type DtoNameGeneratorType { get; set; }
        public Type RequestTypeDefaultMemberBehaviorResolver { get; set; }
        public GeneratedRequestType RequestTypes { get; set; }
        public GeneratedResponseType ResponseTypes { get; set; }

        public AutoDtoConfigurationAttribute()
        {
            ClassDiscoveryBehavior = ClassDiscoveryBehavior.Default;
            DtoNameGeneratorType = typeof(DefaultDtoNameGenerator);
            RequestTypeDefaultMemberBehaviorResolver = typeof(DefaultRequestTypeDefaultBehaviorResolver);
            RequestTypes = GeneratedRequestType.Default;
            ResponseTypes = GeneratedResponseType.Default;
        }
    }
}
