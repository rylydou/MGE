A stage is made up of multiple parts:

- Properties
- Tiles
- Background
- Objects
- Wiring

# Tiles

Each tile has a specific index it is assigned to in the palette. The palette can hold up to 255 tiles + air. The palette contains the hash of the actual tile id. Example:

1. You have a tile palette index of 4
2. So you look up the 4th item in the palette, the hash code of "dirt"
3. The tile is dirt

## Implementation

- The tile data is stored in `byte[width * height] _tileData`
- The tile palette is stored in `int[256] _tileDataPalette`

## Learn more
[[Tile system]]
