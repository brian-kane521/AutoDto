using AutoDtoConfig.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoDtoConfig.Interfaces
{
    internal interface IDtoNameGenerator
    {
        string GenerateRequestName(string baseClassName, GeneratedRequestType requestType);

        string GenerateResponseName(string baseClassName, GeneratedResponseType responseType);
    }
}
