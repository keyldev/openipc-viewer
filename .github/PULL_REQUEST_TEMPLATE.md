<!-- Keep it short. Commit messages and this template are in English. -->

## Summary

<!-- What does this PR do and why? -->

## Related

<!-- Closes #123, or the phase this belongs to (e.g. "Phase 6 — Recording"). -->

## Type

- [ ] Bug fix
- [ ] Feature
- [ ] Refactor / cleanup
- [ ] Docs / CI
- [ ] Other:

## Checklist

- [ ] Builds with **0 warnings** (`TreatWarningsAsErrors=true`).
- [ ] Tests pass (`dotnet test`); new Core logic has unit tests.
- [ ] No layering violation — `App` references `Core` only (Infrastructure / Video / Devices wired via DI in a head).
- [ ] Scope stays within one phase (didn't pull work from a later phase's "Не входит").
- [ ] README / docs updated if public commands, options, or setup changed.

## Platforms tested

<!-- Which heads did you actually run? e.g. Windows desktop; CI-only for the rest. -->

- [ ] Windows
- [ ] Linux
- [ ] macOS
- [ ] Android
- [ ] iOS
- [ ] CI build only

## Screenshots / notes

<!-- For UI changes, before/after screenshots help. -->
