using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using IEvangelist.Harness.Exceptions;
using IEvangelist.Harness.Interfaces;
using IEvangelist.Harness.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;

namespace IEvangelist.Harness
{
    public class Harness : IDisposable, IHarness, IAct, IAssert
    {
        private readonly List<IWebDriver> _browsers = new List<IWebDriver>();

        private IWebDriver _browser;

        public Harness() : this(new Configuration())
        {
        }

        public Harness(Configuration configuration) => Configuration = configuration;

        #region IHarness

        public Configuration Configuration { get; }

        public IWebDriver CurrentDriver => _browser;

        public IWebDriver StartChrome()
        {
            var options = new ChromeOptions();
            options.AddArgument("--ignore-certificate-errors");
            if (Configuration.RunChromeHeadless)
            {
                options.AddArguments("headless", "disable-gpu");
            }
            _browser = new ChromeDriver(Configuration.DriverDirectory, options);
            _browsers.Add(_browser);

            return _browser;
        }

        public IWebDriver StartInternetExplorer()
        {
            var options = new InternetExplorerOptions()
                          {
                              IntroduceInstabilityByIgnoringProtectedModeSettings = true,
                          };
            _browser = new InternetExplorerDriver(Configuration.DriverDirectory, options);
            _browsers.Add(_browser);
            return _browser;
        }

        public IWebDriver StartFireFox()
        {
            var service = FirefoxDriverService.CreateDefaultService(Configuration.DriverDirectory);
            _browser = new FirefoxDriver(service);
            _browsers.Add(_browser);
            return _browser;
        }
        
        public void SwitchTo(IWebDriver browser) => _browser = browser;

        public int NewTab()
        {
            var numWindowHandles = _browser.WindowHandles.Count;

            _browser.ExecuteJavaScript<object>("window.open('', '_blank')");

            WaitUntil(Configuration.PageTimeout, () => _browser.WindowHandles.Count == numWindowHandles + 1);

            _browser.SwitchTo().Window(_browser.WindowHandles[_browser.WindowHandles.Count - 1]);

            return _browser.WindowHandles.Count;
        }

        public int CurrentTab => _browser.WindowHandles.IndexOf(_browser.CurrentWindowHandle) + 1;

        public void CloseTab(int? tab = null)
        {
            var currentWindowHandle = _browser.CurrentWindowHandle;

            if (tab != null)
            {
                SwitchToTab(tab.Value);
                _browser.Close();
                _browser.SwitchTo().Window(currentWindowHandle);

                WaitUntil(Configuration.PageTimeout, () => _browser.CurrentWindowHandle == currentWindowHandle);
            }
            else
            {
                var nextTabIndex = CurrentTab == _browser.WindowHandles.Count ? CurrentTab - 1 : CurrentTab + 1;
                var nextTab = _browser.WindowHandles[nextTabIndex - 1];

                _browser.Close();
                _browser.SwitchTo().Window(nextTab);

                WaitUntil(Configuration.PageTimeout, () => _browser.CurrentWindowHandle == nextTab);
            }
        }

        public void CloseOtherTabs(int? tab = null)
        {
            if (tab != null)
            {
                SwitchToTab(tab.Value);
            }

            var currentWindowHandle = _browser.CurrentWindowHandle;

            foreach (var handle in _browser.WindowHandles)
            {
                if (handle != currentWindowHandle)
                {
                    CloseTab(_browser.WindowHandles.IndexOf(handle) + 1);
                }
            }
        }

        public void SwitchToTab(int tab)
        {
            try
            {
                WaitUntil(Configuration.PageTimeout,
                          () =>
                          {
                              _browser.SwitchTo().Window(_browser.WindowHandles[tab - 1]);
                              return true;
                          });
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessSwitchTabFailedException($"Failed to switch to tab {tab}");
            }
        }

        public bool WaitUntil(int timeout, Func<bool> function) => Wait.Until(timeout, function, this);

        public T WaitUntil<T>(int timeout, Func<T> function) => Wait.Until(timeout, function, this);

        public void Dispose() => Quit();

        public void Quit()
        {
            foreach (var browser in _browsers)
            {
                browser.Quit();
            }

            _browsers.Clear();
            _browser = null;
        }

        #endregion // IHarness

        #region IAssert

        public void Displayed(string cssSelector)
        {
            try
            {
                WaitUntil(Configuration.ElementTimeout,
                          () =>
                          {
                              var atLeastOneDisplayed = false;
                              var elements = Elements(cssSelector);
                              foreach (var element in elements)
                                  try
                                  {
                                      if (!atLeastOneDisplayed)
                                           atLeastOneDisplayed = IsDisplayed(element);
                                  }
                                  catch
                                  {
                                      // Ignore all exceptions
                                  }
                              return atLeastOneDisplayed;
                          });
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessDisplayedFailedException($"Displayed check failed for {cssSelector}");
            }
            catch (HarnessDisplayedFailedException)
            {
                throw new HarnessDisplayedFailedException($"Displayed check failed for {cssSelector}");
            }
        }

