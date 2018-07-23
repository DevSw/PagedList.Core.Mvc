﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PagedList.Core.Mvc
{
    [HtmlTargetElement("pager")]
    public class PagerTagHelper : TagHelper
    {
        #region list

        private const string ListAttributeName = "list";

        [HtmlAttributeName(ListAttributeName)]
        public IPagedList List { get; set; }

        #endregion

        #region asp-route

        private const string RouteValuesDictionaryName = "asp-all-route-data";

        private const string RouteValuesPrefix = "asp-route-";

        [HtmlAttributeName(RouteValuesDictionaryName, DictionaryAttributePrefix = RouteValuesPrefix)]
        public IDictionary<string, string> RouteValues { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        #endregion

        #region asp-action

        private const string ActionAttributeName = "asp-action";

        [HtmlAttributeName(ActionAttributeName)]
        public string AspAction { get; set; }

        #endregion

        #region asp-controller

        private const string ControllerAttributeName = "asp-controller";

        [HtmlAttributeName(ControllerAttributeName)]
        public string AspController { get; set; }

        #endregion

        #region options

        private const string OptionsAttributeName = "options";

        [HtmlAttributeName(OptionsAttributeName)]
        public PagedListRenderOptions Options { get; set; }

        #endregion

        #region param-page-number

        private const string ParamPageNumberAttributeName = "param-page-number";

        [HtmlAttributeName(ParamPageNumberAttributeName)]
        public string ParamPageNumber { get; set; } = "page";

        #endregion

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        private IUrlHelperFactory urlHelperFactory;

        public PagerTagHelper(IUrlHelperFactory urlHelperFactory)
        {
            this.urlHelperFactory = urlHelperFactory;
        }

        private string GeneratePageUrl(int pageNumber, IUrlHelper urlHelper)
        {
            var routeValues = new RouteValueDictionary();

            foreach (var routeValue in this.RouteValues)
            {
                if (!routeValues.ContainsKey(routeValue.Key.ToLower()))
                {
                    routeValues.Add(routeValue.Key, routeValue.Value);
                }
            }

            if (!routeValues.ContainsKey(this.ParamPageNumber))
            {
                routeValues.Add(this.ParamPageNumber, pageNumber.ToString());
            }

            if (this.AspAction != null && this.AspController != null)
            {
                return urlHelper.Action(this.AspAction, this.AspController, routeValues);
            }

            return pageNumber.ToString();
        }

        private TagBuilder WrapInListItem(string text)
        {
            var li = new TagBuilder("li");
            li.InnerHtml.AppendHtml(text);

            return li;
        }

        private TagBuilder WrapInListItem(TagBuilder inner, params string[] classes)
        {
            var li = new TagBuilder("li");
            foreach (var @class in classes)
            {
                li.AddCssClass(@class);
            }

            li.InnerHtml.AppendHtml(inner);

            return li;
        }

        private TagBuilder First(IUrlHelper urlHelper)
        {
            const int targetPageNumber = 1;
            var first = new TagBuilder("a");
            foreach (var @class in this.Options.AhrefElementClasses)
            {
                first.AddCssClass(@class);
            }

            first.InnerHtml.AppendHtml(string.Format(this.Options.LinkToFirstPageFormat, targetPageNumber));

            if (this.List.IsFirstPage)
            {
                return WrapInListItem(first, "PagedList-skipToFirst", "disabled");
            }

            first.Attributes["href"] = GeneratePageUrl(targetPageNumber, urlHelper);
            return WrapInListItem(first, "PagedList-skipToFirst");
        }

        private TagBuilder Previous(IUrlHelper urlHelper)
        {
            var targetPageNumber = this.List.PageNumber - 1;
            var previous = new TagBuilder("a");

            foreach (var @class in this.Options.AhrefElementClasses)
            {
                previous.AddCssClass(@class);
            }

            previous.InnerHtml.AppendHtml(string.Format(this.Options.LinkToPreviousPageFormat, targetPageNumber));
            previous.Attributes["rel"] = "prev";

            if (!this.List.HasPreviousPage)
            {
                return WrapInListItem(previous, "PagedList-skipToPrevious", "disabled");
            }

            previous.Attributes["href"] = this.GeneratePageUrl(targetPageNumber, urlHelper);

            return WrapInListItem(previous, "PagedList-skipToPrevious");
        }

        private TagBuilder Page(int i, IUrlHelper urlHelper)
        {
            var format = this.Options.FunctionToDisplayEachPageNumber
                ?? (pageNumber => string.Format(this.Options.LinkToIndividualPageFormat, pageNumber));
            var targetPageNumber = i;
            var page = new TagBuilder("a");
            foreach (var @class in this.Options.AhrefElementClasses)
            {
                page.AddCssClass(@class);
            }

            page.InnerHtml.AppendHtml(format(targetPageNumber));

            if (i == this.List.PageNumber)
                return WrapInListItem(page, "active");

            page.Attributes["href"] = this.GeneratePageUrl(targetPageNumber, urlHelper);
            return WrapInListItem(page);
        }

        private TagBuilder Next(IUrlHelper urlHelper)
        {
            var targetPageNumber = this.List.PageNumber + 1;
            var next = new TagBuilder("a");
            foreach (var @class in this.Options.AhrefElementClasses)
            {
                next.AddCssClass(@class);
            }

            next.InnerHtml.AppendHtml(string.Format(this.Options.LinkToNextPageFormat, targetPageNumber));
            next.Attributes["rel"] = "next";

            if (!this.List.HasNextPage)
            {
                return WrapInListItem(next, "PagedList-skipToNext", "disabled");
            }

            next.Attributes["href"] = this.GeneratePageUrl(targetPageNumber, urlHelper);
            return WrapInListItem(next, "PagedList-skipToNext");
        }

        private TagBuilder Last(IUrlHelper urlHelper)
        {
            var targetPageNumber = this.List.PageCount;
            var last = new TagBuilder("a");
            foreach (var @class in this.Options.AhrefElementClasses)
            {
                last.AddCssClass(@class);
            }

            last.InnerHtml.AppendHtml(string.Format(this.Options.LinkToLastPageFormat, targetPageNumber));

            if (this.List.IsLastPage)
            {
                return WrapInListItem(last, "PagedList-skipToLast", "disabled");
            }

            last.Attributes["href"] = this.GeneratePageUrl(targetPageNumber, urlHelper);
            return WrapInListItem(last, "PagedList-skipToLast");
        }

        private TagBuilder PageCountAndLocationText()
        {
            var text = new TagBuilder("a");
            text.InnerHtml.AppendHtml(string.Format(this.Options.PageCountAndCurrentLocationFormat, this.List.PageNumber, this.List.PageCount));

            return WrapInListItem(text, "PagedList-pageCountAndLocation", "disabled");
        }

        private TagBuilder ItemSliceAndTotalText()
        {
            var text = new TagBuilder("a");
            text.InnerHtml.AppendHtml(string.Format(this.Options.ItemSliceAndTotalFormat, this.List.FirstItemOnPage, this.List.LastItemOnPage, this.List.TotalItemCount));

            return WrapInListItem(text, "PagedList-pageCountAndLocation", "disabled");
        }

        private TagBuilder Ellipses()
        {
            var a = new TagBuilder("a");
            a.InnerHtml.AppendHtml(this.Options.EllipsesFormat);

            return WrapInListItem(a, "PagedList-ellipses", "disabled");
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (this.List == null)
            {
                return;
            }

            if (this.Options == null)
            {
                this.Options = new PagedListRenderOptions();
            }

            var urlHelper = urlHelperFactory.GetUrlHelper(ViewContext);

            var listItemLinks = new List<TagBuilder>();

            //calculate start and end of range of page numbers
            var firstPageToDisplay = 1;
            var lastPageToDisplay = this.List.PageCount;
            var pageNumbersToDisplay = lastPageToDisplay;
            if (this.Options.MaximumPageNumbersToDisplay.HasValue && this.List.PageCount > this.Options.MaximumPageNumbersToDisplay)
            {
                // cannot fit all pages into pager
                var maxPageNumbersToDisplay = this.Options.MaximumPageNumbersToDisplay.Value;
                firstPageToDisplay = this.List.PageNumber - maxPageNumbersToDisplay / 2;
                if (firstPageToDisplay < 1)
                {
                    firstPageToDisplay = 1;
                }

                pageNumbersToDisplay = maxPageNumbersToDisplay;
                lastPageToDisplay = firstPageToDisplay + pageNumbersToDisplay - 1;
                if (lastPageToDisplay > this.List.PageCount)
                {
                    lastPageToDisplay = this.List.PageCount;
                    firstPageToDisplay = this.List.PageCount - maxPageNumbersToDisplay + 1;
                }
            }

            //first
            if (this.Options.DisplayLinkToFirstPage == PagedListDisplayMode.Always || (this.Options.DisplayLinkToFirstPage == PagedListDisplayMode.IfNeeded && firstPageToDisplay > 1))
            {
                listItemLinks.Add(First(urlHelper));
            }

            //previous
            if (this.Options.DisplayLinkToPreviousPage == PagedListDisplayMode.Always || (this.Options.DisplayLinkToPreviousPage == PagedListDisplayMode.IfNeeded && !this.List.IsFirstPage))
            {
                listItemLinks.Add(Previous(urlHelper));
            }

            //text
            if (this.Options.DisplayPageCountAndCurrentLocation)
            {
                listItemLinks.Add(PageCountAndLocationText());
            }

            //text
            if (this.Options.DisplayItemSliceAndTotal)
            {
                listItemLinks.Add(ItemSliceAndTotalText());
            }

            //page
            if (this.Options.DisplayLinkToIndividualPages)
            {
                //if there are previous page numbers not displayed, show an ellipsis
                if (this.Options.DisplayEllipsesWhenNotShowingAllPageNumbers && firstPageToDisplay > 1)
                {
                    listItemLinks.Add(Ellipses());
                }

                for (int i = firstPageToDisplay; i <= lastPageToDisplay; i++)
                {
                    //show delimiter between page numbers
                    if (i > firstPageToDisplay && !string.IsNullOrWhiteSpace(this.Options.DelimiterBetweenPageNumbers))
                    {
                        listItemLinks.Add(WrapInListItem(this.Options.DelimiterBetweenPageNumbers));
                    }

                    //show page number link
                    listItemLinks.Add(Page(i, urlHelper));
                }

                //if there are subsequent page numbers not displayed, show an ellipsis
                if (this.Options.DisplayEllipsesWhenNotShowingAllPageNumbers && (firstPageToDisplay + pageNumbersToDisplay - 1) < this.List.PageCount)
                {
                    listItemLinks.Add(Ellipses());
                }
            }

            //next
            if (this.Options.DisplayLinkToNextPage == PagedListDisplayMode.Always || (this.Options.DisplayLinkToNextPage == PagedListDisplayMode.IfNeeded && !this.List.IsLastPage))
            {
                listItemLinks.Add(Next(urlHelper));
            }

            //last
            if (this.Options.DisplayLinkToLastPage == PagedListDisplayMode.Always || (this.Options.DisplayLinkToLastPage == PagedListDisplayMode.IfNeeded && lastPageToDisplay < this.List.PageCount))
            {
                listItemLinks.Add(Last(urlHelper));
            }

            if (listItemLinks.Any())
            {
                //append class to first item in list?
                if (!string.IsNullOrWhiteSpace(this.Options.ClassToApplyToFirstListItemInPager))
                {
                    listItemLinks.First().AddCssClass(this.Options.ClassToApplyToFirstListItemInPager);
                }

                //append class to last item in list?
                if (!string.IsNullOrWhiteSpace(this.Options.ClassToApplyToLastListItemInPager))
                {
                    listItemLinks.Last().AddCssClass(this.Options.ClassToApplyToLastListItemInPager);
                }

                //append classes to all list item links
                foreach (var li in listItemLinks)
                {
                    foreach (var c in this.Options.LiElementClasses ?? Enumerable.Empty<string>())
                    {
                        li.AddCssClass(c);
                    }
                }
            }

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;

            var ul = new TagBuilder("ul");

            foreach (var linkItem in listItemLinks)
            {
                ul.InnerHtml.AppendHtml(linkItem);
            }

            if (this.Options.UlElementClasses != null)
            {
                foreach (var cssClass in this.Options.UlElementClasses)
                {
                    ul.AddCssClass(cssClass);
                }
            }

            output.Content.AppendHtml(ul);
        }
    }
}