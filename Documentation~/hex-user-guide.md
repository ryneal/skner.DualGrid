# Hex Dual Grid — User Guide

The hex dual grid is the hexagonal counterpart to the rectangular dual grid system. It works the same way conceptually: you paint into a data tilemap and the library renders a visually richer tilemap automatically.

## What's different from the rectangular dual grid

| Aspect | Rectangular | Hex |
| --- | --- | --- |
| Data cell shape | Square | Hexagon |
| Neighbors that determine a render tile | 4 corners | 3 corners |
| Variants per rule tile | 16 (2⁴) | 16 = 8 up + 8 down |
| Render tilemaps per module | 1 | 2 (up- and down-triangle vertex lattices) |
| Hex orientation | n/a | Pointy-top or flat-top |

The hex grid's natural dual is a triangular grid. Unity's Tilemap does not support triangular layouts, so we render the up-pointing and down-pointing triangles using two separate hex tilemaps offset to the corresponding triangle centroids.

## Creating a hex dual grid tilemap

`GameObject > 2D Object > Hex Dual Grid Tilemap > Pointy-Top` (or `Flat-Top`).

This creates:

- A `Grid` with the chosen hex orientation
- A `Tilemap` child wired with a `HexDualGridTilemapModule` (the data tilemap, hidden)
- Two grandchildren `RenderTilemap_Up` and `RenderTilemap_Down` (the visible render tilemaps)

Assign a `HexDualGridRuleTile` to the module's `Rule Tile` field and a `DualGridDataTile` to its `Data Tile` field.

## Creating a hex rule tile

`Assets > Create > 2D > Tiles > Hex Dual Grid Rule Tile`.

The inspector shows two rows of 8 slots each:

- **Up Triangles** — sprites for the 8 mask values on the up-render tilemap
- **Down Triangles** — sprites for the 8 mask values on the down-render tilemap

### Mask reference

The mask is a 3-bit value packed from the 3 data hexes that share the render-triangle's vertex. The neighbors are ordered canonically by world position: lowest-y first, ties broken by lowest-x, then clockwise around the triangle.

| Mask | Binary | Neighbors filled |
| --- | --- | --- |
| 0 | 000 | none — cell is empty |
| 1 | 001 | neighbor [0] only |
| 2 | 010 | neighbor [1] only |
| 3 | 011 | neighbors [0] and [1] |
| 4 | 100 | neighbor [2] only |
| 5 | 101 | neighbors [0] and [2] |
| 6 | 110 | neighbors [1] and [2] |
| 7 | 111 | all three |

A useful authoring pattern: leave mask 0 empty, and fill 1, 2, 4 with a "single-corner" sprite, 3, 5, 6 with a "two-corner" sprite, and 7 with a "fully-filled" sprite. Up- and down-triangle variants are mirror images of each other.

## Auto-populating from a sliced texture

`AutoHexDualGridRuleTileProvider.ApplyAutoRules(tile, texture)` populates all 16 sprite slots from a sliced texture in canonical order:

- Sprites 0..7: up-triangle masks 0..7 (top row of the sheet, left to right)
- Sprites 8..15: down-triangle masks 0..7 (bottom row, left to right)

The provider sorts the sliced sprites by their rect (top-to-bottom, then left-to-right) and assigns them in order. A 4×4 sheet works well: top two rows = up triangles, bottom two = down triangles. The provider expects exactly 16 sprites in the texture; mismatches log an error.

## Orientation choice

Pointy-top hexes have a vertex at the top and bottom and flat edges on the sides — common in strategy games. Flat-top hexes have a flat edge on top and bottom and vertices on the sides — common in city-builders and some board games. The render math handles both transparently; the choice is purely visual.

## Limitations

- **Editor preview overlays** are not yet implemented for hex (the hover-time outline that the rect brush shows while painting). Painting, erasing, box, and flood-fill all work; only the in-progress preview is missing.
- **Render tilemap anchor offsets** are sensible defaults that may need a small per-project tweak depending on your chosen hex cell size and sprite pivot.