        public void Displayed(IWebElement webElement)
        {
            try
            { 
                WaitUntil(Configuration.CompareTimeout, () => IsDisplayed(webElement));
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessDisplayedFailedException($"Displayed check failed for {webElement}");
            }
        }

        public void NotDisplayed(string cssSelector)
        {
            try
            {
                WaitUntil(Configuration.CompareTimeout,
                          () => !UnreliableElements(cssSelector).Any() ||
                                UnreliableElements(cssSelector).All(element => !IsDisplayed(element)));
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessNotDisplayedFailedException($"NotDisplayed check failed for {cssSelector}");
            }
            catch (HarnessNotDisplayedFailedException)
            {
                throw new HarnessNotDisplayedFailedException($"NotDisplayed check failed for {cssSelector}");
            }
        }

        public void NotDisplayed(IWebElement webElement)
        {
            try
            {
                WaitUntil(Configuration.CompareTimeout, () => !IsDisplayed(webElement));
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessNotDisplayedFailedException($"NotDisplayed check failed for {webElement}");
            }
        }

        private static bool IsDisplayed(IWebElement webElement)
            => webElement.Displayed;

        public void Enabled(string cssSelector)
        {
            try
            {
                Enabled(Element(cssSelector));
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessEnabledFailedException($"Enabled check failed for {cssSelector}");
            }
            catch (HarnessEnabledFailedException)
            {
                throw new HarnessEnabledFailedException($"Enabled check failed for {cssSelector}");
            }
        }

        public void Enabled(IWebElement webElement)
        {
            try
            {
                WaitUntil(Configuration.CompareTimeout, () => webElement.Enabled);
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessEnabledFailedException($"Enabled check failed for {webElement}");
            }
        }

        public void Disabled(string cssSelector)
        {
            try
            {
                Disabled(Element(cssSelector));
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessDisabledFailedException($"Disabled check failed for {cssSelector}");
            }
            catch (HarnessDisabledFailedException)
            {
                throw new HarnessDisabledFailedException($"Disabled check failed for {cssSelector}");
            }
        }

        public void Disabled(IWebElement webElement)
        {
            try
            {
                WaitUntil(Configuration.CompareTimeout, () => !webElement.Enabled);
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessDisabledFailedException($"Disabled check failed for {webElement}");
            }
        }

        public void Class(string cssSelector, string className)
        {
            try
            {
                Class(Element(cssSelector), className);
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessClassFailedException($"{className} is not present on {cssSelector}");
            }
            catch (HarnessClassFailedException)
            {
                throw new HarnessClassFailedException($"{className} is not present on {cssSelector}");
            }
        }

        public void NoClass(string cssSelector, string className)
        {
            try
            {
                NoClass(Element(cssSelector), className);
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessNoClassFailedException($"{className} is present on {cssSelector}");
            }
            catch (HarnessNoClassFailedException)
            {
                throw new HarnessNoClassFailedException($"{className} is present on {cssSelector}");
            }
        }

        public void Class(IWebElement webElement, string className)
        {
            try
            {
                WaitUntil(Configuration.CompareTimeout,
                          () => webElement.GetAttribute("class").Contains(className));
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessClassFailedException($"{className} is not present on {webElement}");
            }
        }

        public void NoClass(IWebElement webElement, string className)
        {
            try
            {
                WaitUntil(Configuration.CompareTimeout,
                          () => !webElement.GetAttribute("class").Contains(className));
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessNoClassFailedException($"{className} is present on {webElement}");
            }
        }

        public void ValueEquals(string cssSelector, Func<string, bool> comparer, string expected)
        {
            var bestValue = string.Empty;
            try
            {
                WaitUntil(Configuration.CompareTimeout,
                          () =>
                          {
                              var webElement = Element(cssSelector);
                              var readValue = TextOf(webElement);

                              if (comparer(readValue))
                              {
                                  return true;
                              }

                              bestValue = readValue;

                              return false;
                          });
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessValueFailedException($"Value comparison failed for {cssSelector}. Expected: {expected}, Got: {bestValue}");
            }
        }

        public void ValueNotEquals(string cssSelector, Func<string, bool> comparer, string expected)
        {
            try
            {
                WaitUntil(Configuration.CompareTimeout,
                          () =>
                          {
                              var webElement = Element(cssSelector);
                              var readValue = TextOf(webElement);

                              return !comparer(readValue);
                          });
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessValueFailedException($"Value not equals comparison failed for {cssSelector}. Expected anything other than: {expected}");
            }
        }

