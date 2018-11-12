# Harness

### Status

[![NuGet version (IEvangelist.Harness)](https://img.shields.io/nuget/v/IEvangelist.Harness.svg?style=flat-square)](https://www.nuget.org/packages/IEvangelist.Harness/)
[![Build status](https://davidpine.visualstudio.com/IEvangelist.Harness/_apis/build/status/IEvangelist.Harness%20.NET%20Core-CI)](https://davidpine.visualstudio.com/IEvangelist.Harness/_build/latest?definitionId=3)

### Overview

Harness is a .Net based framework for interacting with Selenium.  It abstracts away the complication of using Selenium based methods and puts them into simple methods.  It uses timeouts to ensure that elements have time to become available or change state.

### Using Harness
##### Selectors
The methods of Harness uses a variety selectors to find the elements to interact with.
Here is a css selector for example:
```csharp
Act.Click("button");
```
This will click all buttons on the page.  If you only wanted to click the button with the btn-primary class then you would do it like so:
```csharp
Act.Click("button.btn-primary");
```
Harness supports these types of selectors:
* By Css
* By Value
* By Xpath
* By Text
* By Label

##### Timeouts
Harness utilizes timeouts to make sure that elements have time to render or for a database call to complete so that no Waits need to be done in the test code. 
These timeouts are set by these three Configuration properties:
* PageTimeout - Time that Harness will wait for a new page operation to complete such as opening a new tab 
* ElementTimeout - Time that Harness will wait for an element to respond to an Act
* CompareTimeout - Time that Harness will wait for an element to respond to an Assert


Here is an example of setting the timeout for an element that takes a long time to display before we can click it and a long time to render a large chart:
```csharp
Configuration.ElementTimeout = 60;
Act.Click("#generate-chart-button");

Configuration.CompareTimeout = 60;
Assert.Displayed("#chart");
```

##### IWebElement
Sometimes it is necessary to get the web elements directly, this requires using one of two methods.
```csharp
IEnumerable<IWebElement> Elements(string selector);
IWebElement Element(string selector);
```
This will get you an element or a list of elements that you can call Selenium methods on directly.
Refer to [Selenium Dotnet](https://seleniumhq.github.io/selenium/docs/api/dotnet/) documentation for more information on the api.

### Configuration

The Harness.Configuration class has many configuration settings used by Harness.  All properties have default settings and can be set in code by changing the property.

| Property | Default Value | Description |
|---|---|---|
| DriverDirectory | Application base directory | Path of where webdrivers exist |
| ScreenshotDirectory | %ProgramData%\CPO\FeatureTests | Path where to store screenshots and browser logs for failing tests |
| CaptureScreenshotOnFailure | true | Whether to capture screenshots for failing tests |
| CaptureBrowserLogsOnFailure | true | Whether to capture browser logs for failing tests |
| PageTimeout | 30 (seconds) | Time that Harness will wait for a new page operation to complete such as opening a new tab |
| ElementTimeout | 30 (seconds) | Time that Harness will wait for an element to respond to an Act |
| CompareTimeout | 30 (seconds) | Time that Harness will wait for an element to respond to an Assert |
| ThrowIfMoreThanOneElement | false | Harness will throw an exception if there is more than one element returned when getting an IWebElement |
| OptimizeByDisablingClearBeforeWrite | false |  Clear an elements text before writing to it |
| WriteToSelectWithOptionValue | true |  Write to a Select element when option has @value= |
| AnimationWaitTimeMilliseconds | 300 |  How long to wait in between click actions |
| SingleCharacterEntry | true |  Makes Selenium more reliable when writing characters to elements by writing them one at a time but is slower than writing all at once |
| RunChromeHeadless | true | Harness can run Chrome headless so chrome is less resource intensive by not having to render anything |

### Webdrivers

Harness Web Drivers are automatically downloaded into the application's base directory from webdriver dependencies (`<PackageReference Include="Selenium.WebDriver.<webdriver name here>" Version="..." />`).  Each driver has a strict range of browser versions they are compatible with.  If a new browser version is released and is incompatible with the current webdriver versions, please do the following:

- Ensure that your local browser is up-to-date
- Update the browser on the host machine
- Update the version of the specific dependency in the dependent projects
- Create pull requests with your changes

### Simple C# Usage Example
For this example we will setup a simple Harness test using Xunit.

First setup the class fixture for the test.  This fixture will get us a new object of Harness and will depose of it when the test finishes.
```csharp
    public class HarnessExampleFixture : IDisposable
    {
        public Harness.Harness TestHarness { get; }

        public HarnessExampleFixture()
        {
            TestHarness = new Harness.Harness();
        }

        public void Dispose()
        {
            TestHarness.Dispose();
        }
    }
```

Next lets setup the HarnessExample class which will have our tests.
```csharp
    //These tests will use chrome as their browser
    public class ChromeHarnessExample : HarnessExample
    {
        public ChromeHarnessExample(HarnessExampleFixture fixture) : base(fixture)
        {
            fixture.TestHarness.Configuration.RunChromeHeadless = false;
            fixture.TestHarness.StartChrome();
        }
    }

    public abstract class HarnessExample : IClassFixture<HarnessExampleFixture>
    {
        public IAct Act { get; }
        public IAssert Assert { get; }
        public Configuration Configuration { get; }

        //The constructor will setup the objects we will use to interact with Harness
        protected HarnessExample(HarnessExampleFixture fixture)
        {
            Act = fixture.TestHarness;
            Assert = fixture.TestHarness;
            Configuration = fixture.TestHarness.Configuration;
        }

        [Fact]
        public void Test()
        {
            //Act will control the browser
            Act.GoToUrl("http://www.example.com/");
            
            //Assert will verify test conditions based on the browsers current state
            Assert.Displayed("body");
        }
    }
```

The test above simply opens a new instance of chrome, browses to example.com, and then asserts that the css element "body" is visible.

##### Additional Examples
Refer to the code used to test Harness for more examples of how to use the methods. 

[Harness Test Class](HarnessTest.cs)

### Debugging Test Failures
If either `CaptureScreenshotOnFailure` or `CaptureBrowserLogsOnFailure` configuration options are enabled, Harness will capture information that can be useful to determine why tests failed.  Harness can store a screenshot of the browser tab at the point of failure and the console logs since the last navigation to `ScreenshotDirectory` with the following structure:
```
${ScreenshotDirectory}/${ClassName}/${TestName}/${Timestamp:yyyy-dd-MM-HH.mm.ss}/
  |- screen-shot.png
  \- browser-logs.txt
```
