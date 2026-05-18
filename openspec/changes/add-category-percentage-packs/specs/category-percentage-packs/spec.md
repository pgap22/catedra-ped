## ADDED Requirements

### Requirement: Configure category packs by product percentage
The system SHALL allow each category to define a delivery pack composed of products from that category and a percentage for each product.

#### Scenario: Valid pack totals 100 percent
- **WHEN** a category has pack lines whose percentages sum to 100
- **THEN** the system treats that category pack as valid for automatic distribution

#### Scenario: Invalid pack total is rejected
- **WHEN** a category pack has percentages that do not sum to 100
- **THEN** the system prevents using that pack for automatic distribution and shows a clear validation message

### Requirement: Split category assignment into product lines
The system SHALL convert each assigned category quantity into product-specific delivery lines according to the configured pack percentages.

#### Scenario: Family receives multiple products from one category
- **WHEN** a family is assigned 20 units from a category with a 50/30/20 pack
- **THEN** the proposal contains product lines for 10 units, 6 units, and 4 units respectively

#### Scenario: Product line quantities are whole units
- **WHEN** a calculated product quantity contains decimals
- **THEN** the system floors the product quantity before adding it to the proposal

### Requirement: Respect product stock during pack splitting
The system SHALL never propose more of a product than its available stock.

#### Scenario: Pack product has insufficient stock
- **WHEN** the pack calculation requests more of a product than is available
- **THEN** the proposal assigns only the available stock for that product

#### Scenario: Pack product has no stock
- **WHEN** a product in the pack has zero stock
- **THEN** the proposal omits that product line and records the shortage in the calculation explanation

### Requirement: Keep category priority independent from pack composition
The system SHALL continue prioritizing beneficiaries by category deficit before splitting into product lines.

#### Scenario: Category deficit determines beneficiary order
- **WHEN** multiple families need the same category
- **THEN** the system uses the existing deficit priority rules to decide which families receive category quantity first

### Requirement: Confirm product-specific delivery details
The system SHALL save confirmed delivery details with the specific product assigned on each line.

#### Scenario: Confirmed pack delivery records products
- **WHEN** a proposed pack delivery is confirmed
- **THEN** each saved detail includes beneficiary, category, product, assigned quantity, and deficit information

#### Scenario: Product stock is discounted from matching product
- **WHEN** a product-specific delivery line is confirmed
- **THEN** the system discounts stock from that same product only
