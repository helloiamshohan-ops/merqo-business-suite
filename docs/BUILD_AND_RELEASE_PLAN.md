# Build and Release Plan

## 1. Environments

| Stage | Where | Gate |
|---|---|---|
| Local dev | Developer Windows PC | build + tests |
| CI (`ci.yml`) | GitHub Actions `windows-latest` | restore → build → test → publish → **launch the EXE** → upload artefacts |
| Release (`release.yml`) | `windows-latest`, triggered by a `v*` tag | all CI gates + installer + checksums + release |

The development sandbox used by the agent cannot compile .NET at all (no SDK, blocked Microsoft
download hosts). The Windows runner is therefore the authoritative build and verification
environment — this is stated in `docs/ENVIRONMENT_REPORT.md` rather than glossed over.

## 2. Artefacts

| Artefact | Produced by |
|---|---|
| `Tahabil.exe` (self-contained, single file, win-x64) | `dotnet publish` in `ci.yml` |
| `Tahabil-Setup-x.y.z.exe` | Inno Setup 6 (`installer/Tahabil.iss`) compiled on the runner |
| `TahabilLicenseGenerator.exe` | separate publish of `src/Merqo.LicenseGenerator` |
| `SHA256SUMS.txt` | checksum step over all artefacts |
| `RELEASE_NOTES.md` | generated from `CHANGELOG.md` + build metadata |

## 3. Versioning

- `Version` in `Directory.Build.props` is the single source; `VERSION` mirrors it.
- Tags: `vMAJOR.MINOR.PATCH`. Pre-release: `v0.9.0-rc.1`.
- Each artefact embeds `InformationalVersion = <version>+<commit-sha>`; the About screen and the
  release notes both show the commit.

## 4. Release gates (all must pass)

1. Restore + build clean on the runner (warnings reviewed, no new warnings).
2. All unit and integration tests pass (currently 44: 35 domain + 9 persistence).
3. Executable smoke test passes: the published EXE starts, creates `tahabil.db`, and is terminated.
4. No `*` floating package versions remain in `Directory.Packages.props`.
5. No secrets, keys, `.db`, logs or `bin/obj` content in the commit or artefact set.
6. `MASTER_CHECKLIST.md` has no `VERIFIED` row without evidence and no silently dropped requirement.
7. `KNOWN_ISSUES.md` is accurate — no known critical defect left undocumented.
8. Owner authorisation is recorded before publishing a public release (Part 22 of the brief).

## 5. Publication steps (owner authorises)

```pwsh
git tag v1.0.0
git push origin v1.0.0      # triggers release.yml
gh release view v1.0.0      # verify assets and checksums
```
The workflow creates a **draft** release, attaches artefacts and checksums, and requires a human to
publish it. Nothing in the pipeline can publish silently.

## 6. Reproducibility

- SDK pinned in `global.json`; package versions pinned exactly.
- `Deterministic=true`, `ContinuousIntegrationBuild=true` in CI.
- Commit SHA, runner OS and SDK version recorded in the release notes.
- Checksums published per artefact; a rebuild from the same tag is expected to match functionally,
  and byte-identity is reported honestly rather than promised.
