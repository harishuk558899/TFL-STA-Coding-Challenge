using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace TfLJourneyPlanner.Tests.Tests;

[TestFixture]
public sealed class ChromeDriverSmokeTests
{
    [Test, Category("ChromeDriverSmoke")]
    public void ChromeDriverCanOpenTfLJourneyPlanner()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless=new");
        options.AddArgument("--window-size=1440,1000");
        using var driver = new ChromeDriver(options);
        driver.Navigate().GoToUrl("https://tfl.gov.uk/plan-a-journey");
        Assert.That(driver.Title, Does.Contain("Transport for London").Or.Contain("Plan a journey"));
        Assert.That(driver.Url, Does.Contain("tfl.gov.uk"));
    }
}