        public void ValueEquals(string cssSelector, string value, bool ignoreCase = false) 
            => ValueEquals(cssSelector, actual => ignoreCase ? value.Equals(actual, StringComparison.OrdinalIgnoreCase) : value.Equals(actual), value);

        public void ValueMatches(string cssSelector, Regex regex) => ValueEquals(cssSelector, regex.IsMatch, regex.ToString());

        public void ValueNotEquals(string cssSelector, string value, bool ignoreCase = false) 
            => ValueNotEquals(cssSelector, actual => ignoreCase ? value.Equals(actual, StringComparison.OrdinalIgnoreCase) : value.Equals(actual), value);

        public void ValueNotMatches(string cssSelector, Regex regex) => ValueEquals(cssSelector, actual => !regex.IsMatch(actual), regex.ToString());

        public string GetAttributeValue(string cssSelector, string attribute)
        {
            var bestValue = string.Empty;
            try
            {
                WaitUntil(Configuration.CompareTimeout,
                          () =>
                          {
                              var webElement = Element(cssSelector);
                              bestValue = webElement.GetAttribute(attribute);
                              return bestValue != null;
                          });
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessAttributeNotFound($"Attribute: {attribute}, was not found on {cssSelector}");
            }
            return bestValue;
        }

        private void AttributeEquals(string cssSelector, string attribute, Func<string, bool> comparer, string expected)
        {
            var bestValue = string.Empty;
            try
            {
                WaitUntil(Configuration.CompareTimeout,
                          () =>
                          {
                              var webElement = Element(cssSelector);
                              var readValue = webElement.GetAttribute(attribute);
                              if (comparer(readValue))
                              {
                                  return true;
                              }

                              bestValue = readValue;

                              return false;
                          });
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessValueFailedException($"Attribute comparison failed for {cssSelector}. Expected: {expected}, Got: {bestValue}");
            }
        }

        public void AttributeEquals(string cssSelector, string attribute, string value)
            => AttributeEquals(cssSelector, attribute, actual => actual == value, value);

        public void AttributeNotEquals(string cssSelector, string attribute, string value)
            => AttributeEquals(cssSelector, attribute, actual => actual != value, value);

        public void AttributeMatches(string cssSelector, string attribute, Regex regex)
            => AttributeEquals(cssSelector, attribute, regex.IsMatch, regex.ToString());

        public void AttributeNotMatches(string cssSelector, string attribute, Regex regex)
            => AttributeEquals(cssSelector, attribute, actual => !regex.IsMatch(actual), regex.ToString());

        private void UrlEquals(Func<string, bool> comparer, string expected)
        {
            var bestValue = string.Empty;
            try
            {
                WaitUntil(Configuration.CompareTimeout,
                          () =>
                          {
                              var uri = new Uri(_browser.Url);
                              if (comparer(uri.PathAndQuery))
                              {
                                  return true;
                              }

                              bestValue = uri.PathAndQuery;

                              return false;
                          });
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessUrlFailedException($"Url comparison failed. Expected: {expected}, Got: {bestValue}");
            }
        }

        public void UrlEquals(string url) => UrlEquals(actual => url == actual, url);

        public void UrlMatches(Regex url) => UrlEquals(actual => url.IsMatch(actual), url.ToString());

        public void UrlNotEquals(string url) => UrlEquals(actual => url != actual, url);

        public void UrlNotMatches(Regex url) => UrlEquals(actual => !url.IsMatch(actual), url.ToString());

        private void TitleEquals(Func<string, bool> comparer, string expected)
        {
            var bestValue = string.Empty;
            try
            {
                WaitUntil(Configuration.CompareTimeout,
                          () =>
                          {
                              if (comparer(_browser.Title))
                              {
                                  return true;
                              }

                              bestValue = _browser.Title;

                              return false;
                          });
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessTitleFailedException($"Title comparison failed. Expected: {expected}, Got: {bestValue}");
            }
        }

        public void TitleEquals(string title) => TitleEquals(actual => title == actual, title);

        public void TitleMatches(Regex title) => TitleEquals(actual => title.IsMatch(actual), title.ToString());

        public void TitleNotEquals(string title) => TitleEquals(actual => title != actual, title);

        public void TitleNotMatches(Regex title) => TitleEquals(actual => !title.IsMatch(actual), title.ToString());

        public void AlertEquals(string value)
        {
            var bestValue = string.Empty;

            try
            {
                WaitUntil(Configuration.CompareTimeout,
                          () =>
                          {
                              bestValue = _browser.SwitchTo().Alert().Text;
                              return bestValue == value;
                          });
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessValueFailedException($"Alert value comparison failed. Expected: {value}, Got: {bestValue}");
            }
        }

