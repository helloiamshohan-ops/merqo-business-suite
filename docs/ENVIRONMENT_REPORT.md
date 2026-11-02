# Phase 0 — Repository and Environment Inspection Report

Date: 2026-11-02
Author: autonomous engineering agent (Arena Agent Mode)

## 1. Repository inspection

| Item | Observed value | Verified by |
|---|---|---|
| Repository | `helloiamshohan-ops/merqo-business-suite` | `gh repo view` |
| Visibility | Public | `gh repo view --json isPrivate` → `false` |
| Default branch | `main` (initial commit `a0495ba`) | `git log` |
| Working branch | `arena/fce4c4bd-merqo-business-suite` | `git branch` |
| Initial contents | `README.md` only | `ls -la` |
| GitHub CLI identity | `helloiamshohan-ops` (HTTPS token auth) | `gh auth status` |
| Push access | Confirmed — branch pushed, workflow triggered | push exit code 0 + run list |
| Actions | Enabled and executing workflows | run `41524120405` |
| Issue tracker | Enabled; issue **#1** created as the automated CI diagnostics feed | `gh issue create` |
| Releases | None yet | `gh release list` (empty) |

`GET /repos/:owner/:repo/actions/permissions` returns HTTP 403 for this token, so repository-level
Actions policy (e.g. whether fork PRs may run workflows) cannot be read programmatically.
Documented as a limitation; it does not block development.

## 2. Development sandbox capabilities

| Capability | Result |
|---|---|
| OS | Debian 12 (Linux x64), 2 vCPU, 4 GB RAM, 20 GB disk |
| .NET SDK | **Not installed** |
| Install .NET locally | **Impossible** — `dotnet.microsoft.com` and `builds.dotnet.microsoft.com` are unreachable (TLS handshake blocked by the sandbox egress allow-list) |
| Allowed egress hosts | `github.com`, `codeload.github.com`, `api.github.com`, `registry.npmjs.org`, `pypi.org`, `files.pythonhosted.org` |
| Node / Python / gcc | Present (not used for this product) |
| Wine / Mono / MSBuild / NSIS | Absent |
| GUI display | None (headless) |
| WPF buildability on Linux | Not possible (WPF is Windows-only) |
| Reading raw Actions logs | **Blocked** — log downloads redirect to `*.blob.core.windows.net`, which the allow-list rejects |

## 3. Consequence: where the product is really built and verified

Because the sandbox cannot compile .NET or run WPF at all, all compilation, automated testing,
packaging and executable execution happens on **GitHub Actions `windows-latest` runners**, which are
genuine Windows machines with a real .NET SDK:

- `ci.yml` restores, builds, runs the full test suite, publishes a self-contained `Tahabil.exe`,
  **launches that executable**, asserts that it created its SQLite database, and uploads the artefacts.
- Because raw logs are not readable from the sandbox, `ci.yml` posts build/test logs to issue #1
  (`CI diagnostics feed`) via the GitHub API whenever a run fails. This is how failures are triaged.

### What is therefore NOT verified, and will be stated honestly everywhere

1. No physical thermal printer, barcode scanner or shop computer is available. Receipt layout is
   verified by rendering raster output and PDF pages and inspecting them, plus ESC/POS byte-level
   tests — never by claiming a physical print was observed.
2. No Windows desktop session with a human is available; UI verification is done via automation
   (launch + window presence + screenshot capture through the runner where possible) and by
   deterministic layout review, not by a human clicking every control.
3. Physical installer runs (UAC, shortcuts on a real user profile) are executed by the Windows
   runner in silent mode; interactive installation is documented for the owner to perform.

These are recorded as external verification limits in `KNOWN_ISSUES.md`, and are never converted
into false "verified" claims in `MASTER_CHECKLIST.md`.

## 4. Permissions and authorisation boundaries

- The agent can commit, push and run workflows on the working branch.
- The agent must not delete the repository, change its visibility, or publish a release without
  explicit owner authorisation (Part 22 of the brief). Release publication is therefore gated behind
  `docs/RELEASE_GATES.md` and an explicit owner go-ahead.
- No signing private key exists yet and none will ever be generated inside the sandbox or committed.

## 5. Toolchain decisions confirmed by the environment

- Target framework: **.NET 10 LTS** (`net10.0` / `net10.0-windows`). .NET 8 reaches end of support on
  2026-11-10, so it is rejected for a lifetime-licensed product.
- Installer toolchain: **Inno Setup 6** via Chocolatey on the Windows runner (discovery job confirmed
  `choco install innosetup` succeeds and produces `ISCC.exe`).
- Package versions are centralised in `Directory.Packages.props` and will be pinned to exact builds
  before the first release.

## 6. Exit criteria for Phase 0 — status

| Criterion | Status |
|---|---|
| Repository identified and permissions verified | **DONE** |
| Build environment identified | **DONE** (Windows runner, documented) |
| Testing tools identified | **DONE** (`dotnet test`, xunit, SQLite integration tests) |
| Limitations recorded | **DONE** (this document + `KNOWN_ISSUES.md`) |
| Initial project documentation created | **DONE** |
