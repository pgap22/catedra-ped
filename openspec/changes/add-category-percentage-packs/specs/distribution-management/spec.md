## ADDED Requirements

### Requirement: Show product-specific distribution proposal
The distribution screen SHALL show one proposal row per product-specific delivery line.

#### Scenario: Pack creates repeated beneficiary and category rows
- **WHEN** a beneficiary receives multiple products from the same category pack
- **THEN** the grid shows multiple rows with the same beneficiary and category but different products and quantities

### Requirement: Validate manual edits by product stock
The distribution screen SHALL validate manual quantity edits against available stock for the edited product.

#### Scenario: User increases product quantity beyond stock
- **WHEN** the user edits a product line above available product stock
- **THEN** the system rejects the edit and restores the previous value

### Requirement: Preserve undo for product quantity edits
The distribution screen SHALL keep undo support for manual quantity edits on product-specific proposal rows.

#### Scenario: User reverts product quantity edit
- **WHEN** the user clicks revert after editing a product quantity
- **THEN** the grid restores the previous product quantity value
