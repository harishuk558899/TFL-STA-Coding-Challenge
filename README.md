# STA Coding Challenge – TfL Journey Planner

## Purpose

This repository contains a C# UI automation framework for the TfL Journey Planner challenge supplied for the Senior Test Analyst selection process.

The assignment asks for:
- C# tests
- Gherkin syntax
- Playwright and ChromeDriver/.NET UI automation libraries
- At least five automated scenarios
- A README explaining development decisions
- Additional functional and non-functional scenarios
- An AI-use disclaimer if AI was used

The live system under test is:
https://tfl.gov.uk/plan-a-journey

## Framework

- .NET 8
- NUnit
- Reqnroll for Gherkin/BDD
- Microsoft Playwright for the main UI automation
- Selenium WebDriver / ChromeDriver for a dedicated browser smoke check
- Page Object Model for maintainability

## Project structure

```text
TfL-STA-Coding-Challenge/
├── Configuration/
│   └── TestSettings.cs
├── Features/
│   └── JourneyPlanner.feature
├── Hooks/
│   └── TestHooks.cs
├── Pages/
│   └── JourneyPlannerPage.cs
├── Steps/
│   └── JourneyPlannerSteps.cs
├── Tests/
│   └── ChromeDriverSmokeTests.cs
├── docs/
│   └── additional-scenarios.md
├── .gitignore
├── README.md
├── TfLJourneyPlanner.Tests.csproj
└── reqnroll.json
```

## Prerequisites

Install:
1. .NET 8 SDK
2. Visual Studio Code
3. C# Dev Kit extension (recommended)
4. PowerShell 7 or Windows PowerShell
5. Google Chrome

## Open in VS Code

1. Extract the ZIP.
2. Open VS Code.
3. Select **File → Open Folder**.
4. Select the extracted `TfL-STA-Coding-Challenge` folder.
5. Open the integrated terminal.

## Restore packages

```powershell
dotnet restore
```

## Build

```powershell
dotnet build
```

## Install Playwright Chromium

After the first successful build, install the browser used by Playwright:

```powershell
pwsh bin/Debug/net8.0/playwright.ps1 install chromium
```

If `pwsh` is not available on Windows, use Windows PowerShell:

```powershell
powershell bin/Debug/net8.0/playwright.ps1 install chromium
```

## Run all tests

```powershell
dotnet test
```

## Run only the challenge tests

```powershell
dotnet test --filter "TestCategory=JourneyPlanner"
```

## Headless run and HTML report

Run the test script from PowerShell to restore, build, ensure Playwright Chromium is installed, and run headless tests:

```powershell
pwsh ./run-tests.ps1
```

To run only the challenge scenarios:

```powershell
pwsh ./run-tests.ps1 -TestFilter "TestCategory=JourneyPlanner"
```

The script writes an HTML summary to `TestResults/journey-tests.html` and the raw test results to `TestResults/journey-tests.trx`. Failed scenarios also save a screenshot and rendered DOM under `bin/Debug/net8.0/artifacts/failures/`.

## Run the ChromeDriver smoke test

```powershell
dotnet test --filter "TestCategory=ChromeDriverSmoke"
```

## Important: live website

These tests execute against the public TfL website. Live journey times, service disruptions, cookies, accessibility information, and page markup can change. The tests therefore avoid asserting one fixed journey duration.

The assignment specifically asks autocomplete values to be selected rather than entering the complete text. The page object types the station name to trigger autocomplete and then clicks the matching suggestion.

Selectors are centralised in `Pages/JourneyPlannerPage.cs` so they can be updated in one place if TfL changes its HTML.

## Assignment coverage

### Scenario 1
Valid journey:
- Leicester Square Underground Station
- Covent Garden Underground Station
- Validate walking and cycling options/times

### Scenario 2
After planning:
- Edit preferences
- Select routes with least walking
- Update journey
- Validate journey time

### Scenario 3
- View Details
- Verify Covent Garden access information

### Scenario 4
- Invalid location
- Verify a valid journey is not returned

### Scenario 5
- No locations
- Verify journey cannot be planned

Additional functional and non-functional scenarios are in `docs/additional-scenarios.md`.

## Design decisions

### Page Object Model
Browser interactions are kept in `JourneyPlannerPage`. Step definitions describe behaviour rather than implementation details.

### Explicit waits
Playwright's locator-based auto-waiting is preferred over arbitrary sleeps. A small wait is used only after a navigation/update where the live site may refresh result content.

### Dynamic assertions
The test checks that journey information and time patterns are present instead of hard-coding a live travel time that can change.

### Test isolation
A fresh Playwright browser/page is created for each scenario through Reqnroll hooks.

### Failure diagnostics
On scenario failure, a screenshot is saved under `artifacts/screenshots` to make debugging easier.