        #endregion // IAssert

        #region IAct

        public void GoToUrl(string url) => GoToUrl(new Uri(url));

        public void GoToUrl(Uri url) => _browser.Navigate().GoToUrl(url);

        public void Back() => _browser.Navigate().Back();

        public void Forward() => _browser.Navigate().Forward();

        public void Refresh() => _browser.Navigate().Refresh();

        public void Write(string cssSelector, string text, Modifiers modifiers)
        {
            try
            {
                WaitUntil(Configuration.ElementTimeout,
                          () =>
                          {
                              var anythingWritten = false;
                              var elements = Elements(cssSelector);

                              foreach (var element in elements)
                              {
                                  try
                                  {
                                      WriteToElement(element, text, modifiers);
                                      anythingWritten = true;
                                  }
                                  catch
                                  {
                                      // Ignore all exceptions
                                  }
                              }

                              return anythingWritten;
                          });
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessWriteFailedException($"Timed out trying to write '{text}' to {cssSelector}");
            }
        }

        public void Write(string cssSelector, string text) => Write(cssSelector, text, new Modifiers());

        private void WriteToElement(IWebElement webElement, string text, Modifiers modifiers)
        {
            if (webElement.TagName == "select")
            {
                WriteToSelect(webElement, text, modifiers);
            }
            else
            {
                if (webElement.GetAttribute("readonly") == "true")
                {
                    throw new HarnessReadOnlyException($"Element {webElement} is marked as read only, you can not write to read only elements.");
                }

                if (!Configuration.OptimizeByDisablingClearBeforeWrite)
                {
                    try
                    {
                        webElement.Clear();
                    }
                    catch
                    {
                        // Ignore all errors
                    }

                    Sendkeys(webElement, text, modifiers);
                }
            }
        }

        private void WriteToSelect(IWebElement webElement, string text, Modifiers modifiers)
        {
            var selector = Configuration.WriteToSelectWithOptionValue
                               ? $@"""option[text()={text}] | option[@value={text}] | optgroup/option[text()={text}] | optgroup/option[@value={text}]"""
                               : $@"""option[text()={text}] | optgroup/option[text()={text}]""";

            var elementOptions = UnreliableElementsWithin(webElement, selector).ToList();
            if (elementOptions.Any())
            {
                var actions = BuildActionsFromModifiers(webElement, modifiers);

                actions.Perform();

                try
                {
                    elementOptions[0].Click();
                }
                finally
                {
                    // Cleanup, perform KeyUp for all modifiers pressed (KeyDown == KeyUp in WebDriver)
                    actions.Perform();
                }
            }
            else
            {
                throw new HarnessOptionNotFoundException($"Element '{webElement}' does not contain value '{text}'.");
            }
        }

        private void Sendkeys(IWebElement webElement, string text, Modifiers modifiers)
        {
            var actions = BuildActionsFromModifiers(webElement, modifiers);

            actions.Perform();

            try
            {
                if (Configuration.SingleCharacterEntry)
                {
                    foreach (var key in text.ToCharArray())
                    {
                        webElement.SendKeys(key.ToString());
                    }
                }
                else
                {
                    webElement.SendKeys(text);
                }        
            }
            finally
            {
                // Cleanup, perform KeyUp for all modifiers pressed (KeyDown == KeyUp in WebDriver)
                actions.Perform();
            }
        }

        private Actions BuildActionsFromModifiers(IWebElement webElement, Modifiers modifiers)
        {
            var actions = new Actions(_browser);

            if (modifiers.Control) actions.KeyDown(webElement, Keys.Control);
            if (modifiers.Shift) actions.KeyDown(webElement, Keys.Shift);
            if (modifiers.Alt) actions.KeyDown(webElement, Keys.Alt);

            return actions;
        }

        public void Check(string cssSelector, bool checkDisabledElements = false, bool checkInvisibleElements = false)
        {
            try
            {
                WaitUntil(Configuration.ElementTimeout,
                          () =>
                          {
                              var atLeastOneChecked = false;
                              var elements = Elements(cssSelector);
                              foreach (var element in elements)
                              {
                                  try
                                  {
                                      if ((element.Enabled || checkDisabledElements) && (element.Displayed || checkInvisibleElements))
                                      {
                                          atLeastOneChecked |= CheckElement(element);
                                      }
                                  }
                                  catch
                                  {
                                      // Ignore all exceptions
                                  }
                              }
                              return atLeastOneChecked;
                          });
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessClickFailedException($"Timed out trying to check {cssSelector}");
            }
            finally
            {
                Thread.Sleep(Configuration.AnimationWaitTimeMilliseconds);
            }
        }

