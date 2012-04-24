using NUnit.Framework;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.FluentExtensions;
using Umbraco.Cms.Web.Model;
using Umbraco.Cms.Web.Mvc;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Web.Controllers.SurfaceControllers;
using Umbraco.Web.Tests.Common;
using Umbraco.Web.ViewModels;

namespace Umbraco.Web.Tests
{
	[TestFixture]
	public class StatusUpdateSurfaceControllerTests : UmbracoTestBase
	{
		private HiveId homeId;
		private IContentRevisionSaveResult<TypedEntity> homePage;
		private StatusUpdateSurfaceController controller;

		[SetUp]
		public void Setup()
		{
			// TODO: One method with a really nice name
			SetupFakeEnvironment();
			SetupRootContent();
		
			// this test only
			SetupDocumentTypesAndContent();
			controller = new StatusUpdateSurfaceController(AppContext);
		}

		[TearDown]
		public void TearDown()
		{
			TeardownFakeEnvironment();
		}

		private void SetupDocumentTypesAndContent()
		{
			var store = AppContext.Hive.Cms<IContentStore>();
			var statusUpdateType = store
				.NewContentType("statusUpdate")
				.Define("post", "multiLineTextBox", "whatever")
				.Commit();
			store.NewContentType("home")
				.AddPermittedTemplate(statusUpdateType.Item.Id)
				.Commit();
			homePage = AppContext.Hive.Cms()
				.NewRevision("home", "home", "home")
				.Commit();
			homeId = homePage.Item.Id;
		}

		[Test]
		public void RenderReturnsViewWithModel()
		{
			var partialViewResult = controller.Render(homePage.Content.Id);
			Assert.AreEqual("StatusUpdateForm", partialViewResult.ViewName);
			Assert.IsInstanceOf<StatusUpdateViewModel>(partialViewResult.Model);
		}

		[Test]
		public void CreatesStatusUpdateItem()
		{
			controller.SetCurrentPage(AppContext, homePage.Content);
			var post = new StatusUpdateViewModel { Post = "Hei hei!", PageId = homeId };
			var result = controller.CreateStatusUpdateItem(post);
			Assert.IsTrue(AppContext.Hive.Cms().Content.Any<Content>("NodeTypeAlias = @0 && post = @1", "statusUpdate", "Hei hei!"));
			Assert.IsInstanceOf<RedirectToUmbracoPageResult>(result);
			Assert.AreEqual(homePage.Content.Id, ((RedirectToUmbracoPageResult)result).PageEntity.Id);
		}

	}
}
