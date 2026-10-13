using Microsoft.Playwright;
using System.Text.RegularExpressions;
using TfLJourneyPlanner.Tests.Configuration;

namespace TfLJourneyPlanner.Tests.Pages;

public sealed class JourneyPlannerPage
{
    private readonly IPage _page;
    public JourneyPlannerPage(IPage page) => _page = page;

    private ILocator FromInput => _page.Locator("#InputFrom");
    private ILocator ToInput => _page.Locator("#InputTo");

    public async Task OpenAsync()
    {
        IResponse? response = null;
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            response = await _page.GotoAsync(TestSettings.BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 45000 });
            var title = await _page.TitleAsync();
            var body = await _page.Locator("body").InnerTextAsync();
            if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(body))
            {
                if (attempt < 3)
                {
                    Console.WriteLine($"[navigation:retry] TfL returned an empty page (attempt {attempt}/3, HTTP {response?.Status}).");
                    continue;
                }

                throw new InvalidOperationException($"TfL returned an empty page after {attempt} attempts. HTTP {response?.Status}; URL: {_page.Url}.");
            }

            await AcceptCookiesIfVisibleAsync();
            try
            {
                await FromInput.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 30000 });
                return;
            }
            catch (Exception error)
            {
                var summary = body.Length > 500 ? body[..500] : body;
                throw new InvalidOperationException($"Planner did not become visible. HTTP {response?.Status}; URL: {_page.Url}; title: {title}; body: {summary}; cause: {error.Message}", error);
            }
        }
    }

    private async Task AcceptCookiesIfVisibleAsync()
    {
        var acceptEssentialButton = _page.Locator("#CybotCookiebotDialogBodyButtonDecline");
        if (await acceptEssentialButton.CountAsync() > 0 && await acceptEssentialButton.IsVisibleAsync())
        {
            await acceptEssentialButton.ClickAsync(new() { Timeout = 5000 });
            return;
        }

        await _page.EvaluateAsync("""
            () => {
                const overlay = document.querySelector('#cb-cookieoverlay');
                const banner = document.querySelector('#cb-cookiebanner');
                if (overlay?.getAttribute('aria-hidden') === 'true' && banner && getComputedStyle(banner).display === 'none') {
                    overlay.style.pointerEvents = 'none';
                }
            }
            """);
    }

    public async Task SelectAutocompleteLocationAsync(string field, string location)
    {
        var input = field.Equals("From", StringComparison.OrdinalIgnoreCase) ? FromInput : ToInput;
        await input.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await AcceptCookiesIfVisibleAsync();
        await input.ClickAsync();
        await input.FillAsync(location);

        var suggestionLocator = _page.Locator("li[role='option'], [role='option'], [data-testid*='autocomplete'], .suggestion, .result-item, .location-item").Filter(new() { HasText = location }).First;
        await suggestionLocator.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 20000 });
        await suggestionLocator.ClickAsync();

        await input.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15000 });
    }

    public async Task PlanJourneyAsync()
    {
        await AcceptCookiesIfVisibleAsync();
        var button = _page.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("plan (my )?journey|plan journey|search journey|get journey", RegexOptions.IgnoreCase) }).First;
        if (await button.CountAsync() == 0)
        {
            button = _page.GetByText("Plan my journey", new() { Exact = false }).First;
        }

        await button.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await button.ClickAsync();
    }

    private async Task<string> BodyTextAsync() => (await _page.Locator("body").InnerTextAsync()).ToLowerInvariant();

    public async Task<bool> HasJourneyResultsAsync()
    {
        try
        {
            await _page.GetByText("Walking and cycling", new() { Exact = false }).WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15000 });
        }
        catch (TimeoutException)
        {
            return false;
        }

        var body = await BodyTextAsync();
        var hasDuration = Regex.IsMatch(body, @"\b\d+\s*(mins?|minutes|hrs?|hours)\b");
        var routeSpecificSignals = body.Contains("journey results") || body.Contains("your journey") || body.Contains("route details") || body.Contains("walking") || body.Contains("cycling") || body.Contains("least walking") || body.Contains("fastest");
        return routeSpecificSignals && hasDuration;
    }

    public async Task<bool> HasWalkingAndCyclingTimesAsync()
    {
        var body = await BodyTextAsync();
        var walking = body.Contains("walking") && Regex.IsMatch(body, @"walking[\s\S]{0,250}\b\d+\s*(mins?|minutes|hrs?|hours)\b");
        var cycling = body.Contains("cycling") && Regex.IsMatch(body, @"cycling[\s\S]{0,250}\b\d+\s*(mins?|minutes|hrs?|hours)\b");
        return walking && cycling;
    }

    public async Task EditLeastWalkingAndUpdateAsync()
    {
        var preferenceToggle = _page.Locator("button.toggle-options.more-options");
        if (await preferenceToggle.GetAttributeAsync("aria-expanded") != "true")
        {
            await preferenceToggle.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
            await preferenceToggle.ClickAsync();
        }

        var preferenceOption = _page.Locator("label[for='JourneyPreference_2']");
        await preferenceOption.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await preferenceOption.ClickAsync();

        var update = _page.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("update journey|update", RegexOptions.IgnoreCase) }).First;
        await update.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await update.ClickAsync();
    }

    public async Task<bool> HasJourneyTimeAsync()
    {
        try
        {
            await _page.WaitForFunctionAsync(
                "() => document.body !== null && /\\b\\d+\\s*(mins?|minutes|hrs?|hours)\\b/i.test(document.body.innerText)",
                null,
                new() { Timeout = 15000 });
        }
        catch (TimeoutException)
        {
            return false;
        }

        return Regex.IsMatch(await BodyTextAsync(), @"\b\d+\s*(mins?|minutes|hrs?|hours)\b");
    }

    public async Task OpenFirstViewDetailsAsync()
    {
        var detailsButton = _page.Locator("button.show-detailed-results:visible").First;
        await detailsButton.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await detailsButton.ClickAsync();
    }

    public async Task<bool> HasCoventGardenAccessInformationAsync()
    {
        var body = await BodyTextAsync();
        return body.Contains("covent garden") && (body.Contains("accessibility") || body.Contains("access information") || body.Contains("step-free") || body.Contains("stairs") || body.Contains("lift") || body.Contains("escalator"));
    }

    public async Task EnterInvalidLocationAsync()
    {
        await FromInput.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15000 });
        await FromInput.FillAsync("ZZZ Invalid Location 987654");
        await PlanJourneyAsync();
    }

    public async Task SubmitEmptyJourneyAsync() => await PlanJourneyAsync();

    public async Task<bool> HasInvalidOrEmptyValidationAsync()
    {
        var body = await BodyTextAsync();
        var validation = body.Contains("enter") || body.Contains("select") || body.Contains("required") || body.Contains("invalid") || body.Contains("no results") || body.Contains("not found");
        var noJourneyResults = !await HasJourneyResultsAsync();
        return validation || noJourneyResults;
    }
}
