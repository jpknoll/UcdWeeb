using System;
using FluentNHibernate.Cfg;
using FluentNHibernate.Mapping;
using NHibernate.Tool.hbm2ddl;
using UCDArch.Core.DomainModel;
using UCDArch.Data.NHibernate;

namespace UcdWeeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                ResetDb();
                FetchDb();
            }
            catch (Exception)
            {
                //LOL!
                throw;
            }
        }

        private static void ResetDb()
        {
            var config =
                Fluently.Configure()
                        .Mappings(
                            m =>
                            m.FluentMappings.AddFromAssemblyOf<Customer>()
                                .Conventions.AddFromAssemblyOf<UCDArch.Data.NHibernate.Fluent.HasManyConvention>());

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
            NHibernateSessionConfiguration.Mappings.UseFluentMappings(typeof(Customer).Assembly);

            var repo = new RepositoryWithTypedId<Customer, Guid>();
            var customers = repo.GetAll();
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
            Id().GeneratedBy.GuidComb();

            Map(x => x.Name);
        }
    }
}
