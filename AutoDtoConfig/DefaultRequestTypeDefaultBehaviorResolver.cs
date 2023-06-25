using AutoDtoConfig.Attributes;
using AutoDtoConfig.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoDtoConfig
{
    public class DefaultRequestTypeDefaultBehaviorResolver : IRequestTypeDefaultMemberBehaviorResolver
    {
        public virtual DefaultMemberBehavior GetDefaultBehaviorForRequestType(GeneratedRequestType requestType)
        {
            return DefaultMemberBehavior.IncludeAll;
        }

        public virtual DefaultMemberBehavior GetDefaultBehaviorForResponseType(GeneratedResponseType responseType)
        {
            return DefaultMemberBehavior.IncludeAll;
        }
    }
}