        public void Check(IWebElement webElement)
        {
            try
            {
                WaitUntil(Configuration.ElementTimeout,
                          () => CheckElement(webElement));
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessClickFailedException($"Timed out trying to check {webElement}");
            }
        }

        private bool CheckElement(IWebElement webElement)
        {
            if (!webElement.Selected)
            {
                Click(webElement);
            }

            return webElement.Selected;
        }

        public void Uncheck(string cssSelector, bool uncheckDisabledElements = false, bool uncheckInvisibleElements = false)
        {
            try
            {
                WaitUntil(Configuration.ElementTimeout,
                          () =>
                          {
                              var atLeastOneUnchecked = false;
                              var elements = Elements(cssSelector);
                              foreach (var element in elements)
                              {
                                  try
                                  {
                                      if ((element.Enabled || uncheckDisabledElements) && (element.Displayed || uncheckInvisibleElements))
                                      {
                                          atLeastOneUnchecked |= UncheckElement(element);
                                      }
                                  }
                                  catch
                                  {
                                      // Ignore all exceptions
                                  }
                              }
                              return atLeastOneUnchecked;
                          });
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessClickFailedException($"Timed out trying to uncheck {cssSelector}");
            }
            finally
            {
                Thread.Sleep(Configuration.AnimationWaitTimeMilliseconds);
            }
        }

        public void Uncheck(IWebElement webElement)
        {
            try
            {
                WaitUntil(Configuration.ElementTimeout,
                          () => UncheckElement(webElement));
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessClickFailedException($"Timed out trying to uncheck {webElement}");
            }
        }

        private bool UncheckElement(IWebElement webElement)
        {
            if (webElement.Selected)
            {
                Click(webElement);
            }

            return !webElement.Selected;
        }

        public void Click(string cssSelector, bool clickFirstElement = false, bool clickDisabledElements = false, bool clickInvisibleElements = false)
        {
            try
            {
                WaitUntil(Configuration.ElementTimeout,
                          () =>
                          {
                              var atLeastOneClicked = false;
                              var elements = Elements(cssSelector);
                              foreach (var element in elements)
                              {
                                  try
                                  {
                                      if ((element.Enabled || clickDisabledElements) && (element.Displayed || clickInvisibleElements))
                                      {
                                          atLeastOneClicked |= PerformClickAction(element);
                                          if (atLeastOneClicked && clickFirstElement)
                                              return true;
                                      }
                                  }
                                  catch
                                  {
                                      // Ignore all exceptions
                                  }
                              }
                              return atLeastOneClicked;
                          });
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessClickFailedException($"Timed out trying to click {cssSelector}");
            }
            finally
            {
                Thread.Sleep(Configuration.AnimationWaitTimeMilliseconds);
            }
        }

        public void Click(IWebElement element)
        {
            try
            {
                WaitUntil(Configuration.ElementTimeout,
                          () => PerformClickAction(element));
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessClickFailedException($"Timed out trying to click {element}");
            }
        }

        public void DoubleClick(string cssSelector)
        {
            try
            {
                WaitUntil(Configuration.ElementTimeout,
                          () =>
                          {
                              var atLeastOneClicked = false;
                              var elements = Elements(cssSelector);
                              foreach (var element in elements)
                              {
                                  try
                                  {
                                      atLeastOneClicked = PerformDoubleClickAction(element);
                                  }
                                  catch
                                  {
                                      // Ignore all exceptions
                                  }
                              }
                              return atLeastOneClicked;
                          });
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessClickFailedException($"Timed out trying to double click {cssSelector}");
            }
        }

        public void DoubleClick(IWebElement element)
        {
            try
            {
                WaitUntil(Configuration.ElementTimeout,
                          () => PerformDoubleClickAction(element));
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessClickFailedException($"Timed out trying to double click {element}");
            }
        }

        private bool PerformDoubleClickAction(IWebElement element)
        {
            var actions = new Actions(_browser);
            actions.DoubleClick(element).Perform();
            return true;
        }

        private bool PerformClickAction(IWebElement element)
        {
            var actions = new Actions(_browser);
            actions.Click(element).Perform();
            return true;
        }

