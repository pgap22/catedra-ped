## 1. Data Model And Schema

- [ ] 1.1 Add SQLite schema updates for category pack details with category, product, and percentage fields.
- [ ] 1.2 Add optional product fields for maximum per delivery and replenishment days.
- [ ] 1.3 Add product reference support to `OrdenDetalle` for new confirmed deliveries while tolerating old category-only rows.
- [ ] 1.4 Update affected model classes to expose pack, product detail, and replenishment fields.

## 2. Pack Configuration

- [ ] 2.1 Add service methods to list, save, validate, and delete category pack lines.
- [ ] 2.2 Add a simple WinForms screen or menu flow to configure product percentages by category.
- [ ] 2.3 Validate that each usable category pack sums to 100 percent before distribution.
- [ ] 2.4 Add product form fields for optional max-per-delivery and replenishment-days settings.

## 3. Distribution Logic

- [ ] 3.1 Keep existing category deficit and `MonticuloMaximo` priority flow as the first distribution step.
- [ ] 3.2 Split each category assignment into product-specific lines using the configured pack percentages.
- [ ] 3.3 Validate product stock per generated product line and omit or reduce lines when stock is insufficient.
- [ ] 3.4 Apply max-per-delivery limits when a product has that rule configured.
- [ ] 3.5 Check confirmed product history before proposing products with replenishment-days rules.
- [ ] 3.6 Include pack splitting, stock shortage, and replenishment decisions in the calculation explanation.

## 4. Confirmation And History

- [ ] 4.1 Save confirmed `OrdenDetalle` rows with product information for each product-specific line.
- [ ] 4.2 Discount inventory from the exact product saved on the confirmed detail.
- [ ] 4.3 Update history queries and UI to display product-specific deliveries when available.
- [ ] 4.4 Add product-level history lookup used by replenishment checks.

## 5. Distribution UI

- [ ] 5.1 Update the distribution grid to show one row per beneficiary, category, product, and quantity.
- [ ] 5.2 Preserve filtering by category and beneficiary with product-specific rows.
- [ ] 5.3 Validate manual quantity edits against product stock instead of category stock only.
- [ ] 5.4 Preserve undo behavior for manual edits on product-specific rows.

## 6. Demo Data And Verification

- [ ] 6.1 Update the small demo seed with category packs by percentage for grains, oils, and hygiene.
- [ ] 6.2 Add at least one replenishment example such as a toothbrush with a multi-month rule.
- [ ] 6.3 Verify flow: seed demo, generate proposal, confirm delivery, inspect product history, simulate future date, generate next proposal.
- [ ] 6.4 Run `dotnet build` and fix any compile errors.
