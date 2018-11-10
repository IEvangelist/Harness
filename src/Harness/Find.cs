using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;

namespace IEvangelist.Harness
{
    public static class Find
    {
        public static IEnumerable<IWebElement> ByValue(string value, ISearchContext searchContext) =>
            ByCss($"*[value='{value}']", searchContext);

        public static IEnumerable<IWebElement> ByCss(string cssSelector, ISearchContext searchContext) => 
            ByInternal(By.CssSelector, cssSelector, searchContext);

        public static IEnumerable<IWebElement> ByXpath(string xPath, ISearchContext searchContext) => 
            ByInternal(By.XPath, xPath, searchContext);

        public static IEnumerable<IWebElement> ByText(string text, ISearchContext searchContext) =>
            ByXpath($".//*[normalize-space(text())='{text}']", searchContext);
        
        private static IEnumerable<IWebElement> ByInternal(Func<string, By> byFunc, string pattern, ISearchContext searchContext)
        {
            try
            {
                return searchContext.FindElements(byFunc(pattern));
            }
            catch
            {
                return Enumerable.Empty<IWebElement>();
            }
        }

        public static IEnumerable<IWebElement> ByLabel(string label, ISearchContext searchContext)
        {

            try
            {
                var labels = ByXpath($".//label[normalize-space(text())='{label}']", searchContext).ToList();
                if (labels.Any())
                {
                    var firstLabel = labels[0];
                    var forIdentifier = firstLabel.GetAttribute("for");
                    return string.IsNullOrWhiteSpace(forIdentifier) 
                        ? FirstFollowingField(firstLabel) 
                        : searchContext.FindElements(By.Id(forIdentifier));
                }

                return Enumerable.Empty<IWebElement>();
            }
            catch
            {
                return Enumerable.Empty<IWebElement>();
            }
        }

        private static IEnumerable<IWebElement> FirstFollowingField(ISearchContext label)
        {
            var followingElements = label.FindElements(By.XPath("./following-sibling::*[1]")).ToList();
            if (followingElements.Any() && IsField(followingElements[0]))
            {
                return followingElements.GetRange(0, 1);
            }

            return Enumerable.Empty<IWebElement>();
        }

        private static bool IsField(IWebElement webElement) => 
            webElement.TagName == "select" || webElement.TagName == "textarea" || IsInputField(webElement);

        private static bool IsInputField(IWebElement webElement) => 
            webElement.TagName == "input" && webElement.GetAttribute("type") != "hidden";
    }
}