        private const string DragAndDropScript = @"
return (function (source, target, sourceOffsetX, sourceOffsetY, targetOffsetX, targetOffsetY, externalData) {
    function offset(elem, offsetX, offsetY) {
        var rect = elem.getBoundingClientRect();
        return {
            top: rect.top + window.pageYOffset - elem.clientTop + offsetY,
            left: rect.left + window.pageXOffset - elem.clientLeft + offsetX
        };
    }

    function createEvent(type, position) {
        var event = document.createEvent(""CustomEvent"");
        event.initCustomEvent(type, true, true, null);
        event.screenX = window.screenX + position.left;
        event.screenY = window.screenY + position.top;
        event.clientX = position.left;
        event.clientY = position.top;
        event.ctrlKey = false;
        event.altKey = false;
        event.shiftKey = false;
        event.metaKey = false;
        event.button = 0;
        event.relatedTarget = null;
        event.dataTransfer = {
            data: {},
            setData: function(type, value) {
                this.data[type] = value;
            },
            getData: function(type, value) {
                return this.data[type] || """";
            },
            clearData: function(type) {
                if (type) {
                    delete this.data[type];
                } else {
                    this.data = {};
                }
            },
            setDragImage: function(img, xOffset, yOffset) {
            }
        };

        return event;
    }

	try {
	    console.log(arguments);
	    var evStart, evEnter, evOver, evDrop, evEnd;

	    if (source && target) {
	        evStart = createEvent(""dragstart"", offset(source, sourceOffsetX, sourceOffsetY));
	        evEnter = createEvent(""dragenter"", offset(target, targetOffsetX, targetOffsetY));
	        evOver = createEvent(""dragover"", offset(target, targetOffsetX, targetOffsetY));
	        evDrop = createEvent(""drop"", offset(target, targetOffsetX, targetOffsetY));
	        evEnd = createEvent(""dragend"", offset(source, sourceOffsetX, sourceOffsetY));

	        evEnter.dataTransfer = evStart.dataTransfer;
	        evOver.dataTransfer = evStart.dataTransfer;
	        evDrop.dataTransfer = evStart.dataTransfer;
	        evEnd.dataTransfer = evStart.dataTransfer;

	        console.log(""draganddrop.js: "" + evStart);
	        source.dispatchEvent(evStart);
	        console.log(""draganddrop.js: "" + evEnter);
	        target.dispatchEvent(evEnter);
	        console.log(""draganddrop.js: "" + evOver);
	        target.dispatchEvent(evOver);
	        console.log(""draganddrop.js: "" + evDrop);
	        target.dispatchEvent(evDrop);
	        console.log(""draganddrop.js: "" + evEnd);
	        source.dispatchEvent(evEnd);

	        return true;
	    } else if (source && !target) {
	        evStart = createEvent(""dragstart"", offset(source, sourceOffsetX, sourceOffsetY));
	        evEnd = createEvent(""dragend"", offset(source, sourceOffsetX, sourceOffsetY));

	        evEnd.dataTransfer = evStart.dataTransfer;

	        console.log(""draganddrop.js: "" + evStart);
	        source.dispatchEvent(evStart);
	        console.log(""draganddrop.js: "" + evEnd);
	        source.dispatchEvent(evEnd);

	        return evStart.dataTransfer.data;
	    } else if (!source && target && externalData) {
	        evEnter = createEvent(""dragenter"", offset(target, targetOffsetX, targetOffsetY));
	        evOver = createEvent(""dragover"", offset(target, targetOffsetX, targetOffsetY));
	        evDrop = createEvent(""drop"", offset(target, targetOffsetX, targetOffsetY));

	        evEnter.dataTransfer.data = externalData;
	        evOver.dataTransfer = evEnter.dataTransfer;
	        evDrop.dataTransfer = evEnter.dataTransfer;

	        console.log(""draganddrop.js: "" + evEnter);
	        target.dispatchEvent(evEnter);
	        console.log(""draganddrop.js: "" + evOver);
	        target.dispatchEvent(evOver);
	        console.log(""draganddrop.js: "" + evDrop);
	        target.dispatchEvent(evDrop);

	        return true;
	    }
	} catch (e) {
        return e.toString() + "" "" + JSON.stringify(arguments);
    }
}).apply(null, arguments);";

        public void DragAndDrop(DragDetails source, DragDetails destination)
        {
            var sourceTab = source.Tab ?? CurrentTab;
            var destinationTab = destination.Tab ?? CurrentTab;
            var originalTab = CurrentTab;

            try
            {
                WaitUntil(Configuration.ElementTimeout,
                          () =>
                          {
                              object res;

                              if (sourceTab != destinationTab)
                              {
                                  SwitchToTab(sourceTab);
                                  var sourceElem = Element(source.CssSelector);

                                  SwitchToTab(destinationTab);
                                  var destinationElem = Element(destination.CssSelector);

                                  SwitchToTab(sourceTab);
                                  var dataTransfer = _browser.ExecuteJavaScript<object>(DragAndDropScript,
                                                                                        sourceElem,
                                                                                        null,
                                                                                        source.OffsetX,
                                                                                        source.OffsetY,
                                                                                        destination.OffsetX,
                                                                                        destination.OffsetY,
                                                                                        null);

                                  SwitchToTab(destinationTab);
                                  res = _browser.ExecuteJavaScript<object>(DragAndDropScript,
                                                                           null,
                                                                           destinationElem,
                                                                           source.OffsetX,
                                                                           source.OffsetY,
                                                                           destination.OffsetX,
                                                                           destination.OffsetY,
                                                                           dataTransfer);

                                  SwitchToTab(originalTab);
                              }
                              else
                              {
                                  SwitchToTab(sourceTab);

                                  var sourceElem = Element(source.CssSelector);
                                  var destinationElem = Element(destination.CssSelector);
                                  res = _browser.ExecuteJavaScript<object>(DragAndDropScript,
                                                                           sourceElem,
                                                                           destinationElem,
                                                                           source.OffsetX,
                                                                           source.OffsetY,
                                                                           destination.OffsetX,
                                                                           destination.OffsetY,
                                                                           null);

                                  SwitchToTab(originalTab);
                              }
                              Thread.Sleep(1000);
                              return res;
                          });
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessClickFailedException($"Timed out trying to drag {source.CssSelector} to {destination.CssSelector}");
            }
        }

        public void DragAndDrop(IWebElement elementSource, IWebElement elementDestination, int destinationOffsetX = 0, int destinationOffsetY = 0)
        {
            var sourceLocation = elementSource.Location;
            try
            {
                WaitUntil(Configuration.ElementTimeout,
                          () =>
                              _browser.ExecuteJavaScript<object>(DragAndDropScript,
                                                                 elementSource,
                                                                 elementDestination,
                                                                 sourceLocation.X,
                                                                 sourceLocation.Y,
                                                                 destinationOffsetX,
                                                                 destinationOffsetY,
                                                                 null));
                Thread.Sleep(1000);
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessClickFailedException($"Timed out trying to drag {elementSource} to {elementDestination}");
            }
        }

        public void DragAndDrop(
            string cssSelectorSource,
            string cssSelectorDestination,
            int destinationOffsetX = 0,
            int destinationOffsetY = 0)
            => DragAndDrop(new DragDetails {CssSelector = cssSelectorSource},
                           new DragDetails
                           {
                               CssSelector = cssSelectorDestination,
                               OffsetX = destinationOffsetX,
                               OffsetY = destinationOffsetY
                           });

        public void ClearCookies() => _browser.Manage().Cookies.DeleteAllCookies();

        public void WriteToAlert(string value) =>
            // TODO: Check if this needs to be in a Wait.Until block
            // TODO: Authentication?
            _browser.SwitchTo().Alert().SendKeys(value);

        public void AcceptAlert() =>
            // TODO: Check if this needs to be in a Wait.Until block
            _browser.SwitchTo().Alert().Accept();

        public void DismissAlert() =>
            // TODO: Check if this needs to be in a Wait.Until block
            _browser.SwitchTo().Alert().Dismiss();

        public string GetCurrentUri() => new Uri(_browser.Url).PathAndQuery;

        public void MaximizeWindow()
        {
            if (Configuration.RunChromeHeadless)
                ResizeWindow(1280, 1024);
            else
                _browser.Manage().Window.Maximize();
        }

        public void ResizeWindow(int width, int height) => _browser.Manage().Window.Size = new Size(width, height);

        public IWebElement Element(string selector)
        {
            var matchedElements = Elements(selector);
            return ElementFromList(selector, matchedElements);
        }

        private IWebElement UnreliableElement(string selector)
        {
            var matchedElements = UnreliableElements(selector);
            return ElementFromList(selector, matchedElements);
        }

        private IWebElement ElementFromList(string selector, IEnumerable<IWebElement> webElements)
        {
            var elements = webElements.ToList();
            if (elements.Count == 0)
            {
                return null;
            }

            if (Configuration.ThrowIfMoreThanOneElement && elements.Count > 1)
            {
                throw new Exception(
                    $"More than one element was selected when only one was expected for selector: {selector}");
            }

            return elements[0];
        }

        public IEnumerable<IWebElement> Elements(string selector) =>
            FindMany(selector, _browser);

        private IEnumerable<IWebElement> UnreliableElements(string selector) =>
            FindMany(selector, _browser, false);

        private IEnumerable<IWebElement> UnreliableElementsWithin(IWebElement webElement, string selector) =>
            FindMany(selector, webElement, false);

        private IEnumerable<IWebElement> FindMany(string selector, ISearchContext searchContext, bool reliable = true) =>
            FindByFunction(selector, searchContext, reliable);

        private IEnumerable<IWebElement> FindByFunction(string selector, ISearchContext searchContext, bool reliable)
        {
            var elements = Enumerable.Empty<IWebElement>();
            if (!reliable)
            {
                return WaitUntil(Configuration.ElementTimeout, () => FindElements(selector, searchContext));
            }

            WaitUntil(Configuration.ElementTimeout,
                      () =>
                      {
                          elements = FindElements(selector, searchContext);
                          return elements.Any();
                      });
            return elements;
        }

        private static IEnumerable<IWebElement> FindElements(string selector, ISearchContext searchContext)
        {
            try
            {
                return Find.ByCss(selector, searchContext)
                           .Concat(Find.ByValue(selector, searchContext))
                           .Concat(Find.ByXpath(selector, searchContext))
                           .Concat(Find.ByText(selector, searchContext))
                           .Concat(Find.ByLabel(selector, searchContext));
            }
            catch
            {
                return Enumerable.Empty<IWebElement>();
            }
        }

        public string Read(string cssSelector)
        {
            var text = string.Empty;
            WaitUntil(Configuration.CompareTimeout,
                      () =>
                      {
                          var webElement = Element(cssSelector);
                          text = TextOf(webElement);
                          return true;
                      });

            return text;
        }

        public string Read(IWebElement webElement)
        {
            var text = string.Empty;
            WaitUntil(Configuration.CompareTimeout,
                      () =>
                      {
                          text = TextOf(webElement);
                          return true;
                      });

            return text;
        }

        private static string TextOf(IWebElement webElement)
        {
            switch (webElement.TagName)
            {
                case "input":
                case "textarea":
                    return webElement.GetAttribute("value");

                case "select":
                {
                    var value = webElement.GetAttribute("value");
                    var options = webElement.FindElements(By.TagName("option"));
                    var option = options.First(e => e.GetAttribute("value") == value);
                    return option.Text;
                }
            }

            return webElement.Text;
        }

        public int GetNumberOfOccurrences(string selector) => UnreliableElements(selector).Count();

        public IEnumerable<IWebElement> GetChildrenElements(string selector)
        {
            var elements = Enumerable.Empty<IWebElement>();
            try
            {
                WaitUntil(Configuration.ElementTimeout,
                          () =>
                          {
                              elements = Element(selector).FindElements(By.XPath("*"));
                              return elements.Any();
                          });
            }
            catch (WebDriverTimeoutException)
            {
                throw new HarnessNoChildrenFoundException($"Unable to locate children elements of {selector}");
            }
            return elements;
        }

        public void SelectElementFromDropDownByValue(string cssSelector, string value)
        {
            try
            {
                WaitUntil(Configuration.ElementTimeout,
                          () =>
                          {
                              var selectElement = new SelectElement(Element(cssSelector));
                              selectElement.SelectByValue(value);
                              return true;
                          });
            }
            catch (WebDriverTimeoutException)
            {
                throw new SelectElementFromDropDownFailedException($"Failed to select value {value} from {cssSelector}");
            }
        }

        public void SelectElementFromDropDownByText(string cssSelector, string text)
        {
            try
            {
                WaitUntil(Configuration.ElementTimeout,
                          () =>
                          {
                              var selectElement = new SelectElement(Element(cssSelector));
                              selectElement.SelectByText(text);
                              return true;
                          });
            }
            catch (WebDriverTimeoutException)
            {
                throw new SelectElementFromDropDownFailedException($"Failed to select text {text} from {cssSelector}");
            }
        }

        public void SelectElementFromDropDownByIndex(string cssSelector, int index)
        {
            try
            {
                WaitUntil(Configuration.ElementTimeout,
                          () =>
                          {
                              var selectElement = new SelectElement(Element(cssSelector));
                              selectElement.SelectByIndex(index);
                              return true;
                          });
            }
            catch (WebDriverTimeoutException)
            {
                throw new SelectElementFromDropDownFailedException($"Failed to select index {index} from {cssSelector}");
            }
        }

        public void Hover(string cssSelector)
        {
            try
            {
                Hover(Element(cssSelector));
            }
            catch (WebDriverTimeoutException)
            {
                throw new HoverElementFailedException($"Failed to Hover over element {cssSelector}");
            }
            catch (HoverElementFailedException)
            {
                throw new HoverElementFailedException($"Failed to Hover over element {cssSelector}");
            }
        }

        public void Hover(IWebElement element)
        {
            try
            {
                WaitUntil(Configuration.ElementTimeout,
                          () =>
                          {
                              new Actions(_browser)
                                  .MoveToElement(element)
                                  .Build()
                                  .Perform();
                              return true;
                          });
            }
            catch (WebDriverTimeoutException)
            {
                throw new HoverElementFailedException($"Failed to Hover over element {element}");
            }
        }

        public Actions InitiateAction() => new Actions(_browser);

        #endregion // IAct
    }
}