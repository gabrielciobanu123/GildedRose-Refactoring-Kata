# Gilded Rose — Behavior Coverage Matrix

Status: Conjured feature implemented and all tests pass (20/20). The ThirtyDays snapshot was deliberately updated for the changed Conjured behavior; only the four Conjured Quality values for days 1 to 4 differed from the previous snapshot. A follow-up fix restored ordinary-item behavior for items with a null Name. See "Test run results" below.

## Purpose

This document lists the observable behaviors of the current Gilded Rose system and maps each one to an existing test, if any. It is the plan we will use to decide which characterization tests to add **before** refactoring.

A "characterization test" simply means a test that pins down what the code does today, so refactoring cannot silently change the result.

## How the expected values were derived

- All "Expected final SellIn / Quality" values were first traced by hand from the original `GildedRose.UpdateQuality` implementation.
- They have since been **confirmed by running** `dotnet test`: every unit-tested value and the `ThirtyDays` snapshot pass against the current code.
- Daily update order in the original code (important for the numbers below):
  1. Adjust Quality using the item's **current** SellIn (SellIn before the update).
  2. Lower SellIn by 1 (except Sulfuras).
  3. If the now-lowered SellIn is below 0, apply a second Quality adjustment.

## How to read the mappings

- A scenario is marked **covered** if an existing test checks its expected result. A snapshot (approval) test counts: if the snapshot includes the item's row with the expected SellIn and Quality, that case is covered. A separate unit test is not required.
- "What the test checks" is kept distinct from "whether the test has been run and passed." All mapped tests below have now been run; the run results are summarised in the "Test run results" section.
- Status values:
  - **covered** — an existing test (unit or snapshot) checks this exact result.
  - **partially covered** — an existing test touches this area but does not check the full result (for example, checks only Name).
  - **missing** — no existing test checks this.
  - **deferred new feature** — Conjured behavior; not implemented yet and out of scope for the refactoring step.

Important caveat: the `ThirtyDays` snapshot only covers a case if that exact item row (name, SellIn, Quality) actually appears in the saved output on the matching day. The snapshot uses a fixed inventory, so it covers the specific values in that inventory, not every value in this matrix.

## Existing tests (summary)

- `GildedRoseTest.OrdinaryItem_BeforeExpiry_DegradesByOne_AndKeepsName()` — an ordinary item "+5 Dexterity Vest" starts 10/20, updates once, asserts SellIn 9, Quality 19, and unchanged Name. (Replaces the old broken `foo()` placeholder.)
- `GildedRoseTest.OrdinaryItem_AtZeroQuality_AndZeroSellIn_StaysAtZeroQuality_AndKeepsName()` — an ordinary item "+5 Dexterity Vest" starts 0/0, updates once, asserts SellIn -1, Quality 0, and unchanged Name. (Replaces the old `ApprovalTest.Foo` snapshot; moved into the unit-test file as a normal xUnit test.)
- `GildedRoseTest.OrdinaryItem_AfterExpiry_QualityNeverGoesNegative()` — Elixir 0/1, one update, asserts SellIn -1, Quality 0, unchanged Name. (ORD-4)
- `GildedRoseTest.BackstagePass_TenDaysLeft_IncreasesByTwo()` — backstage 10/20, one update, asserts SellIn 9, Quality 22. (BP-10)
- `GildedRoseTest.BackstagePass_SixDaysLeft_IncreasesByTwo()` — backstage 6/20, one update, asserts SellIn 5, Quality 22. (BP-6)
- `GildedRoseTest.BackstagePass_FiveDaysLeft_IncreasesByThree()` — backstage 5/20, one update, asserts SellIn 4, Quality 23. (BP-5)
- `GildedRoseTest.AgedBrie_AfterExpiry_CapsAtFifty()` — Aged Brie 0/49, one update, asserts SellIn -1, Quality 50, unchanged Name. (BRIE-4)
- `GildedRoseTest.Sulfuras_WithUnusualValues_KeepsBothValuesUnchanged()` — Sulfuras 5/10, one update, asserts SellIn 5, Quality 10, unchanged Name. (SULF-3)
- `GildedRoseTest.UpdateQuality_MutatesExistingItemObjectsInPlace()` — asserts the same object reference is updated (SellIn 4, Quality 9). (OBJ-1)
- `GildedRoseTest.UpdateQuality_WithEmptyInventory_DoesNotThrow()` — empty list updates safely. (INV-EMPTY)
- `ApprovalTest.ThirtyDays()` — runs the full program for 30 days and verifies printed output against `ApprovalTest.ThirtyDays.verified.txt`. Confirmed passing by running `dotnet test`, so the saved output matches the current code. This was our golden-master safety net for the refactoring step. The committed snapshot is now the **post-Conjured** baseline: during the refactoring step it matched the original code exactly, and it was later updated for the intended Conjured change (only the four Conjured Quality values for days 1 to 4 differ from the original; see the "New feature — Conjured items" section).

