using System;
using System.Collections.Generic;
using IEvangelist.Harness.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace IEvangelist.Harness.Interfaces
{
    public interface IAct
    {
        void GoToUrl(string url);

        void GoToUrl(Uri url);

        void Back();

        void Forward();

        void Refresh();

        void Write(string cssSelector, string value);
        
        void Write(string cssSelector, string value, Modifiers modifiers);

        string Read(string cssSelector);

        string Read(IWebElement webElement);

        void Click(string cssSelector, bool clickFirstElement = false, bool clickDisabledElements = false, bool clickInvisibleElements = false);

        void Click(IWebElement element);

        void Check(string cssSelector, bool checkDisabledElements = false, bool checkInvisibleElements = false);

        void Check(IWebElement webElement);

        void Uncheck(string cssSelector, bool uncheckDisabledElements = false, bool uncheckInvisibleElements = false);

        void Uncheck(IWebElement webElement);

        void DoubleClick(string cssSelector);

        void DoubleClick(IWebElement element);

        void DragAndDrop(string cssSelectorSource, string cssSelectorDestination, int destinationOffsetX = 0, int destinationOffsetY = 0);

        void DragAndDrop(DragDetails source, DragDetails destination);

        void DragAndDrop(IWebElement elementSource, IWebElement elementDestination, int destinationOffsetX = 0, int destinationOffsetY = 0);

        int NewTab();

        int CurrentTab { get; }

        void CloseTab(int? tab = null);

        void CloseOtherTabs(int? tab = null);

        void SwitchToTab(int tab);

        void ClearCookies();

        void WriteToAlert(string value);

        void AcceptAlert();

        void DismissAlert();

        int GetNumberOfOccurrences(string selector);

        IEnumerable<IWebElement> GetChildrenElements(string selector);

        IEnumerable<IWebElement> Elements(string selector);

        IWebElement Element(string selector);

        void ResizeWindow(int width, int height);

        void MaximizeWindow();

        void SelectElementFromDropDownByValue(string cssSelector, string value);

        void SelectElementFromDropDownByText(string cssSelector, string text);

        void SelectElementFromDropDownByIndex(string cssSelector, int index);

        void Hover(string cssSelector);

        void Hover(IWebElement element);

        string GetAttributeValue(string cssSelector, string attribute);

        Actions InitiateAction();
    }
}