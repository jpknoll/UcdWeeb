using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using FluentNHibernate;
using FluentNHibernate.Cfg;
using FluentNHibernate.Conventions;
using FluentNHibernate.Diagnostics;
using FluentNHibernate.Mapping;
using FluentNHibernate.Utils;
using NHibernate;
using NHibernate.Exceptions;
using NHibernate.Tool.hbm2ddl;
using NHibernate.Util;
using UCDArch.Core.DomainModel;
using UCDArch.Data.NHibernate;
using UCDArch.Data.NHibernate.Fluent;
using UCDArch.Data.NHibernate.Mapping;

namespace UcdWeeb
{
    public class Program
    {
        static FluentLocalConfig _databaseConfig = new FluentLocalConfig();

        public static void Main(string[] args)
        {
            try
            {
                ResetDb();
                FetchDb();
                Console.WriteLine("Finished running. Press any key to end.");
                Console.ReadKey();
            }
            catch (Exception)
            {
                //LOL!
                throw;
            }
        }

        private static void ResetDb()
        {
            var config = _databaseConfig.Config;
            config.ExposeConfiguration(c => new SchemaExport(c).Execute(true, true, false)).BuildConfiguration();

            PopulateDb(config);
        }

        private static void PopulateDb(FluentConfiguration config)
        {
            using (var session = config.BuildSessionFactory().OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    session.Save(new Customer()
                    {
                        Name = "John Knoll"
                    });

                    tx.Commit();
                }
            }
        }

        private static void FetchDb()
        {
            NHibernateSessionConfiguration.Mappings.UseCustomMapping(_databaseConfig);

            var repo = new RepositoryWithTypedId<Customer, Guid>();
            var customers = repo.GetAll();
        }
    }


    class FluentLocalConfig : IMappingConfiguration
    {
        public FluentConfiguration Config { get; private set; }

        public FluentLocalConfig()
        {
            var typesAll = GetLoadableTypes(Assembly.GetAssembly(typeof(HasManyConvention)));

            var conventions = typesAll.Where(x => x.HasInterface(typeof(IConvention))).ToArray();

            Config =
                Fluently.Configure()
                    .Mappings(m => m.FluentMappings.AddFromAssemblyOf<Customer>())
                    .Mappings(m => conventions.ForEach(c => m.FluentMappings.Conventions.Add(c)));
        }

        public static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        public ISessionFactory BuildSessionFactory()
        {
            return Config.BuildSessionFactory();
        }
    }

    public class Customer : DomainObjectWithTypedId<Guid>
    {
        public virtual string Name { get; set; }
    }

    public class CustomerMap : ClassMap<Customer>
    {
        public CustomerMap()
        {
            Id(x=>x.Id).GeneratedBy.GuidComb();

            Map(x => x.Name);
        }
    }
}
