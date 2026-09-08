---
inclusion: always
---

# Assignment

- Read `GildedRoseRequirements.md`
- This is a refactoring exercise. Although the existing code may not look particularly well-structured, it is functionally correct. Some of the requirements can be interpreted in more than one way, but the existing implementation should serve as your guide.


## Working together

- Address Gabriel as "Your Highness" in chat. Use simple language and explain unfamiliar terms.
- Work on the requested step only. Keep changes small and explain why they help. Gabriel reviews the work and decides when to continue.
- State assumptions clearly, point out possible mistakes, and report results honestly.

## Code changes

- Work only in `csharp.xUnit` and `.kiro` settings. Keep the existing .NET version and packages unless there is a specific reason to change them.
- When improving the code structure, keep its results unchanged. If the written requirements are unclear about existing behavior, follow the original code.
- Do not change `Item`, `Items`, or public method names, parameters, and return types.
- First test the existing behavior, then improve the code structure. Add Conjured items feature in a separate step afterward.
- Prefer small methods with clear names. Add extra classes only when they solve a specific problem.

## Tests and review

- Map each business rule to test scenarios and named tests. Check important boundaries, such as SellIn 1 and 0, and quality near 0 and 50.
- Explain expected results using the requirements and original code. Check both Quality and SellIn, and include checks that names remain unchanged and existing item objects are updated.
- Review saved expected program output before using it as a test baseline. Do not change assertions or saved output just to make a failing test pass.
- Run relevant tests after each meaningful change.
- End each implementation step with a short summary: what changed, why, test results, and any unanswered questions. Remind Gabriel to save the actual Kiro conversation and record his own review for the interview.
