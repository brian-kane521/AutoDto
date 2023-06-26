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
    [AutoDtoConfiguration(
        ClassDiscoveryBehavior= ClassDiscoveryBehavior.ExcludeAll, 
        DtoNameGeneratorType = typeof(DefaultDtoNameGenerator), 
        RequestTypeDefaultMemberBehaviorResolver = typeof(DefaultRequestTypeDefaultBehaviorResolver), 
        RequestTypes = GeneratedRequestType.Create, 
        ResponseTypes = GeneratedResponseType.All)]
    internal class TestDbContext : DbContext
    {
        public virtual DbSet<TestClass1> TestClass1 { get; set; }

        public virtual DbSet<TestChildClass1> TestChildClass1 { get; set; }

        public virtual DbSet<TestClass2> TestClass2 { get; set; }

        public virtual DbSet<TestEmptyClass> TestEmptyClass { get; set; }
    }
}
