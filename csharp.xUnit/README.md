# Gilded Rose — C# xUnit solution

This is my solution to the Gilded Rose refactoring kata.
It cleans up the `UpdateQuality` method, protects the old behavior with tests, and adds the new **Conjured** item category.

## What the exercise required

- Refactor the messy `UpdateQuality` method without changing what it does.
- Add a new rule: **Conjured** items lose Quality twice as fast as normal items.
- Do **not** change the `Item` class or the `Items` property (they belong to another team), and do not change any public method names or signatures.

## What I changed

- Rewrote `UpdateQuality` into small, named methods (one per item category).
- Added shared helpers to raise and lower Quality safely.
- Added the Conjured rule.
- Added xUnit tests that pin down the original behavior before refactoring, then cover the new Conjured rule.
- Left `Item.cs`, the `Items` property, `Program.cs`, and all public signatures unchanged.

## Build, test, and run

Requires the **.NET 10 SDK**. Run these from the `csharp.xUnit` folder.

Build (Debug):

```cmd
dotnet build GildedRose.sln -c Debug
```

Run all tests:

```cmd
dotnet test GildedRose.sln
```

Run the demo program (prints the inventory for a number of days, e.g. 10):

```cmd
dotnet run --project GildedRose/GildedRose.csproj -- 10
```

The project targets **.NET 10** (`net10.0`), inherited from the starting code and kept as-is.

## Design

The update loop looks at each item's name and sends it to one small method per category:

- Ordinary items — lose 1 Quality per day, 2 per day after the sell date.
- Aged Brie — gains 1 per day, 2 per day after the sell date.
- Backstage passes — gain more as the concert nears (+2 at 10 days or fewer, +3 at 5 days or fewer), then drop to 0 once the concert has passed.
- Sulfuras — legendary. Left completely untouched (both Quality and SellIn).
- Conjured items — lose 2 per day, 4 per day after the sell date.

Two shared helpers keep the rules in one place:

- `IncreaseQuality` — adds 1, but never above 50.
- `DecreaseQuality` — subtracts 1, but never below 0.

Each category method calls these helpers the right number of times.
This keeps the "never below 0, never above 50" limits in a single spot instead of repeated everywhere.

## Conjured items

**Naming rule:** an item counts as Conjured when its `Name` starts with `"Conjured "` (with the trailing space), compared **case-sensitively**.

- `"Conjured Mana Cake"` → Conjured.
- `"conjured mana cake"` → not Conjured (wrong case), treated as ordinary.
- `"Conjured"` on its own → not Conjured (no trailing space), treated as ordinary.

The requirements only name the category, so this matching rule is a choice I made and documented (see the coverage matrix).
The trailing space avoids matching an unrelated word that merely begins with "Conjured".

**Quality change:** a Conjured item loses **2** Quality per day before its sell date and **4** per day once the sell date has passed, and never drops below 0.
That is exactly twice the rate of an ordinary item.

## How existing behavior was protected

Before refactoring, I added tests that lock in what the original code did, so the cleanup could not silently change any result:

- The `ThirtyDays` approval test runs the whole program for 30 days and compares the printed output against a saved file. It is the main safety net.
- Focused unit tests cover the tricky edges the snapshot cannot isolate on its
  own: the Quality floor at 0 and cap at 50, the backstage boundaries at 10/6/5
  days, Sulfuras keeping unusual values (no auto-correction), the same item
  object being updated in place, and an empty inventory.
- **Null names:** the original code never crashed on a `null` name. The new
  Conjured check could have thrown on a null name, so it guards against it and a
  test confirms a null-named item behaves as an ordinary item, matching the
  original.

## Why the saved output changed for Conjured

Adding the Conjured rule intentionally changes how "Conjured Mana Cake" degrades,
so the saved `ThirtyDays` output had to change too. A line-by-line comparison
confirmed that **only** the four Conjured Quality values for days 1 to 4 differ;
every other line (all other items, all SellIn values, all names) is identical.
This was the one deliberate, expected behavior change.

## Requirements and assumptions

Some requirements could be read in more than one way.
Where the wording was unclear, I chose an interpretation, used the original code as the guide, and pinned the choice with tests.
These were reviewed and agreed during the AI sessions (see the transcripts linked below).

- **How to recognize a Conjured item.**
  - Unclear: the requirements name the category but never say how a name is matched.
  - Choice: match names that start with `"Conjured "` (trailing space, case-sensitive).
  - Reflected in: the `IsConjured` check in `GildedRose.cs`, and tests
    `ItemWithLowercaseConjuredName_IsTreatedAsOrdinary` and
    `ItemNamedConjuredWithoutTrailingSpace_IsTreatedAsOrdinary`.

- **What "twice as fast" means after the sell date.**
  - Unclear: an ordinary item loses 2 per day after expiry, so "twice as fast" could mean the base rate only, or the after-expiry rate too.
  - Choice: Conjured loses 2 per day before expiry and 4 per day after, i.e. double the ordinary rate in both cases.
  - Reflected in: `UpdateConjuredItem` in `GildedRose.cs`, and tests
    `ConjuredItem_BeforeExpiry_DegradesByTwo`,
    `ConjuredItem_StartingAtZeroSellIn_DegradesByFour`, and
    `ConjuredItem_AlreadyPastExpiry_DegradesByFour`.

- **When the "after expiry" faster rate begins.**
  - Unclear: whether a starting SellIn of exactly 0 counts as "after expiry".
  - Choice: follow the original code's order — lower SellIn first, then apply the extra drop when SellIn is below 0.
    So a starting SellIn of 0 becomes -1 and takes the faster rate that same day.
  - Reflected in: every category method lowers SellIn before the `SellIn < 0` check, and tests 
    `ConjuredItem_StartingAtZeroSellIn_DegradesByFour` and
    `OrdinaryItem_AfterExpiry_QualityNeverGoesNegative`.

- **Sulfuras is never "corrected".**
  - Unclear: the notes say Sulfuras Quality is 80, which could tempt a rule that forces it to 80.
  - Choice: never touch Sulfuras at all, matching the original code. An unusual value is left as-is.
  - Reflected in: the Sulfuras branch does nothing, and test
    `Sulfuras_WithUnusualValues_KeepsBothValuesUnchanged`.

- **A name that matches no special rule.**
  - Unclear: how to treat any other name.
  - Choice: treat it as an ordinary item, matching the original code.
  - Reflected in: the fall-through to `UpdateOrdinaryItem`, and the ordinary-item tests.

## How Kiro was used

I used Kiro as a pair-programming partner. Kiro helped write the tests, refactor `UpdateQuality` into the per-category design, and implement the Conjured rule.
I reviewed and approved each step.
The full conversations are saved in the transcripts linked below.

## Related documents

- Behavior coverage matrix: [docs/behavior-coverage.md](docs/behavior-coverage.md)
- AI session transcripts:
  - [Session 1](docs/ai%20session%20transcripts/session%201.txt)
  - [Session 2](docs/ai%20session%20transcripts/session%202.txt)
