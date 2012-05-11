using System.Configuration;
using StructureMap;

namespace RavenOverflow.Web.DependencyResolution
{
    public static class IoC
    {
        public static IContainer Initialize()
        {
            ObjectFactory.Initialize(x =>
                                         {
                                             x.AddRegistry(new RavenDbRegistry());
                                             x.AddRegistry(
                                                 new AuthenticationRegistry(
                                                     ConfigurationManager.AppSettings["FacebookAppId"],
                                                     ConfigurationManager.AppSettings["FacebookSecret"]
                                                     ));
                                             x.AddRegistry(new ServiceRegistry());
                                         });
            return ObjectFactory.Container;
        }
    }
}