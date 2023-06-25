namespace AutoDtoConfig.Attributes
{
    public enum MappingGenerationBehavior
    {
        None = 0,
        GenerateOneWayFunctions = 1,
        GenerateTwoWayFunctions = 2,
        Default = GenerateTwoWayFunctions,
    }
}
