using System;
using System.IO;

namespace IEvangelist.Harness
{
    public class Configuration
    {
        public string DriverDirectory { get; set; } = Environment.ExpandEnvironmentVariables($@"{AppDomain.CurrentDomain.BaseDirectory}");

        public string ScreenshotDirectory { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"CPO\FeatureTests");

        public bool CaptureScreenshotOnFailure { get; set; } = true;

        public bool CaptureBrowserLogsOnFailure { get; set; } = true;

        public int PageTimeout { get; set; } = 30;

        public int ElementTimeout { get; set; } = 30;

        public int CompareTimeout { get; set; } = 30;

        public bool ThrowIfMoreThanOneElement { get; set; } = false;

        public bool OptimizeByDisablingClearBeforeWrite { get; set; } = false;

        public bool WriteToSelectWithOptionValue { get; set; } = true;

        public int AnimationWaitTimeMilliseconds { get; set; } = 300;

        public bool SingleCharacterEntry { get; set; } = true;

        public bool RunChromeHeadless { get; set; } = true;
    }
}