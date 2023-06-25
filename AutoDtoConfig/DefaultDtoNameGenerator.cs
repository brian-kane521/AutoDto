using AutoDtoConfig.Attributes;
using AutoDtoConfig.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoDtoConfig
{
    public class DefaultDtoNameGenerator : IDtoNameGenerator
    {
        public virtual string GenerateRequestName(string baseClassName, GeneratedRequestType requestType)
        {
            return $"{requestType}{baseClassName}Request";
        }

        public virtual string GenerateResponseName(string baseClassName, GeneratedResponseType responseType)
        {
            if (responseType.Equals(GeneratedResponseType.Generic))
            {
                return $"{baseClassName}Response";
            }
            else
            {
                return $"{responseType}{baseClassName}Response";
            }
        }
    }
}
