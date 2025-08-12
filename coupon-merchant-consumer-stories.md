# Coupon Library — Merchant & Consumer User Stories

**Product role:** You are the Product Owner.  
**Goal:** Define features (merchant + consumer) for a C#/.NET coupon library used by ecommerce sites.  
**Note:** Stories intentionally avoid technical/developer tasks.

---

## Epic 1 — Shopper Coupon Entry & Feedback (Consumer)

### U-001 Enter a coupon at checkout
**Story:** As a shopper, I want to enter a coupon code at checkout so I can see my savings before paying.  
**Acceptance Criteria:**
- **Given** a valid, eligible code, **when** I apply it, **then** my order total updates and shows the discount.
- **Given** an invalid or expired code, **when** I apply it, **then** I see a clear message explaining why it can’t be used.

### U-002 Change or remove a coupon
**Story:** As a shopper, I want to remove or replace my coupon so I can try a better one.  
**Acceptance Criteria:**
- **Given** a coupon is applied, **when** I remove it, **then** the total reverts and the discount disappears.
- **Given** a new code, **when** I apply it, **then** the previous code no longer affects my total.

### U-003 See what the coupon affected
**Story:** As a shopper, I want to see which items received a discount so I understand my savings.  
**Acceptance Criteria:**
- **Given** my cart has eligible and ineligible items, **when** the coupon is applied, **then** the savings are shown per affected items and the order total reflects the change.

---

## Epic 2 — Create & Manage Promotions (Merchant)

### M-001 Create a money-off or percent-off coupon
**Story:** As a merchant, I want to create a coupon with either a fixed amount off or a percentage off so I can run simple promotions.  
**Acceptance Criteria:**
- **Given** details I provide (name, code, discount amount or percent), **when** I save the promotion, **then** it’s available to use on its start date.
- **Given** overlapping promotions, **when** both could apply, **then** the one(s) allowed by my stacking rules are considered.

### M-002 Schedule when a promotion runs
**Story:** As a merchant, I want to set the start and end date/time for a coupon so it only works during the intended period.  
**Acceptance Criteria:**
- **Given** the promotion window, **when** the current time is before the start, **then** the code can’t be used.
- **Given** the end time has passed, **when** shoppers try the code, **then** they see that it’s expired.

### M-003 Pause or resume a promotion
**Story:** As a merchant, I want to temporarily pause or resume a coupon so I can react to business needs.  
**Acceptance Criteria:**
- **Given** a running coupon, **when** I pause it, **then** shoppers are told it’s unavailable.
- **Given** a paused coupon, **when** I resume it, **then** shoppers can use it immediately if other conditions are met.

---

## Epic 3 — Order Eligibility & Thresholds (Merchant)

### M-004 Minimum spend requirement
**Story:** As a merchant, I want to set a minimum order subtotal so only larger orders qualify.  
**Acceptance Criteria:**
- **Given** a minimum spend, **when** the shopper’s subtotal is below it, **then** the code is not applied and the message explains the minimum.
- **Given** the subtotal meets or exceeds it, **then** the code can apply.

### M-005 Cap the maximum discount
**Story:** As a merchant, I want to limit the maximum discount from a coupon so it doesn’t exceed a set amount.  
**Acceptance Criteria:**
- **Given** a high-value cart, **when** the discount would exceed my cap, **then** the discount is limited to the cap and the total reflects that.

### M-006 Choose what the discount applies to (shipping/taxes)
**Story:** As a merchant, I want to decide whether the discount applies only to merchandise or can include shipping and/or other charges.  
**Acceptance Criteria:**
- **Given** I set “merchandise only,” **when** the coupon is applied, **then** shipping and similar charges are not reduced.
- **Given** I set “order total,” **when** the coupon is applied, **then** the eligible portions are reduced accordingly.

---

## Epic 4 — Targeting & Exclusions (Merchant)

### M-007 Limit to certain products, categories, or brands
**Story:** As a merchant, I want to restrict a coupon to specific items so the offer targets the right products.  
**Acceptance Criteria:**
- **Given** included categories or brands, **when** the coupon is applied, **then** only matching items receive a discount.

### M-008 Exclude sale items or gift cards
**Story:** As a merchant, I want to exclude sale items and gift cards so promotions don’t stack where I don’t intend.  
**Acceptance Criteria:**
- **Given** excluded types, **when** the coupon is applied, **then** those items do not receive any discount and shoppers see why.

### M-009 First-time or specific customer groups only
**Story:** As a merchant, I want to make a coupon available only to first-time buyers or selected customer groups.  
**Acceptance Criteria:**
- **Given** a first-order-only rule, **when** a returning shopper tries the code, **then** they’re told it’s for first-time orders.
- **Given** a targeted group, **when** an ineligible shopper tries the code, **then** they see they’re not eligible.

