# Additional scenarios requested by the assignment

These are intentionally listed rather than automated, as requested.

## Functional scenarios

1. Verify autocomplete suggestions are displayed after entering a partial station/place name.
2. Verify a user can select an autocomplete suggestion using the keyboard only.
3. Verify origin and destination cannot be left as free text when an autocomplete selection is required.
4. Verify the same origin and destination produces an appropriate validation message.
5. Verify changing departure/arrival time updates the journey results.
6. Verify fastest route, fewest changes and least walking preferences produce appropriate results.
7. Verify individual transport modes can be selected/deselected.
8. Verify step-free accessibility options affect the returned journey.
9. Verify journey results contain departure time, arrival time and duration.
10. Verify View Details expands the correct route and shows the expected legs.
11. Verify a journey with a known service disruption displays the disruption information.
12. Verify browser refresh does not create stale or duplicated journey results.
13. Verify browser back navigation returns to a usable planner state.
14. Verify long, special-character and whitespace-heavy location input is handled safely.
15. Verify responsive layout at mobile, tablet and desktop widths.

## Non-functional scenarios

1. Accessibility: keyboard navigation and logical focus order.
2. Accessibility: labels and controls are announced correctly by a screen reader.
3. Accessibility: sufficient colour contrast and visible focus indicators.
4. Accessibility: page remains usable at 200% browser zoom.
5. Performance: initial Journey Planner page load meets the agreed SLA.
6. Performance: journey results are returned within the agreed response-time threshold.
7. Reliability: repeated journey searches do not produce stale results.
8. Compatibility: supported Chrome, Edge, Firefox and Safari versions.
9. Mobile compatibility: supported iOS and Android browsers.
10. Resilience: friendly error handling when journey services are unavailable.
11. Security: no sensitive information is exposed in URLs, client-side logs or error messages.
12. Observability: browser-console/network failures are captured in test diagnostics.
13. Scalability: repeated parallel searches do not cause the test framework itself to become unstable.
14. Usability: validation messages clearly explain what the user needs to correct.
