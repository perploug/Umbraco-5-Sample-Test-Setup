using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model;
using Umbraco.Cms.Web.Routing;

namespace Umbraco.Web.Tests.Common
{
	public class FakeControllerContext : ControllerContext
	{
		private readonly IUmbracoApplicationContext context;
		private readonly Content page;

		public FakeControllerContext(IUmbracoApplicationContext context, Content page)
		{
			this.context = context;
			this.page = page;
		}

		public override RouteData RouteData
		{
			get
			{
				var data = new RouteData();
				data.DataTokens.Add("umbraco-route-def", new RouteDefinition
				{
				    RenderModel = new UmbracoRenderModel(context, page)
				});
				return data;
			}
			set
			{
				base.RouteData = value;
			}
		}
	}
}