### M-010 Geographic or currency restrictions
**Story:** As a merchant, I want to restrict coupons by country or currency so offers are appropriate for each market.  
**Acceptance Criteria:**
- **Given** a market restriction, **when** the shopper’s order doesn’t match, **then** the code can’t be used and the message explains the market limitation.

---

## Epic 5 — Usage Limits & Code Distribution (Merchant)

### M-011 Limit total redemptions
**Story:** As a merchant, I want to limit how many times a coupon can be used overall so I can control promotion cost.  
**Acceptance Criteria:**
- **Given** a total redemption limit, **when** it’s reached, **then** further attempts show that the offer has ended.

### M-012 Limit per-shopper usage
**Story:** As a merchant, I want to limit how many times a single shopper can use a coupon so it isn’t abused.  
**Acceptance Criteria:**
- **Given** a per-shopper limit, **when** a shopper exceeds it, **then** they’re told they’ve reached the limit.

### M-013 Generate single-use codes
**Story:** As a merchant, I want to generate unique, one-time codes linked to a promotion so I can distribute them individually.  
**Acceptance Criteria:**
- **Given** generated single-use codes, **when** a code has been redeemed, **then** it cannot be used again and the shopper is informed if they try.

### M-014 End or replace unused codes
**Story:** As a merchant, I want to invalidate or replace unused codes when a campaign changes.  
**Acceptance Criteria:**
- **Given** a set of codes, **when** I mark them as ended, **then** shoppers are told the campaign is no longer active.

---

## Epic 6 — Stacking & “Best Deal” (Merchant & Consumer)

### M-015 Control whether coupons can be combined
**Story:** As a merchant, I want to decide if a coupon can be used with others so offers follow my rules.  
**Acceptance Criteria:**
- **Given** a non-combinable coupon, **when** a shopper tries to add another, **then** they’re told only one may be used.

### M-016 Automatically apply the best deal
**Story:** As a merchant, I want the system to pick the combination that gives the shopper the best allowed price so the experience feels fair.  
**Acceptance Criteria:**
- **Given** multiple eligible coupons and my stacking rules, **when** a shopper applies codes, **then** the lowest allowed total is presented with a note indicating which promotions are applied.

### U-004 Clear conflict messages
**Story:** As a shopper, I want clear explanations when coupons can’t be combined so I know how to proceed.  
**Acceptance Criteria:**
- **Given** incompatible codes, **when** I try to apply both, **then** I see which one is in effect and why the other can’t be used.

---

## Epic 7 — Shipping & Perks (Merchant & Consumer)

### M-017 Free shipping coupon
**Story:** As a merchant, I want to offer free or discounted shipping so I can promote higher conversion.  
**Acceptance Criteria:**
- **Given** a free-shipping offer, **when** the shopper qualifies, **then** shipping charges are removed or reduced and labeled accordingly.

### M-018 Spend-and-save thresholds
**Story:** As a merchant, I want offers like “Spend $X, save $Y” so I can encourage larger baskets.  
**Acceptance Criteria:**
- **Given** a threshold, **when** the shopper’s subtotal meets it, **then** the savings apply; otherwise, they see what amount remains to qualify.

### M-019 Gift with purchase (GWP)
**Story:** As a merchant, I want to offer a free item once a threshold is met so I can increase order value.  
**Acceptance Criteria:**
- **Given** a qualifying threshold and gift item, **when** the shopper qualifies, **then** the gift is added at no cost with a note showing it’s part of the promotion.

---

## Epic 8 — Localization, Messaging & Transparency (Merchant & Consumer)

### U-005 Localized messages and currencies
**Story:** As a shopper, I want discount messages and amounts to make sense for my language and currency.  
**Acceptance Criteria:**
- **Given** my locale and currency, **when** I apply a coupon, **then** messages and amounts are presented appropriately.

### M-020 Clear reasons when a code can’t be used
**Story:** As a merchant, I want shoppers to see specific, friendly reasons when a coupon doesn’t apply so support requests decrease.  
**Acceptance Criteria:**
- **Given** an ineligible attempt (e.g., below minimum spend, excluded item), **when** the shopper applies a code, **then** a concise reason is shown.

### U-006 See promotion terms before applying
**Story:** As a shopper, I want to view the key terms of a promotion so I know how to qualify.  
**Acceptance Criteria:**
- **Given** a coupon code, **when** I view details, **then** I can see the basics (dates, eligibility, limits) in plain language.
