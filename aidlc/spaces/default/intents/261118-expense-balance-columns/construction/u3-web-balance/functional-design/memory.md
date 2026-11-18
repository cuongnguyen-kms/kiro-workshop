<!-- INVARIANT: examples are single-line HTML comments so a fresh template parses to total=0 (MEMORY_EMPTY). Do NOT un-comment or split across lines. t100 guards this. -->
> This file is kept up to date automatically while the stage runs.

## Interpretations
<!-- example: 2026-05-29T10:14:32Z — chose REST over GraphQL; the consuming team only needs CRUD, revisit if subscriptions land -->

## Deviations
<!-- example: 2026-05-29T10:14:32Z — skipped the optional caching layer the stage prose suggested; the dataset is small enough that it adds risk -->

## Tradeoffs
<!-- example: 2026-05-29T10:14:32Z — picked TDD over BDD this run; the team is unit-first and the domain is well-understood -->

## Open questions
- 2026-11-18T05:58Z Advisory review READY with one Major: the sticky header needs a vertical scroll container. Carry into NFR Design and the code plan: give the expense table region a height limit (for example max-h-[70vh] with overflow-auto) so the header sticks on the tab and in the popup; keep the pager outside the scrolled region; round with an exact decimal-string method (no float half-cent errors) and test 1.005 and -1.005; treat missing balanceRows as empty and test it; make the region name differ clearly from the hidden caption.
<!-- example: 2026-05-29T10:14:32Z — confirm the retention window with compliance before the next stage hardens the schema -->
