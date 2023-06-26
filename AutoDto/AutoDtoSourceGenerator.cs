using AutoDtoConfig.Attributes;
using AutoDtoConfig.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
        private IDtoNameGenerator? dtoNameGenerator;
        private IRequestTypeDefaultMemberBehaviorResolver? requestTypeDefaultMemberBehaviorResolver;
        public AutoDtoSourceGenerator(AutoDtoConfigurationAttribute configuration)
        {
            this.configuration = configuration;
            dtoNameGenerator = null;
            requestTypeDefaultMemberBehaviorResolver = null;
        }

        public AutoDtoSourceGenerator()
        {
            configuration = new AutoDtoConfigurationAttribute();
            dtoNameGenerator = null;
            requestTypeDefaultMemberBehaviorResolver = null;
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
            try
            {
                dtoNameGenerator = (IDtoNameGenerator)Activator.CreateInstance(configuration.DtoNameGeneratorType);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
            requestTypeDefaultMemberBehaviorResolver = 
                (IRequestTypeDefaultMemberBehaviorResolver)Activator.CreateInstance(configuration.RequestTypeDefaultMemberBehaviorResolver);

            if (dtoNameGenerator == null)
            {
                Console.Error.WriteLine("Unable to create instance of IDtoNameGenerator");
                return;
            }
            if (dtoNameGenerator == null)
            {
                Console.Error.WriteLine("Unable to create instance of IRequestTypeDefaultMemberBehaviorResolver");
                return;
            }

            var typesToScaffold = GetTypesToScaffoldFromDbContext(dbContextType);

            //var dtos = ScaffoldDtos(typesToScaffold);
            //foreach (var dto in dtos)
            //{
            //    context.AddSource(dto.ClassFileName, dto.SourceCode);
            //}
        }

        public List<GeneratedRequestType> GetRequestTypesToScaffold()
        {
            var requestTypes = new List<GeneratedRequestType>();
            foreach (var requestType in new[] { 
                GeneratedRequestType.Create, 
                GeneratedRequestType.Read, 
                GeneratedRequestType.Update, 
                GeneratedRequestType.Delete })
            {
                if ((requestType | configuration.RequestTypes) > 0)
                {
                    requestTypes.Add(requestType);
                }
            }
            return requestTypes;
        }

        public List<GeneratedResponseType> GetResponseTypesToScaffold()
        {
            var requestTypes = new List<GeneratedResponseType>();
            foreach (var requestType in new[] {
                GeneratedResponseType.Create,
                GeneratedResponseType.Read,
                GeneratedResponseType.Update,
                GeneratedResponseType.Delete,
                GeneratedResponseType.Generic })
            {
                if ((requestType | configuration.ResponseTypes) > 0)
                {
                    requestTypes.Add(requestType);
                }
            }
            return requestTypes;
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
                var argName = arg.Key;
                var argValue = arg.Value;
                var propertyInfo = constructedObject.GetType().GetProperty(argName);
                try
                {
                    if (argValue.Type != null && argValue.Type.Name.Equals("Type"))
                    {
                        var str = argValue.ToCSharpString();
                        if (argValue.Value != null)
                        {
                            var specifiedType = argValue.Value;
                            propertyInfo.SetValue(constructedObject, argValue.Value);
                        }
                    }
                    else
                    {
                        propertyInfo.SetValue(constructedObject, argValue.Value);
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

        //public ScaffoldedMember? GetScaffoldedRequestProperty(GeneratedRequestType requestType, IPropertySymbol propertySymbol, List<TypeScaffoldingInfo> typesToScaffold)
        //{
        //    switch (propertySymbol.Type.TypeKind)
        //    {
        //        case TypeKind.Class:
        //            string nullableAnnotation = string.Empty;
        //            string valueInitializer = " = null!;";
        //            string typeName;
        //            if (typesToScaffold.Any(type => type.Name.Equals(propertySymbol.Type.Name)))
        //            {
        //                typeName = $"{propertySymbol.Type.Name}Dto";
        //            }
        //            else
        //            {
        //                typeName = propertySymbol.Type.OriginalDefinition.ToString();
        //            }
        //            if (propertySymbol.Type.NullableAnnotation.Equals(NullableAnnotation.Annotated))
        //            {
        //                nullableAnnotation = "?";
        //                valueInitializer = "";
        //            }
        //            return new ScaffoldedMember
        //            {
        //                BaseTypeDeclaration = $"{typeName}{nullableAnnotation}",
        //                BaseName = propertySymbol.Name,
        //                ValueInitializer = valueInitializer,
        //            };
        //            //sourceBuilder.AppendLine($"\t\tpublic {typeName}{nullableAnnotation} {propertySymbol.Name} {{ get; set; }}{valueInitializer}");
        //        case TypeKind.Array:
        //            var arrayType = (IArrayTypeSymbol)propertySymbol.Type;
        //            var elementType = arrayType.ElementType;
        //            if (elementType.Kind.Equals(TypeKind.Class) && typesToScaffold.Any(type => type.BaseType.Name.Equals(elementType.Name)))
        //            {
        //                return new ScaffoldedMember
        //                {
        //                    BaseTypeDeclaration = elementType.Name
        //                }
        //                sourceBuilder.AppendLine($"\t\tpublic {elementType.Name}Dto[] {propertySymbol.Name} {{ get; set; }} = new {elementType.Name}Dto[0];");
        //            }
        //            else
        //            {
        //                sourceBuilder.AppendLine($"\t\tpublic {propertySymbol.Type.OriginalDefinition} {propertySymbol.Name} {{ get; set; }} = new {elementType.OriginalDefinition}[0];");
        //            }
        //            break;
        //        case TypeKind.Interface:
        //            if (propertySymbol.Type is INamedTypeSymbol && propertySymbol.Type.Name.Equals("ICollection"))
        //            {
        //                var typeArguments = ((INamedTypeSymbol)propertySymbol.Type).TypeArguments;
        //                var baseClass = typeArguments.FirstOrDefault();
        //                if (baseClass != null && typesToScaffold.Any(type => type.Name.Equals(baseClass.Name)))
        //                {
        //                    sourceBuilder.AppendLine($"\t\tpublic ICollection<{baseClass.Name}Dto> {propertySymbol.Name} {{ get; set; }} = new List<{baseClass.Name}Dto>();");
        //                }
        //                else
        //                {
        //                    sourceBuilder.AppendLine($"\t\tpublic {propertySymbol.Type.OriginalDefinition} {propertySymbol.Name} {{ get; set; }} = new {propertySymbol.Type.OriginalDefinition}();");
        //                }
        //            }
        //            break;
        //    }
        //}

        public List<ScaffoldedDto> ScaffoldDtos(List<ITypeSymbol> typesToScaffold)
        {
            var dtos = new List<ScaffoldedDto>();
            foreach (var scaffoldType in typesToScaffold)
            {
                var sourceBuilder = new StringBuilder($@"// <auto-generated />
#nullable enable

using System;
using System.Collections.Generic;

namespace AutoDto
{{
{"\t"}public partial class {scaffoldType.Name}Dto
{"\t"}{{
");
                foreach (var memberPropertyInfo in GetPropertiesToScaffold(scaffoldType))
                {
                    var memberProperty = memberPropertyInfo.BaseProperty;
                    if (memberProperty.Type.IsValueType)
                    {
                        var memberLine = $"\t\tpublic {memberProperty.Type} {memberProperty.Name} {{ get; set; }}";
                        sourceBuilder.AppendLine(memberLine);
                    }
                    else
                    {
                        
                    }
                }
                sourceBuilder.AppendLine("\t}");
                sourceBuilder.AppendLine("}");
                sourceBuilder.AppendLine("");
                sourceBuilder.AppendLine("#nullable restore");
                var source = sourceBuilder.ToString();
                dtos.Add(new ScaffoldedDto
                {
                    ClassFileName = $"{scaffoldType.Name}Dto.cs",
                    SourceCode = source,
                });
            }
            return dtos;
        }

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