Because `ThirtyDays` is a whole-program text snapshot, it exercises many rules together but does not isolate any single rule or boundary. Behaviors it could not isolate on their own have been given dedicated unit tests, so no row remains only partially covered.

---

## Coverage matrix — current behavior

Assumptions used for the table:
- "Ordinary item" uses a neutral name that matches no special rule (e.g. "+5 Dexterity Vest").
- One "daily update" = one call to `UpdateQuality()`.
- "Backstage" = "Backstage passes to a TAFKAL80ETC concert".
- "Sulfuras" = "Sulfuras, Hand of Ragnaros".

Each scenario is covered either by a dedicated unit test or by an exact matching row in the `ThirtyDays` snapshot. The snapshot uses a fixed inventory, so it only covers the specific value paths that appear in it; behaviors it cannot isolate (for example, backstage increments hidden by the 50 cap) are covered by dedicated unit tests instead. All rows below are now covered.

| ID | Business rule | Start name | Start SellIn | Start Quality | Days | Exp. SellIn | Exp. Quality | Existing test | Status |
|----|---------------|-----------|-------------:|--------------:|-----:|------------:|-------------:|---------------|--------|
| ORD-1 | Ordinary item degrades by 1 before expiry | +5 Dexterity Vest | 10 | 20 | 1 | 9 | 19 | GildedRoseTest.OrdinaryItem_BeforeExpiry_DegradesByOne_AndKeepsName (also ThirtyDays row) | covered |
| ORD-2 | Ordinary item degrades by 2 after expiry | Elixir of the Mongoose | 5 | 7 | 6 | -1 | 0 | ThirtyDays (row: Elixir, day 5→6 crosses expiry) | covered |
| ORD-3 | Quality never goes negative (at Quality 0, SellIn 0) | +5 Dexterity Vest | 0 | 0 | 1 | -1 | 0 | GildedRoseTest.OrdinaryItem_AtZeroQuality_AndZeroSellIn_StaysAtZeroQuality_AndKeepsName | covered |
| ORD-4 | Quality never negative even after expiry (positive Quality crossing 0) | Elixir of the Mongoose | 0 | 1 | 1 | -1 | 0 | GildedRoseTest.OrdinaryItem_AfterExpiry_QualityNeverGoesNegative | covered |
| BRIE-1 | Aged Brie increases by 1 before expiry | Aged Brie | 2 | 0 | 1 | 1 | 1 | ThirtyDays (row: Aged Brie, day 0→1) | covered |
| BRIE-2 | Aged Brie increases by 2 after expiry | Aged Brie | 2 | 0 | 3 | -1 | 4 | ThirtyDays (row: Aged Brie, day 2→3 crosses expiry) | covered |
| BRIE-3 | Aged Brie never exceeds 50 | Aged Brie | 2 | 0 | 26 | -24 | 50 | ThirtyDays (row: Aged Brie reaches 50 at day 26 and holds) | covered |
| BRIE-4 | Aged Brie caps at 50 even with double gain after expiry | Aged Brie | 0 | 49 | 1 | -1 | 50 | GildedRoseTest.AgedBrie_AfterExpiry_CapsAtFifty | covered |
| SULF-1 | Sulfuras keeps both values, before sell date | Sulfuras, Hand of Ragnaros | 0 | 80 | 1 | 0 | 80 | ThirtyDays (row: Sulfuras 0/80 unchanged) | covered |
| SULF-2 | Sulfuras keeps both values, past sell date | Sulfuras, Hand of Ragnaros | -1 | 80 | 1 | -1 | 80 | ThirtyDays (row: Sulfuras -1/80 unchanged) | covered |
| SULF-3 | Sulfuras not auto-corrected to 80 (unusual value left as-is) | Sulfuras, Hand of Ragnaros | 5 | 10 | 1 | 5 | 10 | GildedRoseTest.Sulfuras_WithUnusualValues_KeepsBothValuesUnchanged | covered |
| BP-11 | Backstage +1 when 11+ days left | Backstage passes to a TAFKAL80ETC concert | 15 | 20 | 1 | 14 | 21 | ThirtyDays (row: Backstage 15/20, day 0→1) | covered |
| BP-10 | Backstage +2 when 10 or fewer days | Backstage passes to a TAFKAL80ETC concert | 10 | 20 | 1 | 9 | 22 | GildedRoseTest.BackstagePass_TenDaysLeft_IncreasesByTwo | covered |
| BP-6 | Backstage +2 at 6 days (not yet +3) | Backstage passes to a TAFKAL80ETC concert | 6 | 20 | 1 | 5 | 22 | GildedRoseTest.BackstagePass_SixDaysLeft_IncreasesByTwo | covered |
| BP-5 | Backstage +3 when 5 or fewer days | Backstage passes to a TAFKAL80ETC concert | 5 | 20 | 1 | 4 | 23 | GildedRoseTest.BackstagePass_FiveDaysLeft_IncreasesByThree | covered |
| BP-1 | Backstage +3 at 1 day left | Backstage passes to a TAFKAL80ETC concert | 15 | 20 | 14 | 1 | 47 | ThirtyDays (row: Backstage 15/20 at day 14 shows 1/47) | covered |
| BP-0 | Backstage drops to 0 the day it expires | Backstage passes to a TAFKAL80ETC concert | 15 | 20 | 16 | -1 | 0 | ThirtyDays (row: Backstage 15/20 at day 16 shows -1/0) | covered |
| BP-NEG1 | Backstage stays 0 after concert | Backstage passes to a TAFKAL80ETC concert | 15 | 20 | 17 | -2 | 0 | ThirtyDays (row: Backstage 15/20 at day 17 shows -2/0) | covered |
| BP-CAP | Backstage never exceeds 50 near boundary | Backstage passes to a TAFKAL80ETC concert | 10 | 49 | 1 | 9 | 50 | ThirtyDays (row: Backstage 10/49, day 0→1 shows 9/50) | covered |
| NAME-1 | Item name is never changed | +5 Dexterity Vest | 10 | 20 | 1 | 9 | 19 | GildedRoseTest.OrdinaryItem_BeforeExpiry_DegradesByOne_AndKeepsName (asserts Name); also ThirtyDays | covered |
| OBJ-1 | Existing item objects are mutated in place (same object updated, not replaced) | +5 Dexterity Vest | 5 | 10 | 1 | 4 | 9 | GildedRoseTest.UpdateQuality_MutatesExistingItemObjectsInPlace | covered |
| INV-EMPTY | Empty inventory updates without error | (no items) | n/a | n/a | 1 | n/a | n/a | GildedRoseTest.UpdateQuality_WithEmptyInventory_DoesNotThrow | covered |
| INV-MIX | Mixed inventory updates each item by its own rule | multiple (snapshot inventory) | mixed | mixed | 1 | per item | per item | ThirtyDays (full mixed inventory each day) | covered |
| MULTI-BRIE | Aged Brie over several days climbs and caps at 50 | Aged Brie | 2 | 0 | 26 | -24 | 50 | ThirtyDays (row: Aged Brie across 26 days) | covered |
| MULTI-ORD | Ordinary item over several days, degrades then floors at 0 | Elixir of the Mongoose | 5 | 7 | 6 | -1 | 0 | ThirtyDays (row: Elixir across 6 days) | covered |
| MULTI-BP | Backstage over several days rises then drops to 0 after concert | Backstage passes to a TAFKAL80ETC concert | 15 | 20 | 16 | -1 | 0 | ThirtyDays (row: Backstage 15/20 across 16 days) | covered |

