using AutoDtoConfig.Attributes;
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
    public partial class AutoDtoSourceGenerator : ISourceGenerator
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
            // Find EF database context class
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
            // Fetch user configuration from attribute on DB context, if any
            var userConfig = GetAttributeInstanceFromSymbol<AutoDtoConfigurationAttribute>(dbContextType);
            if (userConfig != null)
            {
                configuration = userConfig;
            }

            // Grab all Types that exist as a DbSet<T> in the DB context
            var typesToScaffold = GetTypesToScaffoldFromDbContext(dbContextType);

            // Build metadata for each request type specified in the config for each Type
            var scaffoldedDtos = new List<ScaffoldedDto>();
            var mappedRequestTypeNames = new List<RequestTypeNameMappings>();
            foreach (var requestType in GetRequestTypesToScaffold())
            {
                var mappedTypeNames = new Dictionary<string, string>();
                foreach (var scaffoldInfo in typesToScaffold)
                {
                    var excludedByIgnore = scaffoldInfo.IgnoreAttribute != null
                            && (scaffoldInfo.IgnoreAttribute.RequestTypesWherePropertyIsIgnored & requestType) > 0;
                    if (excludedByIgnore)
                    {
                        continue;
                    }
                    var requestDto = ScaffoldRequestDto(requestType, scaffoldInfo);
                    scaffoldedDtos.Add(requestDto);
                    mappedTypeNames.Add(requestDto.BaseClassName, requestDto.ClassName);
                }
                mappedRequestTypeNames.Add(new RequestTypeNameMappings
                {
                    RequestType = requestType,
                    MappedTypeNames = mappedTypeNames,
                });
            }

            // Build metadata for each response type specified in the config for each Type
            var mappedResponseTypeNames = new List<ResponseTypeNameMappings>();
            foreach (var responseType in GetResponseTypesToScaffold())
            {
                var mappedTypeNames = new Dictionary<string, string>();
                foreach (var scaffoldInfo in typesToScaffold)
                {
                    var excludedByIgnore = scaffoldInfo.IgnoreAttribute != null
                            && (scaffoldInfo.IgnoreAttribute.ResponseTypesWherePropertyIsIgnored & responseType) > 0;
                    if (excludedByIgnore)
                    {
                        continue;
                    }
                    var responseDto = ScaffoldResponseDto(responseType, scaffoldInfo);
                    scaffoldedDtos.Add(responseDto);
                    mappedTypeNames.Add(responseDto.BaseClassName, responseDto.ClassName);
                }
                mappedResponseTypeNames.Add(new ResponseTypeNameMappings
                {
                    ResponseType = responseType,
                    MappedTypeNames = mappedTypeNames,
                });
            }

            // Generate source code for DTO classes
            foreach (var dto in scaffoldedDtos)
            {
                try
                {
                    var source = GenerateSourceFromScaffoldedDto(new DtoSourceCodeMetadata
                    {
                        ScaffoldedDto = dto,
                        RequestTypeNameMappings = mappedRequestTypeNames,
                        ResponseTypeNameMappings = mappedResponseTypeNames,
                    });
                    context.AddSource(dto.ClassFileName, source);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                }
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required
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