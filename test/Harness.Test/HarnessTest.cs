using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IEvangelist.Harness.Exceptions;
using Xunit;

namespace IEvangelistDev.Harness.Test
{
    public class ChromeHarnessTest : HarnessTest
    {
        public ChromeHarnessTest(HarnessTestFixture fixture) : base(fixture)
        {
            fixture.Harness.StartChrome();
            fixture.Act.GoToUrl(TestHtmlFile);
        }
    }

    public class FireFoxHarnessTest : HarnessTest
    {
        public FireFoxHarnessTest(HarnessTestFixture fixture) : base(fixture)
        {
            fixture.Harness.StartFireFox();
            fixture.Act.GoToUrl(TestHtmlFile);
        }
    }

    public class InternetExplorerHarnessTest : HarnessTest
    {
        public InternetExplorerHarnessTest(HarnessTestFixture fixture) : base(fixture)
        {
            fixture.Harness.StartInternetExplorer();
            fixture.Act.GoToUrl(TestHtmlFile);
        }
    }

    public abstract class HarnessTest : IClassFixture<HarnessTestFixture>, IDisposable
    {
        public string TestHtmlFile { get; set; } = Environment.ExpandEnvironmentVariables($@"{AppDomain.CurrentDomain.BaseDirectory}\Test.html");

        private readonly HarnessTestFixture _fixture;

        private const string DisabledButton = "#disabledButton";
        private const string EnabledButton = "#enabledButton";
        private const string ToggleCheckbox = "#toggleEnabledCheckbox";
        private const string ToggleButton = "#toggleTargetButton";
        private const string TestDiv = "body > div";
        private const string TestDivText = "Test div";
        private const string ClassDiv = "#classDiv";
        private const string ClassDivClass = "classDivClass";
        private const string DraggableObject = "#draggableObject";
        private const string DropArea = "#dropArea";
        private const string TestTitleText = "Test Title";
        private const string AlertButton = "#alertButton";
        private const string AlertText = "Test alert box";
        private const string TextInput = "#textInput";
        private const string Select = "#select";
        private const int NumberOfDivs = 4;
        private const string GreenRgbaCode = "rgba(51, 136, 51, 1)";

        protected HarnessTest(HarnessTestFixture fixture) => _fixture = fixture;

        [Fact]
        public void SanityTest()
        {
            _fixture.Act.GoToUrl("http://www.example.com/");
            _fixture.Assert.Displayed("body");
        }

        [Fact]
        public async Task SimpleExampleTest()
        {
            await ArrangeAsync();

            _fixture.Act.Click(ToggleCheckbox);
            _fixture.Assert.Disabled(ToggleButton);
            _fixture.Act.Click(ToggleCheckbox);
            _fixture.Assert.Enabled(ToggleButton);
            _fixture.Act.Click(ToggleCheckbox);
            _fixture.Assert.Disabled(ToggleButton);
        }


