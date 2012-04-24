using System.Collections.Specialized;
using System.Linq;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Cms.Web.Mapping;
using Umbraco.Cms.Web.Security;
using Umbraco.Framework;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Framework.Security;
using Umbraco.Framework.Security.Configuration;
using Umbraco.Framework.Security.Model.Entities;
using Umbraco.Framework.TypeMapping;
using Umbraco.Hive;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Tests.Extensions;

namespace Umbraco.Web.Tests.Common
{
	public class UmbracoTestBase
	{
		protected IHiveManager Hive;
		protected IMembershipService<Member> MembershipService;
		protected IPublicAccessService PublicAccessService;
		protected NhibernateTestSetupHelper NhibernateTestSetup;
		protected FakeUmbracoApplicationContext AppContext;

		protected void SetupFakeEnvironment()
		{
			// TODO: Prettify and extract smaller methods

			NhibernateTestSetup = new NhibernateTestSetupHelper();

			var storageProvider = new IoHiveTestSetupHelper(NhibernateTestSetup.FakeFrameworkContext);

			Hive = new HiveManager(
				new[]
					{
						new ProviderMappingGroup(
							"test",
							new WildcardUriMatch("content://"),
							NhibernateTestSetup.ReadonlyProviderSetup,
							NhibernateTestSetup.ProviderSetup,
							NhibernateTestSetup.FakeFrameworkContext),
						storageProvider.CreateGroup("uploader", "storage://file-uploader"),
					},
				NhibernateTestSetup.FakeFrameworkContext);

			AppContext = new FakeUmbracoApplicationContext(Hive, false);

			var resolverContext = new MockedMapResolverContext(NhibernateTestSetup.FakeFrameworkContext, Hive,
															   new MockedPropertyEditorFactory(AppContext),
															   new MockedParameterEditorFactory());
			var webmModelMapper = new CmsModelMapper(resolverContext);
			var renderModelMapper = new RenderTypesModelMapper(resolverContext);

			NhibernateTestSetup.FakeFrameworkContext.SetTypeMappers(
				new FakeTypeMapperCollection(new AbstractMappingEngine[]
				                             	{
				                             		webmModelMapper, renderModelMapper,
				                             		new FrameworkModelMapper(NhibernateTestSetup.FakeFrameworkContext)
				                             	}));

			var membersMembershipProvider = new MembersMembershipProvider { AppContext = AppContext };
			membersMembershipProvider.Initialize("MembersMembershipProvider", new NameValueCollection());
			MembershipService = new MembershipService<Member, MemberProfile>(AppContext.FrameworkContext, Hive,
																			 "security://member-profiles",
																			 "security://member-groups",
																			 Umbraco.Framework.Security.Model.FixedHiveIds.
																				MemberProfileVirtualRoot,
																			 membersMembershipProvider,
																			 Enumerable.Empty<MembershipProviderElement>());

			PublicAccessService = new PublicAccessService(Hive, MembershipService, AppContext.FrameworkContext);
		}

		protected void TeardownFakeEnvironment()
		{
			ClearCaches();
			NhibernateTestSetup.Dispose();
			Hive.Dispose();
		}

		protected void ClearCaches()
		{
			NhibernateTestSetup.SessionForTest.Clear();
			Hive.Context.GenerationScopedCache.IfNotNull(x => x.Clear());
		}

		protected void SetupRootContent()
		{
			AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

			// Ensure parent schema and content root exists for this test
			var contentVirtualRoot = FixedEntities.ContentVirtualRoot;
			var systemRoot = new SystemRoot();
			var contentRootSchema = new ContentRootSchema();
			Hive.AutoCommitTo<IContentStore>(
				x =>
				{
					x.Repositories.AddOrUpdate(systemRoot);
					x.Repositories.AddOrUpdate(contentVirtualRoot);
					x.Repositories.Schemas.AddOrUpdate(contentRootSchema);
				});
		}
	}
}