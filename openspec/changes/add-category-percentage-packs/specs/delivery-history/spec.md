## ADDED Requirements

### Requirement: Display delivered products in history
The delivery history SHALL show the specific product delivered when product information exists on the delivery detail.

#### Scenario: History line has product
- **WHEN** a confirmed detail includes a product
- **THEN** the history displays beneficiary, category, product, quantity, unit, and delivery date

#### Scenario: Legacy history line has no product
- **WHEN** a confirmed detail does not include a product
- **THEN** the history still displays the category-level delivery without failing

### Requirement: Support product-level history checks
The system SHALL be able to query whether a beneficiary received a specific product within a date range.

#### Scenario: Replenishment check asks for product history
- **WHEN** distribution evaluates a product with replenishment days
- **THEN** the history query returns the most recent confirmed delivery date for that beneficiary and product if one exists
