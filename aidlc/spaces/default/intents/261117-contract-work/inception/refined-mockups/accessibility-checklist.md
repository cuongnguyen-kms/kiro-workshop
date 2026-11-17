# Accessibility Checklist

Target: **WCAG 2.2 Level AA** (answer to Q3). Applies to screens S1, S2 and S3.

## Perceivable

- [ ] Text contrast is at least 4.5:1, and large text and UI component boundaries are at least 3:1. Check the shadcn/ui theme values.
- [ ] Errors are not shown by colour alone: each has an icon and text.
- [ ] Every input has a visible `Label`. Placeholders are never the only label.
- [ ] Tables use real table markup with a caption and column headers.
- [ ] Content reflows at 320 px wide with no loss, except schedule tables, which scroll inside their own region.
- [ ] Text can be resized to 200% without losing content or function.

## Operable

- [ ] Every control works with the keyboard alone, in a logical order that matches the screen order.
- [ ] The focus indicator is visible on every control and is not hidden by sticky elements (WCAG 2.2: Focus Not Obscured).
- [ ] Focus indicator has at least 3:1 contrast against its surroundings.
- [ ] Interactive targets are at least 24 by 24 CSS px (WCAG 2.2: Target Size Minimum).
- [ ] The calendar picker can be skipped: every date can be typed (WCAG 2.2: Dragging Movements, no drag needed).
- [ ] Tabs on S3 follow the arrow-key pattern. The AlertDialog traps focus and closes with Escape.
- [ ] No keyboard trap anywhere. Focus returns to the triggering control when a dialog closes.

## Understandable

- [ ] Form error messages name the field and say how to fix it (see the interaction spec table).
- [ ] After a failed Save, focus moves to the error summary, and each entry links to its field.
- [ ] Fields that repeat information the user already entered are not asked again (WCAG 2.2: Redundant Entry). One-time streams reuse the start date as the end date.
- [ ] Labels and button text are consistent across screens ("Save contract", "New contract", "Remove").
- [ ] The language of the page is set (`lang="en"`).

## Robust

- [ ] Each stream card is a `fieldset` with a `legend`.
- [ ] Error text is tied to its field with `aria-describedby`, and invalid fields use `aria-invalid="true"`.
- [ ] Failure alerts use `role="alert"`; the save success banner uses `role="status"`.
- [ ] Added or removed stream cards are announced (focus moves to the new card or to Add stream).
- [ ] Automated checks (axe) pass on S1, S2 and S3 with no serious or critical issues.
