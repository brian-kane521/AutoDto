namespace AutoDtoConfig.Attributes
{
    public enum ClassDiscoveryBehavior
    {
        IncludeAllDbSets,
        ExcludeAll,
        Default = IncludeAllDbSets,
    }
}
