using System.Collections.Generic;
using System.Text;
using System.Web;
using Raven.Client;
using Raven.Client.Connection;
using Raven.Client.Document;
using Raven.Client.MvcIntegration;
using Raven.Client.Shard;
using RavenOverflow.Web.RavenDb;
using RavenOverflow.Web.RavenDb.Indexes;
using StructureMap.Configuration.DSL;

namespace RavenOverflow.Web.DependencyResolution
{
	public class RavenDbRegistry : Registry
	{
		public RavenDbRegistry()
		{
			For<DocumentStores>()
				.Singleton()
				.Use(x =>
					 {
					 	var shards = new Dictionary<string, IDocumentStore>
                     	{
							{"01", new DocumentStore{Url = "http://localhost:8079", DefaultDatabase = "Questions"}},
							{"02", new DocumentStore{Url = "http://localhost:8078", DefaultDatabase = "Questions"}},
							{"03", new DocumentStore{Url = "http://localhost:8077", DefaultDatabase = "Questions"}},
							{"04", new DocumentStore{Url = "http://localhost:8076", DefaultDatabase = "Questions"}},
                     	};

						 var shardStrategy = new ShardStrategy(shards);
					 	shardStrategy.ShardAccessStrategy.OnError += (commands, request, exception) =>
					 	{
							if(HttpContext.Current.Items["raven-query-error"] == null)
								HttpContext.Current.Items["raven-query-error"] = new HashSet<string>();


							var msgs = ((HashSet<string>)HttpContext.Current.Items["raven-query-error"]);
							msgs.Add(exception.Message + " " + ((ServerClient) commands).Url);

					 		return request.Query != null;
					 	};

					 	var documentStores = new DocumentStores
					 	{
					 		Users = new DocumentStore
					 		{
					 			Url = "http://localhost:8079", DefaultDatabase = "Users"
					 		}.Initialize(), 
							Questions = new ShardedDocumentStore(shardStrategy).Initialize()
					 	};
						

					 	return documentStores;
					 }
				)
				.Named("RavenDB Document Store.");
		}
	}

	public class DocumentStores
	{
		public IDocumentStore Questions { get; set; }
		public IDocumentStore Users { get; set; }
	}
}