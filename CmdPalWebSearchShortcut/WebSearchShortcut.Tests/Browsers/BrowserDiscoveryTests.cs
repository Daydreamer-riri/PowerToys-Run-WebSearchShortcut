using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebSearchShortcut.Browsers;

namespace WebSearchShortcut.Tests.Helpers
{
  [TestClass]
  public class BrowserProgIdFinderTests
  {
    [TestMethod]
    public void FindUniqueHttpUrlAssociationProgIdsShouldPrintResults()
    {
      var browerInfos = BrowserDiscovery.GetAllInstalledBrowsers();

      foreach (var browerInfo in browerInfos)
      {
        Console.WriteLine($"Found brower: {browerInfo}");
      }

      Console.WriteLine($"Total browers: {browerInfos.Count}");
    }
  }
}
