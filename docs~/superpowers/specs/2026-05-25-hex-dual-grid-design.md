# Hex Dual Grid Tilemap — Design

Status: Draft
Date: 2026-05-25
Author: Ryan Neal

## Goal

Extend `skner.DualGrid` to support hexagonal tilemaps using the dual-grid technique, shipped alongside the existing rectangular system as a peer feature. Authoring ergonomics (rule tile inspector, auto-provider, brush, menu item) should match the rect system so users can adopt it without a new mental model.

## Background

The existing system stores tile identity in a Data Tilemap and renders to a Render Tilemap offset by half a cell in both axes. Each render cell looks at its 4 corner data neighbors, giving 2⁴ = 16 combinations encoded as variants on a `DualGridRuleTile`.

For a hex grid, every vertex is shared by exactly 3 hexes, so a "render cell" placed at a hex vertex is determined by 3 data neighbors → 2³ = 8 combinations. The set of all hex vertices forms a triangular lattice that decomposes into two offset hex sub-lattices: up-pointing and down-pointing triangle centroids.

Unity's Tilemap supports Rectangle, Hex (pointy-top and flat-top), and Isometric layouts but not triangular. We therefore represent the triangular render lattice as **two hex render tilemaps**, one per sub-lattice. This preserves Unity's native tilemap rendering, RuleTile integration, brush, palette, and inspector workflows.

## Non-goals

- Custom mesh/sprite renderer for true triangular cells.
- Migration tooling between rect and hex tilemaps.
- Runtime coexistence beyond independent module instances.
- Changes to the existing rect system's public API.

## Architecture

Parallel hex types living next to the existing rect types, sharing utilities only where geometry permits.

```
Runtime/
  Components/
    DualGridTilemapModule.cs            (existing)
    HexDualGridTilemapModule.cs         (new)
  Tiles/
    DualGridRuleTile.cs                 (existing)
    HexDualGridRuleTile.cs              (new — 8 combos, 3 neighbors)
    DualGridDataTile.cs                 (existing, reused as-is)
  Utils/
    DualGridUtils.cs                    (existing)
    HexDualGridUtils.cs                 (new — vertex/triangle math)
Editor/
  AutoHexDualGridRuleTileProvider.cs    (new — generates 8 rules from a tileset)
  Editors/HexDualGridRuleTileEditor.cs  (new — 8-variant authoring UI)
  Editors/HexDualGridTilemapModuleEditor.cs (new)
  Menus/HexDualGridTilemapMenu.cs       (new — Create > Hex Dual Grid Tilemap)
  HexDualGridBrush.cs                   (new — mirrors DualGridBrush)
```

`HexDualGridTilemapModule` owns:

- One Data Tilemap (Unity hex tilemap, pointy-top or flat-top).
- Two Render Tilemaps (Unity hex tilemaps), one for up-triangle centroids and one for down-triangle centroids.

Hex orientation (pointy-top vs flat-top) is read from the parent `Grid` component's `cellLayout`/`cellSwizzle`, so the module does not duplicate that setting.

## Geometry

`HexDualGridUtils` is a stateless static class exposing pure functions over axial/offset coordinates. Triangle-centroid offsets are computed once per orientation from the hex cell size.

Public API:

- `GetRenderCellsForDataCell(Vector3Int dataCell, HexOrientation orientation) → RenderCellSet`
  Returns the 6 render cells touched when a data cell changes: 3 up-triangle cells and 3 down-triangle cells.
- `GetDataNeighborsForRenderCell(Vector3Int renderCell, TriangleKind kind, HexOrientation orientation) → Vector3Int[3]`
  Returns the 3 data hex cells that determine a render tile, in a fixed canonical order (so the encoded mask is stable).
- `EncodeNeighborMask(bool a, bool b, bool c) → int`
  Packs three bools into 0..7 in the canonical order.
- `GetRenderTilemapOffset(TriangleKind kind, HexOrientation orientation, Vector3 cellSize) → Vector3`
  The world-space offset to apply to each render tilemap's transform so its cell centers land on triangle centroids.

`TriangleKind` is `{ Up, Down }`. `HexOrientation` is `{ PointyTop, FlatTop }`.

## Render update flow

Mirrors the existing module: the module `SetTile`s the `HexDualGridRuleTile` asset onto affected render cells, and Unity's tilemap pipeline calls `GetTileData` on the tile to resolve the actual sprite per cell. The module only decides **where** to refresh; the tile decides **what** to render.

