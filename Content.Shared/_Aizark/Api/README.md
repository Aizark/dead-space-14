# AizarkApi — composable trigger extensions

AizarkApi extends Content.Shared.Trigger without modifying TriggerSystem or RobustToolbox. It is a reusable dependency, **not** part of Titan candies. Content mods live in their own branches and prototype folders.

## Implemented in this branch (initial, not yet build-verified)

- `AizarkVariables`: per-entity integer key/value state (networked component).
- `AizarkVariableCondition`: comparison at `AttemptTriggerEvent`; missing variables fail closed.
- `AizarkVariableEffect`: numeric mutation at `TriggerEvent`, server-authoritative.
- `TargetUser`: optionally check/change the triggering user instead of the entity itself.
- `Key`: gate each component on the existing trigger key.

### Example: a finite-use device

```yaml
- type: entity
  id: ExampleAizarkCounter
  components:
  - type: AizarkVariables
    values:
      remaining: 10
  - type: TriggerOnUse
  - type: AizarkVariableCondition
    variable: remaining
    comparison: Greater
    value: 0
  - type: AizarkVariableEffect
    variable: remaining
    operation: Subtract
    value: 1
```

**Important:** This counts uses only. Spawning and replacing the exhausted entity are not yet wired. Do **not** attach two effects whose relative execution order matters and assume subscription order. For candy, one dedicated orchestrating effect should atomically: (1) validate stock, (2) choose a single weighted prototype, (3) spawn it server-side, (4) try to put it into a user's free hand (fallback: at feet), (5) decrement stock, (6) replace box on zero. This effect should reuse generic AizarkApi services, not candy-specific code.

## Planned core

- `AizarkSpawnToHandsOnTrigger`: a reusable effect with proto or entity-table selection, one spawn per activation, hands-first with floor fallback.
- `AizarkReplaceOnTrigger`: server-only replacement while preserving item position when possible.
- `AizarkVariableOperations`: multiple steps/conditions in a configurable list; explicit success/failure/empty keys.
- Extensible target resolver for `Self`, `User`, and explicit event targets, with strict validation.
- Automated tests: condition comparisons, missing keys, isolation between entities, zero and negative stock, saturation/overflow, divide by zero, server-only execution, dropping when both hands occupied, and chain behavior at 10th use.

## Constraints

- Do not edit upstream trigger implementation or engine.
- No global mutable variable store. Keys belong to individual entities.
- Avoid attaching variable state to player entities unless explicitly needed; do not ship hidden information to clients.
- Keep authoritative spawns and stock mutation on the server. No client prediction for weighted random entity tables.
- Use distinct `Aizark` type names and `Content.Shared._Aizark.Api` namespaces to avoid conflicts during upstream merges.
- Current first commit does **not** guarantee compilation or runtime behavior; validate with the project's build/tests before merging.
