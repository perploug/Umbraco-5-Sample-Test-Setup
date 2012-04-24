using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model;
using Umbraco.Cms.Web.Surface;

namespace Umbraco.Web.Tests.Common
{
	public static class SurfaceControllerTestExtensions
	{
		/// <summary>
		/// ATTENTION: Your controller's ControllerContext will be set to a FakeControllerContext
		/// </summary>
		/// <param name="controller"></param>
		/// <param name="context"></param>
		/// <param name="currentPage"></param>
		public static void SetCurrentPage(this SurfaceController controller, IUmbracoApplicationContext context, Content currentPage)
		{
			controller.ControllerContext = new FakeControllerContext(context, currentPage);
		}
	}
}