﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AutoDto
{
    [Generator]
    public class AutoDtoSourceGenerator : ISourceGenerator
    {
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
            var dbContextMembers = dbContextType.GetMembers();
            List<ITypeSymbol> typesToScaffold = new List<ITypeSymbol>();
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
                        typesToScaffold.Add(baseClass!);
                    }
                }
            }
            var dtos = ScaffoldDtos(typesToScaffold);
            foreach (var dto in dtos)
            {
                context.AddSource(dto.ClassFileName, dto.SourceCode);
            }
        }

        public class ScaffoldedDto
        {
            public string ClassFileName { get; set; } = null!;
            public string SourceCode { get; set; } = null!;
        }

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
                foreach (var member in scaffoldType.GetMembers())
                {
                    if (member.Kind.Equals(SymbolKind.Property))
                    {
                        var memberProperty = (IPropertySymbol)member;
                        if (memberProperty.Type.IsValueType)
                        {
                            var memberLine = $"\t\tpublic {memberProperty.Type} {memberProperty.Name} {{ get; set; }}";
                            sourceBuilder.AppendLine(memberLine);
                        }
                        else
                        {
                            switch (memberProperty.Type.TypeKind)
                            {
                                case TypeKind.Class:
                                    string nullableAnnotation = string.Empty;
                                    string valueInitializer = " = null!;";
                                    string typeName;
                                    if (typesToScaffold.Any(type => type.Name.Equals(memberProperty.Type.Name)))
                                    {
                                        typeName = $"{memberProperty.Type.Name}Dto";
                                    }
                                    else
                                    {
                                        typeName = memberProperty.Type.OriginalDefinition.ToString();
                                    }
                                    if (memberProperty.Type.NullableAnnotation.Equals(NullableAnnotation.Annotated))
                                    {
                                        nullableAnnotation = "?";
                                        valueInitializer = "";
                                    }
                                    sourceBuilder.AppendLine($"\t\tpublic {typeName}{nullableAnnotation} {memberProperty.Name} {{ get; set; }}{valueInitializer}");
                                    break;
                                case TypeKind.Array:
                                    var arrayType = (IArrayTypeSymbol)memberProperty.Type;
                                    var elementType = arrayType.ElementType;
                                    if (elementType.Kind.Equals(TypeKind.Class) && typesToScaffold.Any(type => type.Name.Equals(elementType.Name)))
                                    {
                                        sourceBuilder.AppendLine($"\t\tpublic {elementType.Name}Dto[] {memberProperty.Name} {{ get; set; }} = new {elementType.Name}Dto[0];");
                                    }
                                    else
                                    {
                                        sourceBuilder.AppendLine($"\t\tpublic {memberProperty.Type.OriginalDefinition} {memberProperty.Name} {{ get; set; }} = new {elementType.OriginalDefinition}[0];");
                                    }
                                    break;
                                case TypeKind.Interface:
                                    if (memberProperty.Type is INamedTypeSymbol && memberProperty.Type.Name.Equals("ICollection"))
                                    {
                                        var typeArguments = ((INamedTypeSymbol)memberProperty.Type).TypeArguments;
                                        var baseClass = typeArguments.FirstOrDefault();
                                        if (baseClass != null && typesToScaffold.Any(type => type.Name.Equals(baseClass.Name)))
                                        {
                                            sourceBuilder.AppendLine($"\t\tpublic ICollection<{baseClass.Name}Dto> {memberProperty.Name} {{ get; set; }} = new List<{baseClass.Name}Dto>();");
                                        }
                                        else
                                        {
                                            sourceBuilder.AppendLine($"\t\tpublic {memberProperty.Type.OriginalDefinition} {memberProperty.Name} {{ get; set; }} = new {memberProperty.Type.OriginalDefinition}();");
                                        }
                                    }
                                    break;
                            }
                        }
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