### Notes on selected rows

- **Covered-by-snapshot rows** use the snapshot's actual starting inventory (for example, the backstage rows track the pass that starts at 15/20, read at the relevant later day). This is why some Days counts are large: they point at the specific day in the 30-day run where that value appears.
- **BP-10 and BP-5 (now covered by unit tests):** the snapshot has passes starting at SellIn 10 and 5, but those start at Quality 49 and immediately hit the 50 cap, hiding the raw +2 and +3 gain. Dedicated unit tests use a pass at Quality 20 so the gain is visible, proving the increment rule directly.
- **BP-6 vs BP-5:** the code uses `SellIn < 11` and `SellIn < 6`, i.e. "10 or fewer" and "5 or fewer". So SellIn 6 gains +2 (not +3) and SellIn 5 gains +3. This off-by-one is now pinned by the BP-6 and BP-5 unit tests.
- **BP-0 / BP-NEG1:** on the day SellIn is 0, the pass first gains its increase, then SellIn becomes -1, then the expired branch sets Quality to `Quality - Quality = 0`. It ends at 0 regardless of how high it climbed. The snapshot's 15/20 pass shows this at day 16 (-1/0) and day 17 (-2/0).
- **BRIE-2 (covered):** the snapshot's Aged Brie starts 2/0. At day 3 it reads -1/4, which shows the double gain after expiry (2→1→0 gaining +1 each, then -1 gaining +2). Ends -1/4.
- **BRIE-3 (covered):** the snapshot's Aged Brie reaches 50 at day 26 and holds at 50 afterward, showing the 50 cap.
- **BRIE-4 (now covered by a unit test):** the specific "start at 49, gain +2 after expiry, cap at 50 in a single step" path does not appear in the snapshot, so a dedicated unit test pins it.
- **SULF-3 (now covered by a unit test):** the original code never touches Sulfuras, so an unusual Quality like 10 stays 10. The snapshot only has Sulfuras at 80, so a dedicated unit test proves the "leave unusual values alone" behavior. This matters because we must **not** add auto-correction to 80.
- **OBJ-1 (now covered by a unit test):** the snapshot prints values but does not assert that the same object references are updated in place. A dedicated unit test uses `Assert.Same` to prove the passed-in object is mutated, not replaced.
- **INV-MIX (covered):** the snapshot updates the full mixed inventory every day, so each category is exercised together in one `UpdateQuality()` call.

