---
name: upgrade-nuget-packages
description: Upgrade all declared NuGet packages to their latest stable versions, validate the repository, and open a pull request.
---

# Upgrade NuGet packages

Use this skill when asked to upgrade or update all NuGet packages in this repository. Do not perform the upgrade merely because this skill exists.

## Prepare

1. Inspect the worktree and current branch. If there are staged, unstaged, or untracked changes, stop and ask the user how to proceed; do not stash or overwrite their work.
2. Identify the repository's default branch and fetch its latest state. Create a new, uniquely named branch such as `chore/upgrade-nuget-packages-YYYYMMDD` from the fetched default branch. Never make the upgrade on the default branch or reuse an existing branch.
3. Inspect the current package declarations, NuGet configuration, SDK, solution, CI workflow, and any pull request template. This repository currently targets .NET 10, uses `Directory.Packages.props` for central versions, and builds and tests `CryptoFairPicker.sln` in Release. Confirm these details rather than assuming they have not changed.

## Upgrade

1. Inventory all tracked direct package version declarations, including every `PackageVersion` in `Directory.Packages.props`, even if it is not referenced by a project in the solution. Also check for project-level versions, overrides, other central package files, and tracked lock files. Do not add direct references solely to upgrade transitive dependencies.
2. Determine the latest stable version of each declared package from the enabled, applicable NuGet sources. With the .NET 10 SDK, `dotnet package search <package-id> --exact-match --format json` can inspect versions; `dotnet package list --project CryptoFairPicker.sln --outdated` is an additional check for referenced packages, not a complete inventory of unused central entries. Respect source mapping and any configured feeds. Do not treat a failed or inaccessible source, incomplete response, or inability to determine a version as an up-to-date result.
3. Update all declarations to the latest stable versions, including major versions. Preserve central package management, existing metadata, and consistent shared versions. Do not downgrade a package already newer than the latest stable version; report such cases and stop if the requested complete upgrade cannot be established. Include prereleases only if the user explicitly requests them.
4. Make only changes necessary for the upgrade, including narrowly scoped compatibility changes or regenerated tracked lock files if needed. Do not silently skip incompatible packages or change the target framework or SDK to make an upgrade pass. If the complete upgrade cannot be made and validated, stop and report the blocker instead of opening a partial PR.
5. Recheck every declaration against the discovered versions and review the diff. If all packages were already current and no changes are needed, report that and do not create an empty commit or PR.

## Validate and publish

Run the repository's current CI-equivalent restore, build, and tests. At present those commands are:

```bash
dotnet restore CryptoFairPicker.sln
dotnet build CryptoFairPicker.sln --configuration Release --no-restore
dotnet test CryptoFairPicker.sln --configuration Release --no-build --verbosity normal
```

Confirm that the expected test project ran and tests actually executed. Treat restore, build, test, source-access, and package resolution failures as blockers; address upgrade-related failures and rerun the affected checks. Never create a PR if any required check fails or could not run.

After all checks pass, review the final diff and stage only the intended upgrade changes. Commit them, push the new branch, and create a PR targeting the default branch using the available repository PR-creation tool, or `gh pr create` if appropriate. Follow the repository's PR template if one exists. Summarize the package version changes, any compatibility adjustments, and validation results in the PR. If pushing or PR creation is unavailable or requires approval, report the exact blocker rather than claiming success.
