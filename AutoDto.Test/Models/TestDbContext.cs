using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoDtoConfig.Attributes;
using AutoDtoConfig;

namespace AutoDto.Test.Models
{
    //[AutoDtoConfiguration(
    //    ClassDiscoveryBehavior= ClassDiscoveryBehavior.ExcludeAll, 
    //    RequestDtoNamingTemplate = "{RequestType}{BaseClassName}RequestTEST",
    //    ResponseDtoNamingTemplate = "{ResponseType}{BaseClassName}ResponseTEST",
    //    GenerateRequestTypes = GeneratedRequestType.Create, 
    //    GenerateResponseTypes = GeneratedResponseType.All,
    //    RequestTypesIncludingAllPropertiesByDefault = GeneratedRequestType.None,
    //    ResponseTypesIncludingAllPropertiesByDefault = GeneratedResponseType.All)]
    [AutoDtoConfiguration(
        GenerateResponseTypes = GeneratedResponseType.Generic)]
    internal class TestDbContext : DbContext
    {
        public virtual DbSet<TestClass1> TestClass1 { get; set; }

        public virtual DbSet<TestChildClass1> TestChildClass1 { get; set; }

        public virtual DbSet<TestClass2> TestClass2 { get; set; }

        public virtual DbSet<TestEmptyClass> TestEmptyClass { get; set; }
    }
}
