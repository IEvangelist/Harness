using IEvangelist.Harness;
using IEvangelist.Harness.Interfaces;

namespace IEvangelistDev.Harness.Test
{
    public class HarnessTestFixture
    {
        private readonly IEvangelist.Harness.Harness _testHarness;

        public IAct Act => _testHarness;

        public IAssert Assert => _testHarness;

        public IHarness Harness => _testHarness;

        public Configuration Configuration => _testHarness.Configuration;

        public HarnessTestFixture()
            => _testHarness = new IEvangelist.Harness.Harness(new Configuration
                              {
                                  CompareTimeout = 5,
                                  ElementTimeout = 5,
                                  RunChromeHeadless = false
                              });
    }
}