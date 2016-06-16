using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Helpers;
using System.Web.Mvc;
using VacancyParcer.Reporter.Models;

namespace VacancyParcer.Reporter.Helpers
{
    public static class PaginationHelper
    {
        public static MvcHtmlString Pagination<T>(this HtmlHelper helper,UrlHelper url, PaginationList<T> list)
        {
            var action = helper.ViewContext.RouteData.Values["action"].ToString();
            
            var builder = new System.Text.StringBuilder();

            var perpageLinks = new TagBuilder("div");
            perpageLinks.AddCssClass("col-md-4 btn-group");
            builder.AppendLine();

            foreach(var el in PaginationList<T>.Perpages)
            {
                var perLink = new TagBuilder("a");
                perLink.AddCssClass("btn");
                perLink.InnerHtml=el.ToString();
                if (el == list.SelectedPerpage)
                {
                    perLink.AddCssClass("btn-primary active");
                }
                else
                {
                    perLink.AddCssClass("btn-default");
                    perLink.Attributes.Add("href", url.Action(action,new {size=el}));
                }
                builder.AppendLine(perLink.ToString());
            }
            perpageLinks.InnerHtml = builder.ToString();
            builder.Clear();

            var pagesLinks = new TagBuilder("div");
            pagesLinks.AddCssClass("col-md-7 btn-group");
            builder.AppendLine();

            var startIndex = (list.SelectedPage - 3)>1?(list.SelectedPage - 3):1;
            for (var i = startIndex; (i <= list.Totalpages)&&(i<=startIndex+6); i++)
            {
                var pageLink = new TagBuilder("a");
                pageLink.AddCssClass("btn");
                pageLink.InnerHtml = i.ToString();
                if (i == list.SelectedPage)
                {
                    pageLink.AddCssClass("btn-primary active");
                }
                else
                {
                    pageLink.AddCssClass("btn-default");
                    pageLink.Attributes.Add("href", url.Action(action, new { size = list.SelectedPerpage, page=i }));
                }
                builder.AppendLine(pageLink.ToString());
            }
            pagesLinks.InnerHtml = builder.ToString();
            builder.Clear();

            var pages=new TagBuilder("div");
            pages.AddCssClass("col-md-1");
            pages.InnerHtml = string.Format("{0} из {1}", list.SelectedPage, list.Totalpages);

            builder.AppendLine();
            builder.AppendLine(perpageLinks.ToString());
            builder.AppendLine(pagesLinks.ToString());
            builder.Append(pages.ToString());

            var pTag = new TagBuilder("div") { InnerHtml = builder.ToString() };
            pTag.AddCssClass("row");
            return new MvcHtmlString(pTag.ToString());
        }
    }
}