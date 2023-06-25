using System;

namespace AutoDtoConfig.Attributes
{
    [Flags]
    public enum GeneratedResponseType
    {
        None = 0,
        Create = 1,
        Read = 2,
        Update = 4,
        Delete = 8,
        All = Create | Read | Update | Delete,
        Generic = 16,
        Default = Generic,
    }
}