---

## New feature — Conjured items

Status for this whole section: **implemented and passing.** All Conjured tests pass, and the ThirtyDays snapshot has been updated for the changed behavior.

Behavior now in production: a Conjured item (name starting with `"Conjured "`, case-sensitive) loses 2 Quality per day before expiry and 4 per day once past its sell date, never below 0. All other item types are unchanged.

### Name-recognition policy (chosen)

An item is treated as **Conjured** when its `Name` **starts with `"Conjured "` (including the trailing space), using a case-sensitive comparison.**

Consequences of this chosen interpretation:
- `"Conjured Mana Cake"` matches (starts with `"Conjured "`).
- `"conjured mana cake"` does **not** match (wrong case) and stays an ordinary item.
- `"Conjured"` on its own does **not** match (no trailing space) and stays an ordinary item.
- The trailing space avoids accidentally matching an unrelated name that merely begins with the letters "Conjured" with no following word.

This is a deliberate design choice, not a requirement quotation. The written requirement only names the category ("Conjured items degrade twice as fast"); the matching rule above is how we decide which items belong to it.

### Behavior (implemented)

Conjured items lose Quality:
- by **2** before expiry (starting SellIn 1 or more), and
- by **4** when starting SellIn is **zero or below**,
- never dropping below 0.

SellIn decreases by 1 each day, exactly like an ordinary item.

| ID | Business rule (target) | Start name | Start SellIn | Start Quality | Days | Exp. SellIn | Exp. Quality | Test | Status |
|----|------------------------|-----------|-------------:|--------------:|-----:|------------:|-------------:|------|--------|
| CONJ-1 | Conjured degrades by 2 before expiry | Conjured Mana Cake | 3 | 6 | 1 | 2 | 4 | GildedRoseTest.ConjuredItem_BeforeExpiry_DegradesByTwo | covered, passing |
| CONJ-1b | At SellIn 1 (still before expiry) loses only 2 | Conjured Mana Cake | 1 | 10 | 1 | 0 | 8 | GildedRoseTest.ConjuredItem_OneDayBeforeExpiry_DegradesByTwo | covered, passing |
| CONJ-2 | Starting at SellIn 0 (zero or below) loses 4 | Conjured Mana Cake | 0 | 10 | 1 | -1 | 6 | GildedRoseTest.ConjuredItem_StartingAtZeroSellIn_DegradesByFour | covered, passing |
| CONJ-2b | Already past expiry (negative SellIn) loses 4 | Conjured Mana Cake | -2 | 10 | 1 | -3 | 6 | GildedRoseTest.ConjuredItem_AlreadyPastExpiry_DegradesByFour | covered, passing |
| CONJ-3 | Quality never goes below 0 before expiry | Conjured Mana Cake | 5 | 1 | 1 | 4 | 0 | GildedRoseTest.ConjuredItem_BeforeExpiry_QualityNeverGoesNegative | covered, passing |
| CONJ-3b | Quality never goes below 0 after expiry | Conjured Mana Cake | 0 | 3 | 1 | -1 | 0 | GildedRoseTest.ConjuredItem_AfterExpiry_QualityNeverGoesNegative | covered, passing |
| CONJ-NAME-1 | Lowercase "conjured" name is treated as ordinary | conjured mana cake | 3 | 6 | 1 | 2 | 5 | GildedRoseTest.ItemWithLowercaseConjuredName_IsTreatedAsOrdinary | covered, passing |
| CONJ-NAME-2 | Bare "Conjured" (no trailing space) is treated as ordinary | Conjured | 3 | 6 | 1 | 2 | 5 | GildedRoseTest.ItemNamedConjuredWithoutTrailingSpace_IsTreatedAsOrdinary | covered, passing |
| NAME-NULL | Null Name is treated as ordinary (no exception), matching original code | (null) | 10 | 20 | 1 | 9 | 19 | GildedRoseTest.ItemWithNullName_IsTreatedAsOrdinary | covered, passing |

