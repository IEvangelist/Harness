using System;
using OpenQA.Selenium;

namespace IEvangelist.Harness.Interfaces
{
    public interface IHarness
    {
        Configuration Configuration { get; }

        IWebDriver CurrentDriver { get; }

        IWebDriver StartChrome();

        IWebDriver StartInternetExplorer();

        IWebDriver StartFireFox();

        string GetCurrentUri();

        void Quit();

        T WaitUntil<T>(int timeout, Func<T> function);

        bool WaitUntil(int timeout, Func<bool> function);
    }
}