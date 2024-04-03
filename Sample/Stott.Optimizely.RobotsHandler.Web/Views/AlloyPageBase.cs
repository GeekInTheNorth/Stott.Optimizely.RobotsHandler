using EPiServer.ServiceLocation;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Stott.Optimizely.RobotsHandler.Web.Business.Rendering;

namespace Stott.Optimizely.RobotsHandler.Web.Views
{
    public abstract class AlloyPageBase<TModel> : RazorPage<TModel> where TModel : class
    {
        private readonly AlloyContentAreaItemRenderer _alloyContentAreaItemRenderer;

        public abstract override Task ExecuteAsync();

        public AlloyPageBase() : this(ServiceLocator.Current.GetInstance<AlloyContentAreaItemRenderer>())
        {
        }

        public AlloyPageBase(AlloyContentAreaItemRenderer alloyContentAreaItemRenderer)
        {
            _alloyContentAreaItemRenderer = alloyContentAreaItemRenderer;
        }

        protected void OnItemRendered(ContentAreaItem contentAreaItem, TagHelperContext context, TagHelperOutput output)
        {
            _alloyContentAreaItemRenderer.RenderContentAreaItemCss(contentAreaItem, context, output);
        }
    }
}