# Soak ID Mapping Methodology

This document records how the `Brewer's Bounty` soak requirement IDs were derived and why the mapping in [`bmw_web/Services/AchievementPlanner.cs`](bmw_web/Services/AchievementPlanner.cs) is authoritative for the base-game achievement requirements verified below.

The key distinction is:

- save samples exposed inconsistencies in the previous table
- the exact `requirement id -> soak item` mapping came from the game's own runtime protobuf tables

The IDs were not finalized from public guide ordering, screenshots, or save-state guesswork.

## Bottom line

The mapping came from this join:

1. extract `AchievementDesc.data` and `ItemDesc.data` from the installed game
2. parse them with the game's protobuf runtime assemblies
3. read achievement `81078` (`Brewer's Bounty`)
4. take each value in `AchievementDesc.RequirementId`
5. look up that same numeric value in `ItemDesc.Id`

That produced the exact internal mapping for all 27 soak IDs.

Supporting inputs included:

- save outputs showed inconsistencies in the existing table
- decompiling runtime DLLs told us which protobuf types and files to read
- public English guides were used only to label the already-identified items in standard English

## Verification snapshot

The extraction and join were rerun on 2026-07-18 against the locally installed Steam copy:

- Steam app ID: `2358720`
- Steam build ID: `21393610`
- CUE4Parse NuGet package: `1.2.2.202607`
- CUE4Parse game version: `EGame.GAME_BlackMythWukong`
- achievement row: `Id == 81078`, `RequirementType == ProgressGainItem`
- requirement count: exactly 27
- requirement IDs: the consecutive sequence `2301` through `2327`

The mounted archives contained one achievement table and two item-table variants:

| Runtime file | Source archive | Bytes | SHA-256 |
| --- | --- | ---: | --- |
| `AchievementDesc.data` | `pakchunk16-Windows.pak` | 14,397 | `219E5A854DB2C5C05DEE7B0FBF20967A50D1F7889E827D6374776A2B5A8E22A5` |
| `ItemDesc.data` | `pakchunk16-Windows.pak` | 135,093 | `E497392680A30644D9EA51B5B413439995459D41F6EE5EE6ADFDDC2E43E3C1B5` |
| `ItemDesc.data` | `pakchunk26-Windows_P.pak` | 135,129 | `FD3853A54728893DD3BC7594FB741916D164D2FA0B98023084B20A731DA20CD1` |

Both item-table variants contained 952 rows and resolved all 27 requirement IDs to the same Chinese names shown in the final mapping. This matters because a patched archive can override or supplement a base-game runtime table; a future verification should inspect every matching table entry or use the extractor's patch-priority behavior explicitly.

The extracted `.data` files are proprietary game data and are intentionally not committed. The build ID, tool version, byte counts, and hashes above make it possible to identify whether a future re-extraction is comparing the same inputs.

## Evidence hierarchy

The process used several kinds of evidence. They do not all carry the same weight.

1. Highest confidence: extracted runtime data from the installed game
2. High confidence: parsing that data with the game's own protobuf model assemblies
3. Medium confidence: contradictions observed in real save outputs
4. Lower confidence: public English guide ordering alone

Only levels 1 and 2 were used to determine the final IDs. The 2026-07-18 verification snapshot above independently reproduced the same result from the installed game archives.

## Why guide ordering was insufficient

The original planner table mixed together:

- verified English soak names and locations
- inferred internal requirement ID ordering
- placeholder rows for unresolved entries

For an achievement like `Brewer's Bounty`, that leaves room for drift, because public guides solve the player-facing identity problem but not the internal `RequirementId` ordering inside achievement `81078`.

The repo therefore had two distinct issues:

- unresolved rows such as `Soak: Unverified ...`
- rows that were attached to the wrong IDs

## What save evidence established

Before extracting runtime tables, sample save outputs were useful as a consistency check. They showed that the planner's existing mapping was not internally consistent.

One clear contradiction came from the local `near_perfect.sav` regression sample:

- its `Brewer's Bounty` row had 26 of 27 requirements and included ID `2327`
- the one missing ID was `2301`
- player-facing guides place `Guanyin's Willow Leaf` behind the New Game+ Shen Monkey shop
- therefore the old `2327 -> Guanyin's Willow Leaf` assignment was inconsistent with the sample; the direct table join later proved that `2301` is `Guanyin's Willow Leaf` and `2327` is `Goat Skull`

The local save corpus is intentionally untracked, so this observation is supporting evidence rather than the reproducible source of truth.

That was enough to justify moving from inference to direct table extraction.

The saves did **not** establish the full ordering with certainty. Save evidence can rule mappings out and suggest candidates, but it cannot by itself guarantee the complete internal `RequirementId` sequence. That is why the full mapping came from extracted game data.

## Runtime data source

