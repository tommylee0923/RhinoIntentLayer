# RhinoIntentLayer

Inspired by Rhino.Inside.Revit open issue No. 1036 (https://github.com/mcneel/rhino.inside-revit/issues/1036)

A contract-driven BIM intent system built in C# that assigns structured building intents to Rhino geometry with validation and prepares clean data for downstream systems, mainly with Revit integration via Rhino.Inside.Revit planned.

Revit doesn't understand what a brep or a curve is in Rhino, but with RhinoIntentLayer, you could assign necessary attributes depending on the building intent (type) to smoothen the workflow between Rhino and Revit.

---

## Architecture

```
RhinoIntentLayer/
└── src/
    ├── Intent.Contract      → DTOs, validation models, JSON serialization
    ├── Intent.Core          → Validation engine (no Rhino dependency)
    ├── Intent.RevitStub     → Revit integration interfaces (planned)
    ├── Intent.RhinoLayer    → Rhino plugin: commands, UserText I/O, color feedback
    └── Intent.Core.Tests    → xUnit tests for the validation engine
```

**Intent.Contract** defines the shared data model and owns JSON serialization.

**Intent.Core** contains the validation engine. It can be compiled, tested, and run in complete isolation.

**Intent.RevitStub** defines integration interfaces for future Revit mapping without taking an actual Revit dependency.

**Intent.RhinoLayer** contains Rhino intent services commands. Such as `WallIntentService`.

---

## Rhino Commands

**`AssignWallIntent`** — select a brep or a curve, enter wall parameters, assigns and validates intent, stores JSON in UserText, applies color feedback.

**`InspectWallIntent`** — select a brep or a curve, prints a readable summary of the stored intent and validation result to the command line.

**Under development**

---

## Tech Stack

| | |
|---|---|
| Language | C# / .NET |
| Serialization | `System.Text.Json` |
| Unit testing | xUnit |
| Rhino plugin | RhinoCommon SDK (Rhino 8) |
| Data storage | Rhino UserText (inside `.3dm`) |

---

## Status

| Phase | | |
|---|---|---|
| 1 | Unit tests | ✅ |
| 2 | `AssignTYPEIntent` + `TYPEIntentService` + `InspectTYPEIntent`| ✅ |
| 3 | Standalone exporter (`intents.json`) | 🔲 Planned |
| 4 | Rhino.Inside.Revit bridge | 🔲 Planned |
| 5 | Eto panel UI | 🔲 Planned |

---

## Related

Developed alongside [IfcModelAuditor](https://github.com/tommylee0923/ifcqa-tool) — a multi-engine IFC validation platform. Where IfcModelAuditor demonstrates data pipeline design across Python and C#, RhinoIntentLayer demonstrates domain modeling, clean architecture, and BIM intent system design in a Revit-adjacent context.
