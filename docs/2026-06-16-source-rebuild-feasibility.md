# UT4 client source-rebuild feasibility (Linux)

Why this matters: a source rebuild is the only clean path to enable the
in-game friends list on Linux (current binary stub at `UTLocalPlayer.cpp:6254`)
and is the route Epic intended. Binary patches work but accumulate.

## Inventory (what's on disk)

| Asset | Location | Status |
|-------|----------|--------|
| UT4 source | `/home/lucas/code/unrealtournament/UnrealTournament/` | 33 MB ✓ |
| UE4 engine source | `/home/lucas/code/unrealtournament/Engine/Source/` | bundled, ~883 MB ✓ |
| Linux toolchain configs | `Engine/Build/BatchFiles/Linux/Toolchain_x86-64/` | scripts only ✗ (binaries not bundled) |
| GitDependencies.exe | `Engine/Binaries/DotNET/GitDependencies.exe` | bundled ✓ (needs mono) |
| Engine deps manifest | `UnrealTournament/Build/Commit.gitdeps.xml` | ~107k entries, packs reference Epic CDN |

Engine version: **UE4 4.15.0 (++UT+Main @ CL 3228288)**.

## What's needed to build

### 1. Linux cross-toolchain (clang 3.9, centos7)

UE4 4.15 expects `v8_clang-3.9.0-centos7` in `$HOME/UnrealToolchains/v8_clang-3.9.0-centos7/` or set via `LINUX_MULTIARCH_ROOT`. Epic hosted this at `https://cdn.unrealengine.com/Toolchain_Linux/native-linux-v8_clang-3.9.0-centos7.tar.gz` (~700 MB). The URL **might still be alive** — the CDN host responded `403` (needs auth or moved). Test: `curl -I` to confirm.

If that toolchain is gone, modern clang (10+) sometimes builds UE 4.15 with minor patches. Several community ports exist.

### 2. Engine binary dependencies (~5–10 GB)

`GitDependencies.exe` fetches packs referenced in `Commit.gitdeps.xml`. Total uncompressed: hard to know without downloading; pack count suggests 5–10 GB. CDN base URL is hardcoded in the .exe; if Epic's bucket is still around, this works. If not, alternative mirrors (e.g. archive.org) may have it.

### 3. Mono runtime

UBT (Unreal Build Tool) is a C# binary that needs Mono to run on Linux. `mono` is in nixpkgs (~317 MB unpacked). Run via `nix-shell -p mono --run …`.

### 4. Disk + RAM

| Need | Size |
|------|------|
| Repo + Engine source | 1.1 GB (already on disk) |
| Toolchain | ~1.5 GB |
| GitDependencies binaries | ~5–10 GB |
| Intermediate build artifacts | ~20–30 GB |
| Final UT4 binary | ~316 MB |
| **Total disk** | **~40 GB minimum** |

Available: 149 GB free. Comfortable.

RAM: 16+ GB for parallel compile, otherwise reduce `-j$(nproc)` count.

## Estimated build time

Stage | Time on a modern laptop
------|------:
Setup.sh (toolchain + git deps) | 30–60 min (network bound)
GenerateProjectFiles.sh | 1 min
Engine + UT4 incremental | 2–4 hours
Packaging / cooking | 30 min

## Known blockers

1. **NixOS FHS**: UE4's build scripts assume Ubuntu paths (`/usr/bin/python`, `/usr/lib/libfoo.so`). Either run in `steam-run` / `nix-shell` with the right wrappers, or use `buildFHSUserEnv` to fake an Ubuntu environment.

2. **CDN URLs may be dead**. The 403 from `cdn.unrealengine.com` and `epicgames-download1.akamaized.net` is ambiguous — could be missing-auth or could be "bucket gone". Probe one pack URL specifically before committing time. If dead, this becomes much harder (need to source a mirror or build with a custom toolchain).

3. **Mono compatibility**: UE4 4.15 expects Mono 4.x; modern Mono 6.14 might surface compile-time warnings or runtime quirks in UBT/AutomationTool. Usually fixable with minor patches.

4. **`UnrealTournament.uproject` is missing** from this checkout. Look for it elsewhere — it might be `UnrealTournament/UnrealTournament.uproject` or need to be reconstructed. Without it, the build system doesn't know which project to compile.

## Minimal smoke test before committing

Before downloading 10 GB, run these 4 checks (~5 minutes):

```bash
# 1. Mono works
nix-shell -p mono --run "mono /home/lucas/code/unrealtournament/Engine/Binaries/DotNET/GitDependencies.exe --help"

# 2. CDN pack URL responds (replace HASH with one from Commit.gitdeps.xml)
curl -I "https://cdn.unrealengine.com/dependencies/3228288-d07aece5403b47899b8c4774b35ff00e/000f463db3afdc067e308c4fbe20d9e3de3786d1"

# 3. Linux toolchain URL responds
curl -I "https://cdn.unrealengine.com/Toolchain_Linux/native-linux-v8_clang-3.9.0-centos7.tar.gz"

# 4. uproject exists
ls /home/lucas/code/unrealtournament/UnrealTournament/UnrealTournament.uproject
```

If any of (1)–(3) fails, the build path needs new tooling. If (4) is missing, find it in another fork (timiimit, JimmieKJ, Letgam3rs).

## Smoke test results (run 2026-06-16)

| Check | Result |
|-------|--------|
| Mono works | ✅ Mono 6.14.1 from nixpkgs |
| GitDependencies.exe runs | ✅ `--help` shows full CLI |
| `UnrealTournament.uproject` exists | ✅ |
| **CDN reachable** | ❌ **`cdn.unrealengine.com/dependencies/...` returns 403 Forbidden** |
| Linux toolchain CDN | ❌ same — `cdn.unrealengine.com/Toolchain_Linux/...` returns 403 |
| Akamai mirror | ❌ `epicgames-download1.akamaized.net/...` returns 404 |

The CDN endpoints are dead. Source rebuild is **not feasible from upstream** today.

## Available paths now

1. **Mirror hunt**: someone in the UT4 community archived these deps. Check
   - archive.org WayBack saves of `cdn.unrealengine.com/dependencies/`
   - JimmieKJ's GitHub releases (the source we cloned)
   - timiimit's storage
   - UT4UU plugin authors (Letgam3rs) — they shipped patched binaries, may have build env
2. **Build engine from source-only**: skip GitDependencies, build everything in Engine/Source from scratch. Means recompiling `UnrealHeaderTool`, `ShaderCompileWorker`, etc. from C++ before the actual engine. Months of work, very fragile.
3. **Accept the binary-patch debt**: keep the player card patch (already applied) and the friends-list LD_PRELOAD trick if the user wants it. No source rebuild.
4. **Wait/ask**: post in the UT4 Discord asking if anyone has a tar of the 4.15 deps. Smaller community but they may have the artifacts.

## Recommendation

The CDN being dead is a hard blocker. The realistic options for tonight:

- **Path 3 (binary patches)** is the lowest-effort. Document it as the supported path; add a CONTRIBUTING note that source rebuild requires the missing deps.
- **Path 1 (mirror hunt)** is a 30–60 min investigation — try `archive.org`, post to Discord, check community forks.

The original ut4-install flake's "shippable" model already accepts binary distribution. Continuing on that model + accumulating selective binary patches is the pragmatic choice.
