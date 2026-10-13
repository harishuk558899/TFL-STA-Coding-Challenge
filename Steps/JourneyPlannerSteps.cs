using NUnit.Framework;
using Reqnroll;
using TfLJourneyPlanner.Tests.Hooks;
using TfLJourneyPlanner.Tests.Pages;

namespace TfLJourneyPlanner.Tests.Steps;

[Binding]
public sealed class JourneyPlannerSteps
{
    private readonly TestHooks _hooks;
    private JourneyPlannerPage Page => new(_hooks.Page!);
    public JourneyPlannerSteps(TestHooks hooks) => _hooks = hooks;

    [Given("I open the TfL Journey Planner")]
    public async Task GivenIOpenTheTfLJourneyPlanner() => await Page.OpenAsync();

    [When("I select {string} as the starting location")]
    public async Task WhenISelectStartingLocation(string location) => await Page.SelectAutocompleteLocationAsync("From", location);

    [When("I select {string} as the destination")]
    public async Task WhenISelectDestination(string location) => await Page.SelectAutocompleteLocationAsync("To", location);

    [When("I submit the journey planner")]
    public async Task WhenISubmitTheJourneyPlanner() => await Page.PlanJourneyAsync();

    [Then("journey results should be displayed")]
    public async Task ThenJourneyResultsShouldBeDisplayed() => Assert.That(await Page.HasJourneyResultsAsync(), Is.True);

    [Then("walking and cycling journey times should be displayed")]
    public async Task ThenWalkingAndCyclingTimesShouldBeDisplayed() => Assert.That(await Page.HasWalkingAndCyclingTimesAsync(), Is.True);

    [When("I plan a journey from {string} to {string}")]
    public async Task WhenIPlanAJourney(string from, string to)
    {
        await Page.SelectAutocompleteLocationAsync("From", from);
        await Page.SelectAutocompleteLocationAsync("To", to);
        await Page.PlanJourneyAsync();
    }

    [When("I edit preferences to select routes with least walking")]
    public async Task WhenIEditPreferencesToLeastWalking() => await Page.EditLeastWalkingAndUpdateAsync();

    [Then("the updated journey should show a journey time")]
    public async Task ThenTheUpdatedJourneyShouldShowAJourneyTime() => Assert.That(await Page.HasJourneyTimeAsync(), Is.True);

    [When("I open the first journey details")]
    public async Task WhenIOpenTheFirstJourneyDetails() => await Page.OpenFirstViewDetailsAsync();

    [Then("complete access information for Covent Garden Underground Station should be displayed")]
    public async Task ThenCompleteAccessInformationShouldBeDisplayed() => Assert.That(await Page.HasCoventGardenAccessInformationAsync(), Is.True);

    [When("I enter an invalid location and submit the journey")]
    public async Task WhenIEnterAnInvalidLocationAndSubmit() => await Page.EnterInvalidLocationAsync();

    [When("I submit the journey planner without entering locations")]
    public async Task WhenISubmitWithoutLocations() => await Page.SubmitEmptyJourneyAsync();

    [Then("no valid journey results should be displayed")]
    public async Task ThenNoValidJourneyResultsShouldBeDisplayed() => Assert.That(await Page.HasInvalidOrEmptyValidationAsync(), Is.True);

    [Then("the journey should not be planned")]
    public async Task ThenTheJourneyShouldNotBePlanned() => Assert.That(await Page.HasInvalidOrEmptyValidationAsync(), Is.True);
}
