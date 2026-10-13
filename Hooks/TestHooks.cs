using Microsoft.Playwright;
using Reqnroll;
using TfLJourneyPlanner.Tests.Configuration;

namespace TfLJourneyPlanner.Tests.Hooks;

[Binding]
public sealed class TestHooks
{
    private readonly ScenarioContext _scenarioContext;
    public IPlaywright? Playwright { get; private set; }
    public IBrowser? Browser { get; private set; }
    public IPage? Page { get; private set; }

    public TestHooks(ScenarioContext scenarioContext) => _scenarioContext = scenarioContext;

    [BeforeScenario]
    public async Task StartBrowser()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = TestSettings.Headless });
        Page = await Browser.NewPageAsync(new BrowserNewPageOptions { ViewportSize = new ViewportSize { Width = 1440, Height = 1000 } });
        await Page.RouteAsync("**/translate.google.com/**", route => route.FulfillAsync(new() { Status = 200, ContentType = "application/javascript", Body = string.Empty }));
        await Page.RouteAsync("**/translate.googleapis.com/**", route => route.FulfillAsync(new() { Status = 200, ContentType = "application/javascript", Body = string.Empty }));
        Page.Console += (_, message) =>
        {
            if (message.Type is "warning" or "error") Console.WriteLine($"[browser:{message.Type}] {message.Text}");
        };
        Page.PageError += (_, error) => Console.WriteLine($"[browser:error] {error}");
        Page.RequestFailed += (_, request) => Console.WriteLine($"[request:failed] {request.Method} {request.Url}: {request.Failure}");
    }

    [AfterScenario]
    public async Task StopBrowser()
    {
        if (_scenarioContext.TestError is not null && Page is not null)
        {
            var safeName = string.Join("_", _scenarioContext.ScenarioInfo.Title.Split(Path.GetInvalidFileNameChars()));
            var artifactDirectory = Path.Combine(AppContext.BaseDirectory, "artifacts", "failures");
            Directory.CreateDirectory(artifactDirectory);
            var screenshotPath = Path.Combine(artifactDirectory, $"{safeName}.png");
            var htmlPath = Path.Combine(artifactDirectory, $"{safeName}.html");
            await Page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath, FullPage = true });
            await File.WriteAllTextAsync(htmlPath, await Page.ContentAsync());
            Console.WriteLine($"[scenario:failed] {_scenarioContext.ScenarioInfo.Title}: {_scenarioContext.TestError.Message}");
            Console.WriteLine($"[diagnostics] URL: {Page.Url}; screenshot: {screenshotPath}; DOM: {htmlPath}");
        }
        if (Browser is not null) await Browser.CloseAsync();
        Playwright?.Dispose();
    }
}
