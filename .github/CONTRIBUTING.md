# Contributing

OpenIPC Viewer is a side-project NVR-style viewer for OpenIPC cameras.
Contributions welcome — please follow the conventions below to keep
review cycles short.

## Branches and PRs

- Work on a feature branch off `main`. Branch names: `phase-NN-short-slug`
  for phase-bound work, `fix-short-slug` or `feat-short-slug` otherwise.
- Open the PR against `main`. Squash-merge is the default; the squash
  commit message should explain the *why*, not restate the diff.
- Keep PRs small. One phase sub-step per PR (e.g. "Phase 9c: in-process
  recording + foreground service on Android" is one PR; the touch-UX
  follow-up is another).

## Build expectations

- `dotnet build OpenIPC.Viewer.slnx` must finish with **0 warnings,
  0 errors**. `TreatWarningsAsErrors=true` is enforced.
- `dotnet test OpenIPC.Viewer.slnx --no-build` must pass; the MediaMTX
  integration test auto-skips when the container isn't running.
- CI runs the matrix on every code push (docs-only pushes are skipped);
  don't ignore red status.

## Code style

- Code, identifiers, log messages, commit messages: English.
- Phase planning docs (`phase-NN-*.md`): Russian.
- One blank line between members; sealed `partial` classes when XAML
  code-behind or `CommunityToolkit.Mvvm` source-gen is involved.
- Comments only where the *why* is non-obvious; well-named identifiers
  carry the *what*.

## Architecture rules (load-bearing)

- `App` references `Core` only. Don't add references to
  Infrastructure / Video / Devices from `App`.
- The platform trio (`IFileSystem` / `ISecretsStore` /
  `IHwDecoderFactory`) is wired per-platform in each head's
  `Composition.cs`. Shared registrations belong in
  `OpenIPC.Viewer.Composition.SharedComposition`.
- Don't blur work from later phases into earlier ones — each
  `phase-NN-*.md` has a **Не входит** section listing what's
  deliberately out of scope.

## Reporting issues

Open one in the GitHub issue tracker — pick the **Bug report** or
**Feature request** form and fill in the fields. For crash reports,
attach the most recent log file from the `logs/` folder in your
platform's AppData root (paths are in the README's *User data* section).
