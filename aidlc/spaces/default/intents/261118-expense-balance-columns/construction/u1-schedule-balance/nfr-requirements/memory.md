<!-- INVARIANT: examples are single-line HTML comments so a fresh template parses to total=0 (MEMORY_EMPTY). Do NOT un-comment or split across lines. t100 guards this. -->
> This file is kept up to date automatically while the stage runs.

## Interpretations
<!-- example: 2026-05-29T10:14:32Z — chose REST over GraphQL; the consuming team only needs CRUD, revisit if subscriptions land -->

## Deviations
<!-- example: 2026-05-29T10:14:32Z — skipped the optional caching layer the stage prose suggested; the dataset is small enough that it adds risk -->

## Tradeoffs
<!-- example: 2026-05-29T10:14:32Z — picked TDD over BDD this run; the team is unit-first and the domain is well-understood -->

## Open questions
- 2026-11-18T04:52Z Advisory review READY with findings to carry into NFR Design and Code Generation: what a saved contract read does when stored totals differ (map to an API error in U2, or skip balance rows); SEC1 should only throw on unequal totals when expense rows exist (BR3.2), with an empty-expense test; the 50 ms timing test should be loose enough not to fail verify.ps1 on a busy machine; add a test per ST/LT branch (BR2.4) and per sign (BR2.5).
<!-- example: 2026-05-29T10:14:32Z — confirm the retention window with compliance before the next stage hardens the schema -->
