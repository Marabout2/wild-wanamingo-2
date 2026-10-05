# Contributing to Wild Wanamingo

Wild Wanamingo is a Fallout server built on [Space Station 14](https://github.com/space-wizards/space-station-14)
("Wizden", our upstream). Most of its content is ported from other projects, so this guide is mainly about
**where files go** and **how to credit them**. That keeps upstream merges painless and makes it obvious where
everything came from.

Read the upstream [CONTRIBUTING.md](CONTRIBUTING.md) too; its code and PR guidelines still apply.

## Where our content comes from

| Source | Repository | What we take |
|---|---|---|
| Misfits | https://github.com/Misfit-Sanctuary/nuclear-14 | Code, prototypes, sprites (Misfits is a Nuclear 14 fork) |
| Nuclear 14 (N14) | https://github.com/Vault-Overseers/nuclear-14 | Prototypes and sprites, reached through Misfits |
| Mojave Sun 13 (MS13) | https://github.com/Mojave-Sun/mojave-sun-13 | Sprites only (it's a BYOND game; no code is shared) |
| Wild Wanamingo | this repository | Everything new we write |

## Folder rules

**Textures and audio** keep their current folders: `Resources/Textures/{Misfits,Nuclear14,MojaveSun}` and
`Resources/Audio/{Misfits,Nuclear14}`. Each texture's `meta.json` already records its source and license.

**Everything else (code and markup)** is sorted by where it came from:

| The file is... | It goes in | Examples |
|---|---|---|
| A direct or modified copy from Misfits | `_Misfits` | `Content.Server/_Misfits/...`, `Resources/Prototypes/_Misfits/...`, `Resources/Locale/en-US/_Misfits/...` |
| A direct or modified copy of an N14 file | `_Nuclear14` | `Resources/Prototypes/_Nuclear14/...` |
| A Misfits copy of an upstream SS14 file or another fork's file | `_Misfits/<Kind>/Legacy/` | `Resources/Prototypes/_Misfits/Entities/Legacy/...`, `_Misfits/Decals/Legacy/...` |
| New, written for Wild Wanamingo | `_WW` | `Content.Server/_WW/...`, `Resources/Prototypes/_WW/...`, `Resources/Maps/_WW/...` |

This applies to C#, XAML, prototypes (YAML), locale (FTL) and maps. Prototypes that only *use* MS13 sprites
are new code, so they go in `_WW`.

Inside a prototype folder, keep the kind first (`_WW/Entities/...`, `_Misfits/Decals/...`). The GRIMP mapping
tool only scans `Prototypes/_<Fork>/{Tiles,Entities,Catalog,Decals}`, so anything else is invisible to mappers.

C# namespaces follow the folder: `Content.Server._Misfits.Sound`, `Content.Shared._WW.Something`.

## Prototype IDs

- **New** prototypes (entities, recipes, stacks, materials, tiles...) use the **`F14` prefix**, e.g.
  `F14Smelter`, `F14CableHV`, `F14ScrapAluminum`. Never start new IDs with `N14`, `MS13` or `Misfits`.
- **Copied** prototypes keep their original IDs (`N14WallMetal`, `MisfitsBannerChildrenOfAtom`...), so
  existing maps keep loading.

## Attributing copied code

Every copied file starts with a header naming the repository and the original path. If you changed it in a way
that matters (rewritten for a Wizden system, features dropped), say so on the next line.

YAML and FTL:

```yaml
# Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Resources/Prototypes/_Misfits/Entities/...
```

```yaml
# Ported from Nuclear 14 (https://github.com/Vault-Overseers/nuclear-14) via Misfits (https://github.com/Misfit-Sanctuary/nuclear-14):
# Resources/Prototypes/_Nuclear14/Reagents/Consumable/food.yml
# Rewritten for Wizden's metabolism: ...
```

C#:

```csharp
// Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Server/_Misfits/NPC/ProximityNPCSystem.cs
```

XAML:

```xml
<!-- Ported from Misfits (https://github.com/Misfit-Sanctuary/nuclear-14): Content.Client/_Misfits/... -->
```

New `_WW` files don't need a source header; a short comment on what the file is for is enough. Files whose art
comes from MS13 should say so, e.g. `# Sprites from Mojave Sun 13.`

## Sprites (RSIs)

- Every RSI needs a `meta.json` with `license` and `copyright`. The copyright line says where the art came from
  (repository, commit and original file).
- MS13 art is **CC-BY-NC-SA-3.0**. Keep that license on anything made from it, including edited or combined
  sprites (say what was changed, e.g. "pieces combined into SS14 connection states").
- Validate with `python RobustToolbox/Schemas/validate_rsis.py <path to .rsi>`.
- BYOND sprites anchor at the bottom-left and SS14 centers sprites on the tile, so oversized sprites need a
  `Sprite.offset`: 32x48 is `0, 0.25`, 32x64 is `0, 0.5`, 64x64 is `0.5, 0.5`, 64x48 is `0.5, 0.25`.

## Changing upstream (Wizden) files

Avoid it: every edit to an upstream file is a possible merge conflict. Prefer a new prototype that parents the
upstream one, or a new system in `_WW` that subscribes to upstream events.

When you must edit an upstream file, keep the change small and mark it so it's easy to find during merges:

```csharp
.Where(x => x.KeyCode != '\0') // Misfits Change: channels with no keycode can't be used with a prefix
```

```csharp
// WW Change start: <why>
...
// WW Change end
```

Use `Misfits Change` for changes ported from Misfits and `WW Change` for our own.

## Locale

Wizden requires locale keys (not plain text) for many names: stacks, materials, tiles, reagents, recipes. Put
new strings in `Resources/Locale/en-US/_WW/` and ported ones in the matching `_Misfits` or `_Nuclear14` folder.
Entity `name` and `description` can stay plain English.

## Code style

Follow upstream's style and analyzers. The ones Misfits code most often trips on:

- Systems and components are `partial`.
- `[Dependency] private SomeSystem _some = default!;` (not `readonly`).
- Use `ProtoId<T>` / `EntProtoId` instead of string literals when indexing prototypes.
- Use the `Entity<T>` event handler signature: `private void OnFoo(Entity<FooComponent> ent, ref FooEvent args)`.

## Before you commit

1. Build: `dotnet build -c Release`
2. YAML linter (use Release; Debug hits an upstream assert):
   `dotnet run --project Content.YAMLLinter/Content.YAMLLinter.csproj --no-build -c Release`
3. Tests for what you touched, at least the entity spawn tests:
   `dotnet test Content.IntegrationTests -c Release --no-build --filter "FullyQualifiedName~EntityTest|FullyQualifiedName~Tests._WW"`
   Add `Lathe`, `Material`, `Construction`, `PostMapInit` and so on when your change involves them.

## Maps

Wild Wanamingo maps live in `Resources/Maps/_WW/`. Maps reference prototypes by ID, which is why copied
prototypes keep their IDs. Check a map loads with
`dotnet test Content.IntegrationTests -c Release --no-build --filter "FullyQualifiedName~PostMapInitTest|FullyQualifiedName~NonGameMapsLoadable"`.

## Pulling upstream changes

The repository has an `upstream` remote for Wizden. Merge `upstream/master` regularly. Because our content
lives in underscored folders, conflicts should be limited to the marked `Misfits Change` / `WW Change` edits.
After merging, rebuild, run the linter and the tests above: upstream renames components and fields often, and
the linter is the quickest way to find what broke.
