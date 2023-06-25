namespace AutoDto.Attributes
{
    public enum ResponseTypes
    {
        None = 0,
        Create = 1,
        Update = 2,
        Delete = 4,
        All = Create | Update | Delete,
        Generic = 8,
        Default = Generic,
    }
}
