Feature: TfL Journey Planner
  As a passenger
  I want to use the TfL Journey Planner
  So that I can plan journeys and understand route/access information

  @JourneyPlanner
  Scenario: Plan a valid journey and validate walking and cycling times
    Given I open the TfL Journey Planner
    When I select "Leicester Square Underground Station" as the starting location
    And I select "Covent Garden Underground Station" as the destination
    And I submit the journey planner
    Then journey results should be displayed
    And walking and cycling journey times should be displayed

  @JourneyPlanner @Preferences
  Scenario: Select routes with least walking and validate journey time
    Given I open the TfL Journey Planner
    When I plan a journey from "Leicester Square Underground Station" to "Covent Garden Underground Station"
    And I edit preferences to select routes with least walking
    Then the updated journey should show a journey time

  @JourneyPlanner @Accessibility
  Scenario: View Covent Garden station access information
    Given I open the TfL Journey Planner
    When I plan a journey from "King's Cross St. Pancras Underground Station" to "Covent Garden Underground Station"
    And I open the first journey details
    Then complete access information for Covent Garden Underground Station should be displayed

  @JourneyPlanner @Negative
  Scenario: Invalid location does not return a valid journey
    Given I open the TfL Journey Planner
    When I enter an invalid location and submit the journey
    Then no valid journey results should be displayed

  @JourneyPlanner @Negative
  Scenario: Journey cannot be planned when no locations are entered
    Given I open the TfL Journey Planner
    When I submit the journey planner without entering locations
    Then the journey should not be planned
