using System.Text.RegularExpressions;
using OpenQA.Selenium;

namespace IEvangelist.Harness.Interfaces
{
    public interface IAssert
    {
        void Displayed(string cssSelector);

        void Displayed(IWebElement webElement);

        void NotDisplayed(string cssSelector);

        void NotDisplayed(IWebElement webElement);

        void Enabled(string cssSelector);

        void Disabled(string cssSelector);

        void Class(string cssSelector, string className);

        void NoClass(string cssSelector, string className);

        void Class(IWebElement webElement, string className);

        void NoClass(IWebElement webElement, string className);

        void ValueEquals(string cssSelector, string value, bool ignoreCase = false);

        void ValueMatches(string cssSelector, Regex regex);

        void ValueNotEquals(string cssSelector, string value, bool ignoreCase = false);

        void ValueNotMatches(string cssSelector, Regex regex);

        void UrlEquals(string url);

        void UrlMatches(Regex url);

        void UrlNotEquals(string url);

        void UrlNotMatches(Regex url);

        void TitleEquals(string title);

        void TitleMatches(Regex title);

        void TitleNotEquals(string title);

        void TitleNotMatches(Regex title);

        void AlertEquals(string value);

        void AttributeEquals(string cssSelector, string attribute, string value);

        void AttributeNotEquals(string cssSelector, string attribute, string value);

        void AttributeMatches(string cssSelector, string attribute, Regex regex);

        void AttributeNotMatches(string cssSelector, string attribute, Regex regex);
    }
}