# 01 Logic Runtime

Live `LogicGraph` demo. Tweak Inspector inputs, watch per-frame
`Add(int, int)` and `Const(float)` results update via
`Function<TImpl, T1, T2, TOutput>` + the underlying `BoxedValue` Push/Pop
stack frame.

Now bundles **four** entry points — the original frame-by-frame smoke
(`LogicRuntimeDemo`), a one-shot deep-nesting expression run
(`NestedExpressionDemo`), a control-flow + shared-variable run
(`ControlFlowDemo`), and a `LogicWhile` accumulator
(`WhileLoopDemo`). The latter two share state across `LogicSequence`
statements via an explicit host-side variable pre-declaration — see
*Shared variables across `LogicSequence` statements* below for the
mechanism and its known limitation.

## Purpose

- Show the smallest viable wiring of `LogicGraph.Blockly` (host → source tree
  → `Set` / `SetSource` / `Call<TResult>` / `Destroy`).
- Demonstrate writing a 0-arity function (`ConstIntSource` / `ConstFloatSource`)
  and a 2-arity function (`AddIntSource`) using the `Function<TImpl, ...>`
  base classes — including how nested `Expression` slots compose into a single
  evaluable graph.
- Demonstrate deeper composition: multi-level `Function` nesting plus a 5-arity
  function (`Sum5IntsSource`) feeding a single `Call<int>()`
  (`NestedExpressionDemo`).
- Demonstrate control-flow primitives (`LogicSequence` + `LogicBranch` +
  `LogicSetVariable<int>` / `LogicGetVariable<int>`) sharing one variable
  across sibling statements (`ControlFlowDemo`).
- Demonstrate `LogicWhile` accumulating across iterations using the same
  shared-variable pattern (`WhileLoopDemo`).

## How to open

1. Open the bundled scene from a test project that mounts
   `Tests/01_LogicRuntime` directly, or copy this directory into the
   project's `Assets/` folder.
2. Open `Scenes/LogicRuntimeDemo.unity` (or follow the manual scene step
   below if Unity has not regenerated the scene yet — see *Scene*).
3. Press Play.

## Expected behaviour

### `LogicRuntimeDemo` (frame-by-frame smoke)

With the default values (`operandA = 3`, `operandB = 4`, `floatOperand = 1.5`),
on the first frame:

- `Last Int Sum` should read `7`
- `Last Float Value` should read `1.5`
- `Evaluation Count` increments by 1 every frame

While in Play mode, edit `Operand A` to `10` — within the next frame
`Last Int Sum` becomes `14`. Same for `Operand B` and `Float Operand`.

### `NestedExpressionDemo` (one-shot deep nesting)

On `Start()`:

- `Last Result` should read `43`
- Console prints
  `[NestedExpressionDemo] ((3+4)*(10-6)) + Sum5(1..5) = 43 (expected 43)`

### `ControlFlowDemo` (one-shot Sequence + Branch)

On `Start()`:

- `Last Result` should read `20`
- Console prints
  `[ControlFlowDemo] if (x>5) result = x*2 else result = x;  result = 20 (expected 20)`

The graph evaluates `int x = 10; int result; if (x > 5) result = x * 2;
else result = x;` — i.e. `result == 20`.

### `WhileLoopDemo` (one-shot While accumulator)

On `Start()`:

- `Last Sum` should read `55`
- Console prints
  `[WhileLoopDemo] sum 1..10 = 55 (expected 55)`

The graph evaluates `int sum = 0; int i = 1; int n = 10; while (i <= n)
{ sum = sum + i; i = i + 1; }` — i.e. `sum == 1 + 2 + ... + 10 == 55`.

## Knobs

On the `LogicRuntimeDemo` component:

- **Operand A / Operand B** — inputs to the `AddInt` graph.
- **Float Operand** — input to the standalone `ConstFloat` graph.

The three read-only fields **Last Int Sum**, **Last Float Value**, and
**Evaluation Count** mirror live evaluation results to the Inspector.

## Reference code

- `Scripts/LogicRuntimeDemo.cs` — `MonoBehaviour` entry point: holds the
  host, rebuilds and evaluates one int graph + one float graph per frame.
- `Scripts/NestedExpressionDemo.cs` — one-shot `Start()` demo evaluating
  `((3 + 4) * (10 - 6)) + Sum5(1, 2, 3, 4, 5) = 43` to validate
  multi-level `Function` nesting + 5-arity through a single `Call<int>()`.
- `Scripts/ControlFlowDemo.cs` — one-shot `Start()` demo combining
  `LogicSequence`, `LogicBranch`, `LogicSetVariableInt` /
  `LogicGetVariableInt` to evaluate `if (x > 5) result = x * 2; else
  result = x;` with `x = 10`. Uses host-side variable pre-declaration
  (see below) to share `x` / `result` across the two sibling statements.
- `Scripts/WhileLoopDemo.cs` — one-shot `Start()` demo using `LogicWhile`
  + `LogicSequence` body to accumulate `sum = 1 + 2 + ... + 10 = 55`.
  Same host-side pre-declaration pattern as `ControlFlowDemo` for the
  loop counter `i`, bound `n`, and accumulator `sum`.
