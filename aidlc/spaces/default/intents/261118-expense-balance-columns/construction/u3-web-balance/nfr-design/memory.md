<!-- INVARIANT: examples are single-line HTML comments so a fresh template parses to total=0 (MEMORY_EMPTY). Do NOT un-comment or split across lines. t100 guards this. -->
> This file is kept up to date automatically while the stage runs.

## Interpretations
<!-- example: 2026-05-29T10:14:32Z — chose REST over GraphQL; the consuming team only needs CRUD, revisit if subscriptions land -->

## Deviations
<!-- example: 2026-05-29T10:14:32Z — skipped the optional caching layer the stage prose suggested; the dataset is small enough that it adds risk -->

## Tradeoffs
<!-- example: 2026-05-29T10:14:32Z — picked TDD over BDD this run; the team is unit-first and the domain is well-understood -->

## Open questions
- 2026-11-18T06:08Z Advisory review READY, six Minor items for the code plan: right-align all amount columns with table-level classes (for example `[&_tbody_td]:text-right` when the first column is a row header) and give the row-header th the td padding; the expense table passes no emptyMessage so headers show; add a regionLabel prop and move the pager outside the scroll region; give sticky cells borders and z-order (top-left above header above column); test payment table, StreamsSummary and stream labels in the new money format; keep the timing test lenient.
<!-- example: 2026-05-29T10:14:32Z — confirm the retention window with compliance before the next stage hardens the schema -->