        [Fact]
        public void Displayed()
        {
            try
            {
                _fixture.Assert.Displayed(TestDiv);
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void DisplayedException()
        {
            const string cssSelectorString = "a > div > that > does > not > exist";
            try
            {
                _fixture.Assert.Displayed(cssSelectorString);
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == $"Displayed check failed for {cssSelectorString}");
            }
        }

        [Fact]
        public void NotDisplayed()
        {
            const string cssSelectorString = "a > div > that > does > not > exist";
            try
            {
                _fixture.Assert.NotDisplayed(cssSelectorString);
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void NotDisplayedException()
        {
            try
            {
                _fixture.Assert.NotDisplayed(TestDiv);
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == $"NotDisplayed check failed for {TestDiv}");
            }
        }

        [Fact]
        public void Enabled()
        {
            try
            {
                _fixture.Assert.Enabled(EnabledButton);
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void EnabledException()
        {
            try
            {
                _fixture.Assert.Enabled(DisabledButton);
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == $"Enabled check failed for {DisabledButton}");
            }
        }

        [Fact]
        public void Disabled()
        {
            try
            {
                _fixture.Assert.Disabled(DisabledButton);
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void DisabledException()
        {
            try
            {
                _fixture.Assert.Disabled(EnabledButton);
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == $"Disabled check failed for {EnabledButton}");
            }
        }

        [Fact]
        public void Class()
        {
            try
            {
                _fixture.Assert.Class(ClassDiv, ClassDivClass);
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void ClassException()
        {
            const string className = "classNotPresent";
            try
            {
                _fixture.Assert.Class(ClassDiv, className);
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == $"{className} is not present on {ClassDiv}");
            }
        }

        [Fact]
        public void NoClass()
        {
            const string className = "classNotPresent";
            try
            {
                _fixture.Assert.NoClass(ClassDiv, className);
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void NoClassException()
        {
            try
            {
                _fixture.Assert.NoClass(ClassDiv, ClassDivClass);
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == $"{ClassDivClass} is present on {ClassDiv}");
            }
        }

        [Fact]
        public void Click()
        {
            try
            {
                _fixture.Act.Click(EnabledButton);
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public async void CheckUncheck()
        {
            await ArrangeAsync();

            _fixture.Act.Uncheck(ToggleCheckbox);
            _fixture.Assert.Disabled(ToggleButton);

            _fixture.Act.Check(ToggleCheckbox);
            _fixture.Assert.Enabled(ToggleButton);

            _fixture.Act.Uncheck(ToggleCheckbox);
            _fixture.Assert.Disabled(ToggleButton);

            _fixture.Act.Uncheck(ToggleCheckbox);
            _fixture.Assert.Disabled(ToggleButton);

            _fixture.Act.Check(ToggleCheckbox);
            _fixture.Assert.Enabled(ToggleButton);

            _fixture.Act.Check(ToggleCheckbox);
            _fixture.Assert.Enabled(ToggleButton);
        }

        [Fact]
        public void ClickException()
        {
            const string cssSelectorString = "button > that > does > not > exist";
            try
            {
                _fixture.Act.Click(cssSelectorString);
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == $"Timed out trying to click {cssSelectorString}");
            }
        }

        [Fact]
        public void DoubleClick()
        {
            try
            {
                _fixture.Act.DoubleClick(EnabledButton);
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void DoubleClickException()
        {
            const string cssSelectorString = "button > that > does > not > exist";
            try
            {
                _fixture.Act.DoubleClick(cssSelectorString);
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == $"Timed out trying to double click {cssSelectorString}");
            }
        }

        [Fact]
        public void ValueEquals()
        {
            try
            {
                _fixture.Assert.ValueEquals(TestDiv, TestDivText);
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void ValueEqualsException()
        {
            const string expectedValue = "Bad Text";
            try
            {
                _fixture.Assert.ValueEquals(TestDiv, expectedValue);
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == $"Value comparison failed for {TestDiv}. Expected: {expectedValue}, Got: {TestDivText}");
            }
        }

        [Fact]
        public void ValueMatches()
        {
            var value = new Regex(TestDivText);
            try
            {
                _fixture.Assert.ValueMatches(TestDiv, value);
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void ValueMatchesException()
        {
            var expectedValue = new Regex("Bad Text");
            try
            {
                _fixture.Assert.ValueMatches(TestDiv, expectedValue);
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == $"Value comparison failed for {TestDiv}. Expected: {expectedValue}, Got: {TestDivText}");
            }
        }

        [Fact]
        public void ValueNotEquals()
        {
            try
            {
                _fixture.Assert.ValueNotEquals(TestDiv, "Not the correct value");
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void ValueNotEqualsException()
        {
            try
            {
                _fixture.Assert.ValueNotEquals(TestDiv, TestDivText);
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == $"Value not equals comparison failed for {TestDiv}. Expected anything other than: {TestDivText}");
            }
        }

        [Fact]
        public void UrlEquals()
        {
            _fixture.Act.GoToUrl(TestHtmlFile);
            var currentUri = _fixture.Harness.GetCurrentUri();
            try
            {
                _fixture.Assert.UrlEquals(currentUri);
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void UrlEqualsException()
        {
            var falseUrl = $"{TestHtmlFile}/123";
            var currentUri = _fixture.Harness.GetCurrentUri();
            try
            {
                _fixture.Assert.UrlEquals(falseUrl);
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == $"Url comparison failed. Expected: {falseUrl}, Got: {currentUri}");
            }
        }

        [Fact]
        public void UrlNotEquals()
        {
            _fixture.Act.GoToUrl(TestHtmlFile);
            var nonCurrentUri = "https://google.com";
            try
            {
                _fixture.Assert.UrlNotEquals(nonCurrentUri);
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void UrlNotEqualsException()
        {
            _fixture.Act.GoToUrl(TestHtmlFile);
            try
            {
                _fixture.Assert.UrlNotEquals(_fixture.Harness.GetCurrentUri());
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == $"Url comparison failed. Expected: {_fixture.Harness.GetCurrentUri()}, Got: {_fixture.Harness.GetCurrentUri()}");
            }
        }

        [Fact]
        public void UrlMatches()
        {
            _fixture.Act.GoToUrl($"{TestHtmlFile}/123");
            var currentUri = _fixture.Harness.GetCurrentUri();
            try
            {
                _fixture.Assert.UrlMatches(new Regex(currentUri));
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void UrlMatchesException()
        {
            var falseUri = $"{_fixture.Harness.GetCurrentUri()}/123";
            var currentUri = _fixture.Harness.GetCurrentUri();
            try
            {
                _fixture.Assert.UrlMatches(new Regex(falseUri));
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == $"Url comparison failed. Expected: {falseUri}, Got: {currentUri}");
            }
        }

        [Fact]
        public void UrlNotMatches()
        {
            var falseUri = "https://google.com";
            try
            {
                _fixture.Assert.UrlNotMatches(new Regex(falseUri));
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void UrlNotMatchesException()
        {
            _fixture.Act.GoToUrl($"{TestHtmlFile}/123");
            var currentUri = _fixture.Harness.GetCurrentUri();
            try
            {
                _fixture.Assert.UrlNotMatches(new Regex(_fixture.Harness.GetCurrentUri()));
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == $"Url comparison failed. Expected: {currentUri}, Got: {currentUri}");
            }
        }

        [Fact]
        public void TitleEquals()
        {
            try
            {
                _fixture.Assert.TitleEquals(TestTitleText);
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void TitleEqualsException()
        {
            const string badTestTitle = "Bad Test Title";
            try
            {
                _fixture.Assert.TitleEquals(badTestTitle);
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == $"Title comparison failed. Expected: {badTestTitle}, Got: {TestTitleText}");
            }
        }

        [Fact]
        public void TitleMatches()
        {
            try
            {
                _fixture.Assert.TitleMatches(new Regex(TestTitleText));
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void TitleMatchesException()
        {
            var badTestTitle = new Regex("Bad Test Title");
            try
            {
                _fixture.Assert.TitleMatches(new Regex("Bad Test Title"));
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == $"Title comparison failed. Expected: {badTestTitle}, Got: {TestTitleText}");
            }
        }

        [Fact]
        public void AlertEquals()
        {
            _fixture.Act.Click(AlertButton);
            try
            {
                _fixture.Assert.AlertEquals(AlertText);
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void AlertEqualsException()
        {
            _fixture.Act.Click(AlertButton);
            const string alertText = "bad Test alert box";
            try
            {
                _fixture.Assert.AlertEquals(alertText);
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == $"Alert value comparison failed. Expected: {alertText}, Got: {AlertText}");
            }
        }

        [Fact]
        public void Write()
        {
            const string textToWrite = "Text 123";
            try
            {
                _fixture.Act.Write(TextInput, textToWrite);
                _fixture.Assert.ValueEquals(TextInput, textToWrite);
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void WriteException()
        {
            const string textToWrite = "Text 123";
            const string badInput = "#badInput";
            try
            {
                _fixture.Act.Write(badInput, textToWrite);
                _fixture.Assert.ValueEquals("#textInput", textToWrite);
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == $"Timed out trying to write '{textToWrite}' to {badInput}");
            }
        }

        [Fact]
        public void GoToUrl()
        {
            var startingUri = _fixture.Harness.GetCurrentUri();
            try
            {
                _fixture.Assert.UrlEquals(startingUri);
                _fixture.Act.GoToUrl("https://google.com");
                _fixture.Assert.UrlNotEquals(startingUri);
                _fixture.Act.GoToUrl(TestHtmlFile);
                _fixture.Assert.UrlEquals(startingUri);
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void GetNumberOfOccurrences()
        {
            try
            {
                var numberOfDivs = _fixture.Act.GetNumberOfOccurrences("div");
                Assert.True(numberOfDivs == NumberOfDivs);
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void NewTab()
        {
            var tabCount = _fixture.Act.NewTab();
            Assert.True(tabCount == 2);
        }

        [Fact]
        public void SwitchToTab()
        {
            var startingTab = _fixture.Act.CurrentTab;
            _fixture.Act.NewTab();
            Assert.True(startingTab != _fixture.Act.CurrentTab);
            _fixture.Act.SwitchToTab(startingTab);
            Assert.True(startingTab == _fixture.Act.CurrentTab);
        }

        [Fact]
        public void SwitchToTabException()
        {
            const int tabToSwitchTo = 2;
            try
            {
                _fixture.Act.SwitchToTab(tabToSwitchTo);
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == $"Failed to switch to tab {tabToSwitchTo}");
            }
        }

        [Theory, InlineData(null), InlineData(1)]
        public void CloseTab(int? specifcTab)
        {
            _fixture.Act.NewTab();
            if (specifcTab != null)
            {
                _fixture.Act.CloseTab(specifcTab);

                try
                {
                    _fixture.Act.SwitchToTab(2);
                }
                catch (Exception ex)
                {
                    Assert.True(true, ex.Message);
                }
            }
            else
            {
                _fixture.Act.CloseTab();
                try
                {
                    _fixture.Act.SwitchToTab(2);
                }
                catch (Exception ex)
                {
                    Assert.True(true, ex.Message);
                }
            }
        }

        [Theory, InlineData(null), InlineData(2)]
        public void CloseOtherTabs(int? tabToNotClose)
        {
            for (var i = 0; i < 2; i++)
            {
                _fixture.Act.NewTab();
            }
            if (tabToNotClose != null)
            {
                _fixture.Act.CloseOtherTabs(tabToNotClose);
            }
            else
            {
                _fixture.Act.CloseOtherTabs();
            }

            try
            {
                _fixture.Act.SwitchToTab(2);
            }
            catch (HarnessSwitchTabFailedException ex)
            {
                Assert.True(true, ex.Message);
            }
        }

        [Fact]
        public void DragAndDrop()
        {
            try
            {
                _fixture.Act.DragAndDrop(DraggableObject, DropArea);
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void DragAndDropException()
        {
            const string dropArea = "#badDropArea";
            try
            {
                _fixture.Act.DragAndDrop(DraggableObject, dropArea);
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == $"Timed out trying to drag {DraggableObject} to {dropArea}");
            }
        }

        [Fact]
        public void SelectElementFromDropDownByValue()
        {
            try
            {
                _fixture.Assert.ValueEquals(Select, "Option 1");
                _fixture.Act.SelectElementFromDropDownByValue(Select, "2");
                _fixture.Assert.ValueEquals(Select, "Option 2");
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void SelectElementFromDropDownByValueException()
        {
            const string value = "5";
            try
            {
                _fixture.Assert.ValueEquals(Select, "Option 1");
                _fixture.Act.SelectElementFromDropDownByValue(Select, value);
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == $"Failed to select value {value} from {Select}");
            }
        }

        [Fact]
        public void SelectElementFromDropDownByText()
        {
            try
            {
                _fixture.Assert.ValueEquals(Select, "Option 1");
                _fixture.Act.SelectElementFromDropDownByText(Select, "Option 2");
                _fixture.Assert.ValueEquals(Select, "Option 2");
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void SelectElementFromDropDownByTextException()
        {
            const string text = "Optionsdfsdfsdfsdfsdfsdfdfdfdf";
            try
            {
                _fixture.Assert.ValueEquals(Select, "Option 1");
                _fixture.Act.SelectElementFromDropDownByText(Select, text);
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == $"Failed to select text {text} from {Select}");
            }
        }

        [Fact]
        public void SelectElementFromDropDownByIndex()
        {
            try
            {
                _fixture.Assert.ValueEquals(Select, "Option 1");
                _fixture.Act.SelectElementFromDropDownByIndex(Select, 1);
                _fixture.Assert.ValueEquals(Select, "Option 2");
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void SelectElementFromDropDownByIndexException()
        {
            const int index = 105;
            try
            {
                _fixture.Assert.ValueEquals(Select, "Option 1");
                _fixture.Act.SelectElementFromDropDownByIndex(Select, index);
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == $"Failed to select index {index} from {Select}");
            }
        }

        [Fact]
        public void Hover()
        {
            try
            {
                Assert.False(_fixture.Act.Elements(EnabledButton).First().GetCssValue("background-color") == GreenRgbaCode);
                _fixture.Act.Hover(EnabledButton);
                Assert.True(_fixture.Act.Elements(EnabledButton).First().GetCssValue("background-color") == GreenRgbaCode);
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void HoverException()
        {
            try
            {
                _fixture.Act.Hover("NotAButton");
                Assert.True(_fixture.Act.Elements("NotAButton").First().GetCssValue("background-color") == GreenRgbaCode);
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == "Failed to Hover over element NotAButton");
            }
        }

        [Fact]
        public void GetAttributeValue()
        {
            try
            {
                Assert.True(_fixture.Act.GetAttributeValue(TestDiv, "data-value") == "5");
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void GetAttributeValueException()
        {
            try
            {
                _fixture.Act.GetAttributeValue(TestDiv, "bad-attribute");
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message == $"Attribute: bad-attribute, was not found on {TestDiv}");
            }
        }

        public void Dispose() => _fixture.Harness.Quit();

        private Task ArrangeAsync() => Task.Delay(750); // Emulate I/O Bound arrangement... i.e.; Database (or network) I/O. 
    }
}