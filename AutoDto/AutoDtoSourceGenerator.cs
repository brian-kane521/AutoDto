using AutoDtoConfig.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AutoDto
{
    [Generator]
    public class AutoDtoSourceGenerator : ISourceGenerator
    {
        private AutoDtoConfigurationAttribute configuration;
        public AutoDtoSourceGenerator(AutoDtoConfigurationAttribute configuration)
        {
            this.configuration = configuration;
        }

        public AutoDtoSourceGenerator()
        {
            configuration = new AutoDtoConfigurationAttribute();
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // Code generation goes here
            var globalNamespace = context.Compilation.GlobalNamespace;
            if (globalNamespace == null)
            {
                Console.Error.WriteLine("Found null value for global namespace");
                return;
            }
            var dbContextType = RecurseNamespaceForTypeWithBaseTypeName(globalNamespace, "DbContext");
            if (dbContextType == null)
            {
                Console.Error.WriteLine("Missing type DbContext for DTO generation");
                return;
            }
            var userConfig = GetAttributeInstanceFromSymbol<AutoDtoConfigurationAttribute>(dbContextType);
            if (userConfig != null)
            {
                configuration = userConfig;
            }

            var typesToScaffold = GetTypesToScaffoldFromDbContext(dbContextType);

            //var dtos = ScaffoldDtos(typesToScaffold);
            //foreach (var dto in dtos)
            //{
            //    context.AddSource(dto.ClassFileName, dto.SourceCode);
            //}
        }

        internal T? GetAttributeInstanceFromSymbol<T>(ISymbol namedType)
            where T : new()
        {
            var typeAttributes = namedType.GetAttributes();
            var attributeTypeName = typeof(T).Name;
            foreach (var dbContextAttribute in typeAttributes)
            {
                if (dbContextAttribute.AttributeClass != null
                    && dbContextAttribute.AttributeClass.Name.Equals(attributeTypeName))
                {
                    return ConstructObjectFromNamedArguments<T>(dbContextAttribute.NamedArguments);
                }
            }
            return default(T);
        }

        internal T ConstructObjectFromNamedArguments<T>(ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments)
            where T : new()
        {
            T constructedObject = new T();
            foreach (var arg in namedArguments)
            {
                var propertyInfo = constructedObject.GetType().GetProperty(arg.Key);
                try
                {
                    if (arg.Value.Type != null && arg.Value.Type.Name.Equals("Type"))
                    {
                        if (arg.Value.Value != null)
                        {
                            propertyInfo.SetValue(constructedObject, arg.Value.Value.GetType());
                        }
                    }
                    else
                    {
                        propertyInfo.SetValue(constructedObject, arg.Value.Value);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Unable to set configuration property from attribute: " + ex.Message);
                }
            }
            return constructedObject;
        }

        internal List<TypeScaffoldingInfo> GetTypesToScaffoldFromDbContext(INamedTypeSymbol dbContextType)
        {
            var dbContextMembers = dbContextType.GetMembers();
            List<TypeScaffoldingInfo> typesToScaffold = new List<TypeScaffoldingInfo>();
            foreach (var dbMember in dbContextMembers)
            {
                if (dbMember.Kind.Equals(SymbolKind.Property))
                {
                    var typeSymbol = ((IPropertySymbol)dbMember).Type;
                    var typeName = typeSymbol.Name;
                    if (typeName.Equals("DbSet") && typeSymbol is INamedTypeSymbol)
                    {
                        var typeArguments = ((INamedTypeSymbol)typeSymbol).TypeArguments;
                        var baseClass = typeArguments.FirstOrDefault();
                        if (baseClass == null)
                        {
                            continue;
                        }
                        var includeAttribute = GetAttributeInstanceFromSymbol<AutoDtoIncludeAttribute>(baseClass);
                        var ignoreAttribute = GetAttributeInstanceFromSymbol<AutoDtoIgnoreAttribute>(baseClass);
                        if (configuration.ClassDiscoveryBehavior.Equals(ClassDiscoveryBehavior.ExcludeAll))
                        {
                            if (includeAttribute == null)
                            {
                                continue;
                            }
                        }
                        if (ignoreAttribute != null)
                        {
                            if (ignoreAttribute.ResponseTypesWherePropertyIsIgnored.Equals(GeneratedResponseType.All)
                                && ignoreAttribute.RequestTypesWherePropertyIsIgnored.Equals(GeneratedResponseType.All))
                            {
                                continue;
                            }
                        }
                        var properties = GetPropertiesToScaffold(typeSymbol);
                        typesToScaffold.Add(new TypeScaffoldingInfo
                        {
                            BaseType = baseClass!,
                            IgnoreAttribute = ignoreAttribute,
                            IncludeAttribute = includeAttribute,
                            Properties = properties,
                        });
                    }
                }
            }
            return typesToScaffold;
        }

        public class ScaffoldedDto
        {
            public string ClassFileName { get; set; } = null!;
            public string SourceCode { get; set; } = null!;
        }

        internal List<PropertyScaffoldingInfo> GetPropertiesToScaffold(ITypeSymbol baseType)
        {
            var properties = new List<PropertyScaffoldingInfo>();
            foreach (var member in baseType.GetMembers())
            {
                if (member.Kind.Equals(SymbolKind.Property))
                {
                    var memberProperty = (IPropertySymbol)member;
                    var includeAttribute = GetAttributeInstanceFromSymbol<AutoDtoIncludeAttribute>(memberProperty);
                    var ignoreAttribute = GetAttributeInstanceFromSymbol<AutoDtoIgnoreAttribute>(memberProperty);
                    if (ignoreAttribute != null 
                        && ignoreAttribute.RequestTypesWherePropertyIsIgnored.Equals(GeneratedRequestType.All)
                        && ignoreAttribute.ResponseTypesWherePropertyIsIgnored.Equals(GeneratedRequestType.All))
                    {
                        continue;
                    }
                    properties.Add(new PropertyScaffoldingInfo
                    {
                        BaseProperty = memberProperty,
                        IncludeAttribute = includeAttribute,
                        IgnoreAttribute = ignoreAttribute,
                    });
                }
            }
            return properties;
        }

//        public List<ScaffoldedDto> ScaffoldDtos(List<ITypeSymbol> typesToScaffold)
//        {
//            var dtos = new List<ScaffoldedDto>();
//            foreach (var scaffoldType in typesToScaffold)
//            {
//                var sourceBuilder = new StringBuilder($@"// <auto-generated />
//#nullable enable

//using System;
//using System.Collections.Generic;

//namespace AutoDto
//{{
//{"\t"}public partial class {scaffoldType.Name}Dto
//{"\t"}{{
//");
//                foreach (var memberProperty in GetPropertiesToScaffold(scaffoldType))
//                {
//                    if (memberProperty.Type.IsValueType)
//                    {
//                        var memberLine = $"\t\tpublic {memberProperty.Type} {memberProperty.Name} {{ get; set; }}";
//                        sourceBuilder.AppendLine(memberLine);
//                    }
//                    else
//                    {
//                        switch (memberProperty.Type.TypeKind)
//                        {
//                            case TypeKind.Class:
//                                string nullableAnnotation = string.Empty;
//                                string valueInitializer = " = null!;";
//                                string typeName;
//                                if (typesToScaffold.Any(type => type.Name.Equals(memberProperty.Type.Name)))
//                                {
//                                    typeName = $"{memberProperty.Type.Name}Dto";
//                                }
//                                else
//                                {
//                                    typeName = memberProperty.Type.OriginalDefinition.ToString();
//                                }
//                                if (memberProperty.Type.NullableAnnotation.Equals(NullableAnnotation.Annotated))
//                                {
//                                    nullableAnnotation = "?";
//                                    valueInitializer = "";
//                                }
//                                sourceBuilder.AppendLine($"\t\tpublic {typeName}{nullableAnnotation} {memberProperty.Name} {{ get; set; }}{valueInitializer}");
//                                break;
//                            case TypeKind.Array:
//                                var arrayType = (IArrayTypeSymbol)memberProperty.Type;
//                                var elementType = arrayType.ElementType;
//                                if (elementType.Kind.Equals(TypeKind.Class) && typesToScaffold.Any(type => type.Name.Equals(elementType.Name)))
//                                {
//                                    sourceBuilder.AppendLine($"\t\tpublic {elementType.Name}Dto[] {memberProperty.Name} {{ get; set; }} = new {elementType.Name}Dto[0];");
//                                }
//                                else
//                                {
//                                    sourceBuilder.AppendLine($"\t\tpublic {memberProperty.Type.OriginalDefinition} {memberProperty.Name} {{ get; set; }} = new {elementType.OriginalDefinition}[0];");
//                                }
//                                break;
//                            case TypeKind.Interface:
//                                if (memberProperty.Type is INamedTypeSymbol && memberProperty.Type.Name.Equals("ICollection"))
//                                {
//                                    var typeArguments = ((INamedTypeSymbol)memberProperty.Type).TypeArguments;
//                                    var baseClass = typeArguments.FirstOrDefault();
//                                    if (baseClass != null && typesToScaffold.Any(type => type.Name.Equals(baseClass.Name)))
//                                    {
//                                        sourceBuilder.AppendLine($"\t\tpublic ICollection<{baseClass.Name}Dto> {memberProperty.Name} {{ get; set; }} = new List<{baseClass.Name}Dto>();");
//                                    }
//                                    else
//                                    {
//                                        sourceBuilder.AppendLine($"\t\tpublic {memberProperty.Type.OriginalDefinition} {memberProperty.Name} {{ get; set; }} = new {memberProperty.Type.OriginalDefinition}();");
//                                    }
//                                }
//                                break;
//                        }
//                    }
//                }
//                sourceBuilder.AppendLine("\t}");
//                sourceBuilder.AppendLine("}");
//                sourceBuilder.AppendLine("");
//                sourceBuilder.AppendLine("#nullable restore");
//                var source = sourceBuilder.ToString();
//                dtos.Add(new ScaffoldedDto
//                {
//                    ClassFileName = $"{scaffoldType.Name}Dto.cs",
//                    SourceCode = source,
//                });
//            }
//            return dtos;
//        }

        public INamedTypeSymbol? RecurseNamespaceForTypeWithBaseTypeName(INamespaceSymbol targetNamespace, string baseTypeName)
        {
            var types = targetNamespace.GetTypeMembers();
            foreach (var type in types)
            {
                if (type.BaseType != null && type.BaseType.Name.Equals(baseTypeName))
                {
                    return type;
                }
            }
            // type not found in target namespace; check child namespaces recursively
            foreach (var childNamespace in targetNamespace.GetNamespaceMembers())
            {
                var discoveredType = RecurseNamespaceForTypeWithBaseTypeName(childNamespace, baseTypeName);
                if (discoveredType != null)
                {
                    return discoveredType;
                }
            }
            return null;
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this one
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif 
            Debug.WriteLine("Initalize code generator");
        }
    }
}