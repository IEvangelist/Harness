using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using IEvangelist.Harness.Extensions;
using IEvangelist.Harness.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Extensions;

namespace IEvangelist.Harness
{
    public static class Wait
    {
        public static T Until<T>(int timeout,
                                 Func<T> function,
                                 IHarness harness = null)
            => Until(timeout, () => (TryExec(function, harness, out var result), result), harness);

        public static bool Until(int timeout,
                                 Func<bool> function,
                                 IHarness harness = null)
            => Until(timeout, () => (TryExec(function, harness, out var isSuccessful) && isSuccessful, isSuccessful), harness);

        private static T Until<T>(int timeout,
                                  Func<(bool isSuccessful, T)> function,
                                  IHarness harness = null)
        {
            var stopWatch = Stopwatch.StartNew();
            do
            {
                var (isSuccessful, result) = function();
                if (isSuccessful)
                {
                    return result;
                }

                Thread.Sleep(500);
            } while (timeout > stopWatch.Elapsed.TotalSeconds);

            TryTakeScreenShotAndCaptureLogs(harness);
            throw new WebDriverTimeoutException("Timed out!");
        }

        internal static void TryTakeScreenShotAndCaptureLogs(IHarness harness)
        {
            try
            {
                var driver = harness?.CurrentDriver;
                var config = harness?.Configuration;
                if (driver is null || config is null)
                {
                    return;
                }

                var contextDirectory = GetContextDirectory(harness, config);

                if (config.CaptureScreenshotOnFailure)
                {
                    var screenshot = driver.TakeScreenshot();
                    if (screenshot != null)
                    {
                        var imageFileName = Path.Combine(contextDirectory, "screen-shot.png");
                        screenshot.SaveAsFile(imageFileName, ScreenshotImageFormat.Png);
                    }
                }

                if (config.CaptureBrowserLogsOnFailure)
                {
                    TryCaptureAndSaveLogs(driver, contextDirectory);
                }
            }
            catch
            {
            }
        }

        private static string GetContextDirectory(IHarness harness, Configuration config)
        {
            var (className, testName) = harness.GetTestClassAndMethodNames();
            var dateFolder = Path.Combine(config.ScreenshotDirectory, 
                                          className ?? "UnknownTestClass", 
                                          testName ?? "UnknownTestName", 
                                          $"{DateTime.Now:yyyy-dd-MM-HH.mm.ss}");
            var contextDirectory = GetOrCreateTestDirectory(dateFolder);
            return contextDirectory;
        }

        private static string GetOrCreateTestDirectory(string screenshotDirectory)
        {
            try
            {
                var info = Directory.CreateDirectory(screenshotDirectory);
                return info.FullName;
            }
            catch
            {
                return Directory.GetCurrentDirectory();
            }
        }

        private static void TryCaptureAndSaveLogs(IWebDriver driver, string directory)
        {
            try
            {
                var browserLogs = driver?.Manage()
                                        ?.Logs
                                        ?.GetLog(LogType.Browser);
                if (browserLogs is null || browserLogs.Count == 0)
                {
                    return;
                }

                var builder = new StringBuilder();
                foreach (var log in browserLogs)
                {
                    builder.AppendLine($"{log.Timestamp}, [{log.Level}] :: {log.Message}");
                }

                File.WriteAllText(Path.Combine(directory, "browser-logs.txt"), builder.ToString());
            }
            catch
            {
            }
        }

        private static bool TryExec<T>(Func<T> predicate, IHarness harness, out T result)
        {
            try
            {
                result = predicate();
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                TryTakeScreenShotAndCaptureLogs(harness);
            }
            catch
            {
            }

            result = default;
            return false;
        }
    }
}