The repo already vendors the runtime protobuf assemblies under:

- [`vendor/blackwukong-dlls/Google.Protobuf.dll`](vendor/blackwukong-dlls/Google.Protobuf.dll)
- [`vendor/blackwukong-dlls/Protobuf.RunTime.dll`](vendor/blackwukong-dlls/Protobuf.RunTime.dll)

Inspection of those assemblies showed that the game exposes protobuf table types including:

- `ResB1.TBAchievementDesc`
- `ResB1.AchievementDesc`
- `ResB1.TBItemDesc`
- `ResB1.ItemDesc`
- `ResB1.TBWineDesc`
- `ResB1.WineDesc`

This established two useful points:

1. the achievement and item metadata live in protobuf-backed `.data` tables
2. the vendored DLLs already know how to parse those tables once extracted from the game

From there, the workflow was no longer "infer IDs from guides" but "extract the tables and join them directly".

## Extraction workflow

The installed game was mounted from the local Steam install under a path like:

- `...\SteamLibrary\steamapps\common\BlackMythWukong\b1\Content\Paks`

The verified extraction used the official [`CUE4Parse`](https://github.com/FabianFG/CUE4Parse) package with explicit Black Myth support. Its [`EGame` source](https://github.com/FabianFG/CUE4Parse/blob/master/CUE4Parse/UE4/Versions/EGame.cs) defines `GAME_BlackMythWukong` as a game-specific Unreal Engine 5.0 variant, so the workflow should select that value rather than assuming a generic UE5 profile is equivalent.

The AES key used for the pak mount was:

```text
0xA896068444F496956900542A215367688B49B19C2537FCD2743D8585BA1EB128
```

Once mounted successfully, the runtime protobuf tables of interest were:

- `b1/Content/00Main/PBTable/Runtime/AchievementDesc.data`
- `b1/Content/00Main/PBTable/Runtime/ItemDesc.data`
- `b1/Content/00Main/PBTable/Runtime/WineDesc.data`

Only the first two were required to recover the exact soak IDs. `WineDesc.data` was extracted as auxiliary reference material, but the final `RequirementId` mapping did not depend on it.

## Exact parse-and-join method

After extraction, the exact ID recovery logic was:

1. load `AchievementDesc.data` as `ResB1.TBAchievementDesc`
2. load `ItemDesc.data` as `ResB1.TBItemDesc`
3. find the `AchievementDesc` row with `Id == 81078`
4. read its `RequirementId` repeated field
5. build an `ItemDesc` lookup keyed by `ItemDesc.Id`
6. resolve each requirement ID through that item lookup

That join is the decisive step because the IDs are shared directly between the achievement table and the item table.

One minimal PowerShell prototype is:

```powershell
Add-Type -Path '.\vendor\blackwukong-dlls\Google.Protobuf.dll'
Add-Type -Path '.\vendor\blackwukong-dlls\Protobuf.RunTime.dll'

$achBytes = [IO.File]::ReadAllBytes('<path-to-extracted-AchievementDesc.data>')
$itemBytes = [IO.File]::ReadAllBytes('<path-to-extracted-ItemDesc.data>')

$achTable = [ResB1.TBAchievementDesc]::Parser.ParseFrom($achBytes)
$itemTable = [ResB1.TBItemDesc]::Parser.ParseFrom($itemBytes)

$itemMap = @{}
foreach ($item in $itemTable.List) {
    $itemMap[$item.Id] = $item
}

$achievement = $achTable.List | Where-Object { $_.Id -eq 81078 }

foreach ($requirementId in $achievement.RequirementId) {
    $item = $itemMap[$requirementId]
    [PSCustomObject]@{
        RequirementId = $requirementId
        ChineseName   = $item.Name
        BriefDesc     = $item.BriefDesc
        Source        = $item.Source
    }
}
```

The important detail is that the ID sequence came out of the achievement row itself, and each ID was resolved by the item table, not by external guide ordering.

## What came directly from game data

The extracted runtime data gave us, directly:

- the achievement row for `81078`
- the full set of 27 soak requirement IDs
- the achievement row's 27 requirement IDs, currently the consecutive sequence `2301` through `2327`
- the Chinese item name attached to each ID
- the item metadata fields such as `BriefDesc`, `EffectDesc`, and `Source`

This part of the result came directly from game data.

## What still required a crosswalk

The extracted item rows exposed Chinese in-game names, not the player-facing English names used elsewhere in the repo.

So there was a separate labeling step:

1. take the already-resolved item identity from `ItemDesc`
2. match that item to the standard English soak name used by players and guides
3. update the planner with the correct English label for that already-known item

That means:

- `ID -> item identity` came from runtime tables
- `item identity -> English display name` came from crosswalking

This distinction matters because the second step can be revised for wording without changing the authoritative internal ID mapping.

The English vocabulary and acquisition descriptions were rechecked against the [PowerPyx soak guide](https://www.powerpyx.com/black-myth-wukong-all-soaks-locations/) and the [PlayStationTrophies soak guide](https://www.playstationtrophies.org/game/black-myth-wu-kong/guide/all-soak-locations). Those guides corroborate player-facing names and routes only; they are not evidence for the internal numeric IDs.

## Final mapping

| ID | Chinese name from `ItemDesc.data` | English name used in planner |
| --- | --- | --- |
| 2301 | 净瓶柳叶 | Guanyin's Willow Leaf |
| 2302 | 百花蕤 | Flower Primes |
| 2303 | 龟泪 | Turtle Tear |
| 2304 | 困龙须 | Stranded Loong's Whisker |
| 2305 | 灵台药苗 | Mount Lingtai Seedlings |
| 2306 | 十二重楼胶 | Breath of Fire |
| 2307 | 瑶池莲子 | Celestial Lotus Seeds |
| 2308 | 不老藤 | Undying Vine |
| 2309 | 虎舍利 | Tiger Relic |
| 2310 | 梭罗琼芽 | Laurel Buds |
| 2311 | 甜雪 | Sweet Ice |
| 2312 | 霹雳角 | Thunderbolt Horn |
| 2313 | 倒马毒钩 | Deathstinger |
| 2314 | 紫纹缃核 | Purple-Veined Peach Pit |
| 2315 | 蜂山石髓 | Bee Mountain Stone |
| 2316 | 铁弹 | Iron Pellet |
| 2317 | 瞌睡虫蜕 | Slumbering Beetle Husk |
| 2318 | 铜丸 | Copper Pill |
| 2319 | 血杞子 | Goji Shoots |
| 2320 | 清虚道果 | Fruit of Dao |
| 2321 | 火焰丹头 | Flame Mediator |
| 2322 | 双冠血 | Double-Combed Rooster Blood |
| 2323 | 胆中珠 | Gall Gem |
| 2324 | 蕙性兰 | Graceful Orchid |
| 2325 | 嫩玉藕 | Tender Jade Lotus |
| 2326 | 铁骨银参 | Steel Ginseng |
| 2327 | 青山骨 | Goat Skull |

## Why the fix replaced the whole table

This update did not only fill the placeholder rows. It also corrected rows that were attached to the wrong IDs.

Examples of corrected assignments:

- `2304` is `Stranded Loong's Whisker`, not `Steel Ginseng`
- `2306` is `Breath of Fire`, not `Tiger Relic`
- `2311` is `Sweet Ice`, not `Stranded Loong's Whisker`
- `2321` is `Flame Mediator`, not `Double-Combed Rooster Blood`
- `2324` is `Graceful Orchid`, not `Undying Vine`
- `2326` is `Steel Ginseng`, not `Graceful Orchid`
- `2327` is `Goat Skull`, not `Guanyin's Willow Leaf`

That is why the update replaced the full soak table in the planner instead of patching only the unresolved names.

## Reproducibility checklist

If this needs to be re-verified in the future, the minimum reproducible workflow is:

1. use a local Black Myth install under `...\BlackMythWukong\b1\Content\Paks`
2. record the Steam build ID and exact extractor version
3. mount the paks with a parser that explicitly supports `GAME_BlackMythWukong`
4. extract every matching `AchievementDesc.data` and `ItemDesc.data` entry, including patched archive variants
5. record each extracted file's source archive, byte count, and SHA-256 hash
6. parse them with:
   - [`vendor/blackwukong-dlls/Google.Protobuf.dll`](vendor/blackwukong-dlls/Google.Protobuf.dll)
   - [`vendor/blackwukong-dlls/Protobuf.RunTime.dll`](vendor/blackwukong-dlls/Protobuf.RunTime.dll)
7. read achievement `81078` and confirm its requirement type and count
8. resolve each `RequirementId` through every applicable `ItemDesc.Id` table
9. compare the result with the planner table
10. only after that, assign or revise the English display labels

If the direct `AchievementDesc.RequirementId -> ItemDesc.Id` join is skipped and the list is reconstructed from public guide ordering alone, the same off-by-several-ID mismatch can recur.

## Repo update and validation

The corrected mapping was written into:

- [`bmw_web/Services/AchievementPlanner.cs`](bmw_web/Services/AchievementPlanner.cs)

The web project was validated after the mapping change with:

```powershell
dotnet build .\bmw_web\bmw_web.csproj -warnaserror -o <temp-output-dir>
```

That build succeeded.

## Final takeaway

The exact soak IDs came from the game's runtime tables, specifically the direct relationship between `AchievementDesc.RequirementId` and `ItemDesc.Id`.

Save samples were useful for consistency checks, but they were not the source of the final mapping.