Notes:
- CONJ-3 (5/1 → 4/0) now passes under the Conjured rate (loses 2 from 1, floored at 0). It happens to share the same result an ordinary item would reach here, but it is a genuine floor check for Conjured items.
- The two name-policy tests pass because non-matching names behave as ordinary items, guarding the matching rule rather than the degrade rate.
- **Null name (NAME-NULL):** the original code used `!=` name comparisons, which are null-safe, so a null-named item fell through to the ordinary path without error. The new `IsConjured` check called `item.Name.StartsWith(...)`, which would throw a `NullReferenceException` on a null name. The smallest fix adds a `item.Name != null` guard in `IsConjured`, so a null name is not Conjured and behaves as an ordinary item, restoring the original behavior. No other behavior changed.
- **Snapshot impact (done):** implementing the feature changed the "Conjured Mana Cake" lines in `ApprovalTest.ThirtyDays.verified.txt`. A line-by-line comparison confirmed that only the four Conjured Quality values for days 1 to 4 differed (day 1: 5→4, day 2: 4→2, day 3: 3→0, day 4: 1→0); names, SellIn values, and all other items were identical. The snapshot was updated to the new output.

---

## Assumptions

- The committed `ThirtyDays.verified.txt` is the **post-Conjured** baseline, not the original-code output. During the refactoring step it reflected the original code's behavior exactly; it was then deliberately updated for the Conjured change, so it now differs from the original in only the four Conjured Quality values for days 1 to 4 (all other rows are identical). This has been **confirmed by running** `dotnet test`: the `ApprovalTest.ThirtyDays` test passes, so the saved output matches the current code exactly.
- "Ordinary item" behavior is represented by names that match no special rule; the specific neutral name does not affect the numbers.
- One daily update equals one `UpdateQuality()` call.
- The Conjured name-recognition policy is now decided: a name that starts with `"Conjured "` (with the trailing space), case-sensitive. See the "New feature — Conjured items" section.

---

## Test run results

Confirmed by running `dotnet test GildedRose.sln` from `csharp.xUnit`.

Characterization run (before Conjured tests were added): total 11 tests, all passed. The existing-behavior tests and the `ThirtyDays` golden master are all green and unchanged.

Intermediate run (after adding Conjured tests, before implementing the feature): total 19 tests, 14 passed, 5 failed — the expected "feature missing" state, with the five Conjured degrade-rate tests failing.

Run after implementing the feature and updating the snapshot: total 19 tests, 19 passed, 0 failed. The snapshot update changed only the four Conjured Quality values for days 1 to 4 (day 1: 5→4, day 2: 4→2, day 3: 3→0, day 4: 1→0); everything else in the 373-line output was byte-for-byte identical.

Final run (after the null-name fix and its test): total 20 tests, **20 passed, 0 failed**.

- All 8 Conjured tests pass, plus the new NAME-NULL test.
- All existing-behavior tests still pass.
- `ApprovalTest.ThirtyDays` passes against the updated snapshot (unchanged by the null-name fix).

## Coverage status after adding tests

Every current-behavior scenario in the matrix is now **covered** — either by a focused unit test or by the `ThirtyDays` snapshot. The behaviors the snapshot could not isolate (backstage increments, the floor and cap edges, Sulfuras with unusual values, object identity, and the empty list) now have dedicated unit tests.

### Remaining gaps

These are minor and optional; the core rules are protected.

- **Extra backstage boundary at SellIn 11 with visible headroom (BP-11 at low Quality):** covered by the snapshot at 15/20 (+1), but not by a dedicated low-Quality unit test. The +1 base gain is low-risk, so this is optional.
- **Very high multi-day ordinary runs and other value combinations:** the rules are proven at their boundaries; exhaustive value sweeps are unnecessary.
- **Conjured behavior (CONJ-*):** implemented and fully covered by unit tests plus the updated snapshot. No longer a gap.
- **Reminder on the snapshot-covered rows:** these depend on the exact `ThirtyDays` inventory. If that inventory ever changes, re-check those rows.