- `Scripts/DemoExprNodes.cs` — node families used by the demos:
  - `ConstIntImpl` + `ConstIntSource : Function<ConstIntImpl, int>`
  - `ConstFloatImpl` + `ConstFloatSource : Function<ConstFloatImpl, float>`
  - `AddIntImpl` + `AddIntSource : Function<AddIntImpl, int, int, int>`
  - `SubtractIntImpl` + `SubtractIntSource` (int×int→int)
  - `MultiplyIntImpl` + `MultiplyIntSource` (int×int→int)
  - `GreaterThanIntImpl` + `GreaterThanIntSource` (int×int→bool)
  - `LessThanOrEqualIntImpl` + `LessThanOrEqualIntSource` (int×int→bool)

  The four new arithmetic / comparison sources mirror `AddIntSource`'s
  5-step protocol layout (`Initialize` → `EvaluateChildren` → `InitializeProperties`
  → `_impl.Evaluate(...)` → `Push(result)` driven by the `Function<TImpl, T1, T2, TOutput>`
  base). The `bool`-returning sources rely solely on `TOutput = bool` of the
  `Function` base — no extra signature attribute is required, since
  `[ExpressionSignature(typeof(bool))]` only applies to `LogicGraph` *slot*
  fields (e.g. `LogicBranch.condition`) and is checked editor-side.
- `Scripts/Arity5Smoke.cs` — `Sum5IntsSource` reused by `NestedExpressionDemo`.
- `Scripts/DemoSampleHost.cs` — `BlocklyHostBase` subclass overriding
  `NodeFactory` with a `ReflectionNodeFactory` so demo-defined sources resolve
  without depending on a generated `INodeMetadataProvider`.

## Scene

The demo expects a `LogicDemoRunner` GameObject with the
`LogicRuntimeDemo` component attached. If the provided scene is missing
(e.g. `.meta` not yet regenerated by Unity), create one manually:

1. `File → New Scene` (Basic 2D / 3D — either works).
2. Create an empty GameObject, name it `LogicDemoRunner`.
3. `Add Component → Logic Runtime Demo`.
4. Save as `Scenes/LogicRuntimeDemo.unity`.

To also exercise `NestedExpressionDemo`:

1. Add a second empty GameObject, name it `NestedExpressionRunner`.
2. `Add Component → Nested Expression Demo`.
3. Press Play — the result is logged once on `Start()` to the Console:
   `[NestedExpressionDemo] ((3+4)*(10-6)) + Sum5(1..5) = 43 (expected 43)`.
   The component also surfaces `lastResult` on the Inspector.

To also exercise `ControlFlowDemo`:

1. Add an empty GameObject, name it `ControlFlowRunner`.
2. `Add Component → Control Flow Demo`.
3. Press Play — Console:
   `[ControlFlowDemo] if (x>5) result = x*2 else result = x;  result = 20 (expected 20)`.
   The component surfaces `lastResult` on the Inspector.

To also exercise `WhileLoopDemo`:

1. Add an empty GameObject, name it `WhileLoopRunner`.
2. `Add Component → While Loop Demo`.
3. Press Play — Console:
   `[WhileLoopDemo] sum 1..10 = 55 (expected 55)`.
   The component surfaces `lastSum` on the Inspector.

## Note on per-frame rebuild

For demonstration clarity this sample rebuilds the source tree and the
`LogicGraph.Blockly` instance every frame, then `Destroy`s them. Production
use cases should reuse a single `LogicGraph.Blockly` and only feed new inputs
through variables / parameters — see the package core's `LogicSetVariable<T>`
/ `LogicGetVariable<T>` for the variable-channel pattern.

## Next steps

- Compose a deeper expression: `Add ( Add(a, b), c )` by nesting two
  `AddIntSource` instances.
- Replace `ConstIntSource` with `LogicGetVariableInt` (defined in
  `Vena.Blockly`) and feed values through `Variables` instead of fields.
- See sibling sample `02 Behavior Runtime` for the time-stepped side of the
  same engine.

## Shared variables across `LogicSequence` statements

`ControlFlowDemo` and `WhileLoopDemo` both rely on multiple sibling
statements reading and writing the same variables (e.g. `x`, `result`,
`sum`, `i`, `n`). The way they share state is worth calling out
because it is a **deliberate workaround**, not the long-term shape of
the API.

**Mechanism (option A — host pre-declaration).** After `SetSource(...)`
and before `Invoke()`, the demo host calls
`graph.SetVariable<int>(name, initialValue)` on the **root**
`LogicGraph.Blockly` for every variable that sibling statements need
to share. Those entries land in the root `ScopeChain.Variables`.
At runtime, sibling statements live in **child** Blockly scopes; their
`LogicSetVariable<T>` invokes `ScopeChain.ResolveWriteTarget`, which
walks the parent chain and finds the pre-declared name in the root
scope — so the write **passes through** to the root. Subsequent reads
from sibling scopes likewise resolve via the parent chain to the same
root entry. Result: all sibling statements observe one shared value
per name.

**Why this is a workaround.** As implemented in
`Runtime/Expression/LogicControl.cs`, `LogicSequence.Node` constructs a
*child* `LogicGraph.Blockly` per statement (via
`Blockly.CreateBlockly(...)`). Sibling Blocklies are not on each
other's parent chain, so state declared inside one statement is not
visible to its siblings. Without the host-side pre-declaration, the
lookup falls through to `default(T)` and the demos would silently
return `0` instead of `20` / `55`.

**Limitation.** This pattern requires C# host code to know every shared
variable name up front. A pure UGC graph authored at runtime by a
player has no such hook — there is no "host" beyond the graph itself.
Closing that gap requires changing `LogicSequence` so its statements
share one host scope rather than each owning a private child Blockly
(equivalently, `LogicSetVariable<T>` declaring at the host scope rather
than the local statement scope). That is a runtime architectural
change tracked as **task #19 (LogicSequence shared host scope)** in the
project backlog and is intentionally **out of scope** for these demos —
the demos exist to exercise `LogicGraph` control-flow primitives with
the runtime as it stands today.
