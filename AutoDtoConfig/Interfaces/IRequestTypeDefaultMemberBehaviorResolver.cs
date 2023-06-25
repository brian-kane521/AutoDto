using AutoDtoConfig.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoDtoConfig.Interfaces
{
    internal interface IRequestTypeDefaultMemberBehaviorResolver
    {
        DefaultMemberBehavior GetDefaultBehaviorForRequestType(GeneratedRequestType requestType);
        DefaultMemberBehavior GetDefaultBehaviorForResponseType(GeneratedResponseType responseType);
    }
}