1. Data cell painted or erased at `(q, r)`.
2. Module calls `HexDualGridUtils.GetRenderCellsForDataCell` to get the 6 affected render cells (3 up, 3 down).
3. For each affected cell, the module calls `RenderTilemap.SetTile(pos, hexRuleTile)` on the matching up- or down-render tilemap (clearing with `null` if all 3 neighbors are empty so the cell stays unrendered).
4. Unity then invokes `hexRuleTile.GetTileData(pos, tilemap, ref data)` per cell. The tile:
   a. Resolves the owning `HexDualGridTilemapModule` by walking from the `ITilemap` to its `GameObject`.
   b. Asks the module which `TriangleKind` this render tilemap represents (the module knows; the tile does not store this).
   c. Calls `HexDualGridUtils.GetDataNeighborsForRenderCell` to get the 3 contributing data cells.
   d. Reads filled-state from the module's data tilemap and calls `EncodeNeighborMask` for a 0..7 index.
   e. Returns the matching sprite from `upTriangleSprites` or `downTriangleSprites`.

This matches how `DualGridRuleTile.GetTileData` + `RuleMatches` already works in the rect system: the tile owns lookup, the module owns refresh dispatch. The two render tilemaps are refreshed independently in one pass.

## `HexDualGridRuleTile`

Subclasses `UnityEngine.Tilemaps.TileBase` directly (not `RuleTile`). `RuleTile`'s 3×3 neighbor model and `TilingRule` serialization assume rectangular geometry and add machinery we would not use; a leaner `TileBase` subclass is the right primitive.

Serialized fields:

- `Sprite[8] upTriangleSprites`
- `Sprite[8] downTriangleSprites`
- `Tile.ColliderType[8] upColliderTypes` (default `Sprite`)
- `Tile.ColliderType[8] downColliderTypes`
- Optional `GameObject[8]` per kind for instantiated objects, matching the rect rule tile's surface area.

Overrides:

- `GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)` — performs the lookup described in the render flow above and populates `tileData.sprite`, `tileData.colliderType`, and (optionally) `tileData.gameObject`.
- `RefreshTile` semantics inherited from `TileBase` are sufficient — the module triggers refresh by calling `SetTile`, which already invalidates the cell.

The tile finds its module via the `ITilemap.GetComponent<...>()` chain (same approach the rect tile uses to find its data tilemap).

## Authoring UX

**Custom inspector (`HexDualGridRuleTileEditor`):** two labeled rows of 8 slots — "Up Triangles" and "Down Triangles". Each slot shows a small triangle icon indicating which of the 3 corners are filled (mask 0..7) so the user can match sprites to patterns visually. Per-slot collider type dropdown matches the rect editor's layout.

**Auto-provider (`AutoHexDualGridRuleTileProvider`):** given a source texture sliced into 16 sprites (8 up + 8 down) in a documented canonical order, populate both arrays. Mirrors `AutoDualGridRuleTileProvider`'s ergonomics. Document the canonical layout in `Documentation~/hex-user-guide.md`.

**Brush (`HexDualGridBrush`):** mirrors `DualGridBrush`. Painting into the palette paints the data layer; the module's listener updates both render tilemaps.

**Menu:** `GameObject > 2D Object > Hex Dual Grid Tilemap` creates a Grid (hex layout, orientation chosen via submenu: Pointy-Top / Flat-Top), one data hex tilemap, two render hex tilemaps positioned at the correct triangle-centroid offsets, and a `HexDualGridTilemapModule` wired to all three.

## Persistent listener

Reuse the pattern from `DualGridTilemapPersistentListener` for change callbacks. A single listener on the data tilemap fans out to both render tilemaps via the module.

## Testing

- Edit-mode tests for `HexDualGridUtils` covering both orientations:
  - Round-trip: data cell → 6 render cells → each renders back to the originating data cell as one of its 3 neighbors.
  - Mask encoding stability across orientations.
  - Triangle-centroid offsets at unit cell size match analytically known values.
- Integration test: scripted paint into a module's data tilemap produces expected render-tile variants on both render tilemaps.
- Sample scene under `Samples~/HexDualGrid/` with placeholder tilesets for both orientations.

## Documentation

- New `Documentation~/hex-user-guide.md` covering setup, the 8-variant model, sprite canonical order, and orientation choice.
- README updated with a short section pointing to the hex guide alongside the existing rect guide.
- CHANGELOG entry under a new minor version.

## Security

No new attack surface: the asset runs in Editor and Player, reads serialized data and writes to tilemaps. No network, file I/O outside Unity's serializer, or user-supplied script paths. Input validation needed only on the auto-provider's source texture (reject if sprite count != 16) to avoid index-out-of-range at runtime.

## Open items deferred to implementation plan

- Exact canonical neighbor order for `GetDataNeighborsForRenderCell` (locked in alongside the first util test).
- Exact sprite sheet layout for the auto-provider (4×4? 8×2? user-facing decision, picked when drafting the user guide).
- Whether to expose a public C# event on the module for external listeners (parity check with the rect module — match whatever it does).
