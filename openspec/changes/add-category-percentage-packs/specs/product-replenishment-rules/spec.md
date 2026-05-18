## ADDED Requirements

### Requirement: Configure optional product replenishment rules
The system SHALL allow products to optionally define a maximum quantity per delivery and a minimum number of days before the same beneficiary can receive the product again.

#### Scenario: Product without replenishment rules behaves normally
- **WHEN** a product has no maximum quantity and no replenishment days configured
- **THEN** the system treats it as a normal consumable product

#### Scenario: Product with maximum per delivery is limited
- **WHEN** a product has a maximum quantity per delivery configured
- **THEN** the system does not propose more than that maximum for a single beneficiary in one confirmed delivery

### Requirement: Enforce minimum replenishment days by beneficiary and product
The system SHALL check delivery history before proposing a product that has replenishment days configured.

#### Scenario: Product was delivered recently
- **WHEN** a beneficiary received the same product fewer days ago than the configured replenishment period
- **THEN** the system omits that product from the proposal for that beneficiary

#### Scenario: Product is eligible after replenishment period
- **WHEN** a beneficiary has not received the product within the configured replenishment period
- **THEN** the system may include that product in the proposal if pack percentage and stock allow it

### Requirement: Explain replenishment omissions
The system SHALL include replenishment decisions in the distribution explanation when a product is omitted because it was recently delivered.

#### Scenario: Product skipped by history
- **WHEN** a product is skipped because the beneficiary received it recently
- **THEN** the explanation states that the product is not eligible yet and includes the replenishment rule used
