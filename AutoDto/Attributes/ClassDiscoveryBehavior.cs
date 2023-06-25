namespace AutoDto.Attributes
{
    public enum ClassDiscoveryBehavior
    {
        IncludeAllDbSets,
        ExcludeAll,
        Default = IncludeAllDbSets,
    }
}
