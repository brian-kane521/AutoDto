namespace AutoDto.Attributes
{
    public enum RequestTypes
    {
        None = 0,
        Create = 1,
        Update = 2,
        Delete = 4,
        All = Create | Update | Delete,
        Default = All,
    }
}
