using System;
using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Raven.Client;
using RavenOverflow.Core.Entities;
using RavenOverflow.FakeData;
using RavenOverflow.Web.DependencyResolution;
using RavenOverflow.Web.RavenDb.Indexes;

namespace RavenOverflow.Web.RavenDb
{
    public static class HelperUtilities
    {
        public static void CreateSeedData(this DocumentStores documentStores)
        {
			Condition.Requires(documentStores).IsNotNull();

			RavenFacetTags.CreateFacets(documentStores.Questions);

			new RecentPopularTags().Execute(documentStores.Questions);
			new RecentPopularTagsMapOnly().Execute(documentStores.Questions);
			new Questions_Search().Execute(documentStores.Questions);

			ICollection<User> users = FakeUsers.CreateFakeUsers(50);
			
			using (IDocumentSession documentSession = documentStores.Users.OpenSession())
            {
                // Users.
                StoreFakeEntities(users, documentSession);

           
                documentSession.SaveChanges();
            }

			using (IDocumentSession documentSession = documentStores.Questions.OpenSession())
			{
				// Questions.
				ICollection<Question> questions = FakeQuestions.CreateFakeQuestions(users.Select(x => x.Id).ToList());
				StoreFakeEntities(questions, documentSession);

				documentSession.SaveChanges();
			}
			

        }

        public static void StoreFakeEntities(IEnumerable<RootAggregate> entities, IDocumentSession session)
        {
            // Dont' use Condition.Requires for entities becuase it might enumerate through it.
            if (entities == null)
            {
                throw new ArgumentNullException("entities");
            }

            Condition.Requires(session).IsNotNull();

            foreach (RootAggregate entity in entities)
            {
                session.Store(entity);
            }
        }
    }
}