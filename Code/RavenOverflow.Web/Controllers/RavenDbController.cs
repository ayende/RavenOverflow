using System;
using CuttingEdge.Conditions;
using Raven.Abstractions.Extensions;
using Raven.Client;
using RavenOverflow.Web.DependencyResolution;

namespace RavenOverflow.Web.Controllers
{
    public abstract class RavenDbController : BaseController
    {
    	protected readonly DocumentStores _documentStores;
    	private IDocumentSession _usersSession, _questionsSession;

        protected RavenDbController(DocumentStores documentStore)
        {
            Condition.Requires(documentStore).IsNotNull();

            _documentStores = documentStore;
        }

        public IDocumentSession UsersSession
        {
			get { return _usersSession ?? (_usersSession = _documentStores.Users.OpenSession()); }
        }

		public IDocumentSession QuestionsSession
		{
			get { return _questionsSession ?? (_questionsSession = _documentStores.Questions.OpenSession()); }
		}

		public IDisposable AggressivelyCacheFor(TimeSpan cacheDuration)
		{
			var q = _documentStores.Questions.AggressivelyCacheFor(cacheDuration);
			var u = _documentStores.Users.AggressivelyCacheFor(cacheDuration);

			return new DisposableAction(() =>
			{
				q.Dispose();
				u.Dispose();
			});
		}

		protected override void OnActionExecuted(System.Web.Mvc.ActionExecutedContext filterContext)
		{
			using(_usersSession)
			{
				if(_usersSession!= null && filterContext.Exception == null)
					_usersSession.SaveChanges();
			}

			using (_questionsSession)
			{
				if (_questionsSession != null && filterContext.Exception == null)
					_questionsSession.SaveChanges();
			}
		}
    }
}