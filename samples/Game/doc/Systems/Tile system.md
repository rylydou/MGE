Each tile has an internal id which is used to identify the tile. This is done by calling `GetHashCode` on the tile id.

Each tile also has a physical behavior which is self explanatory. Some common physical behaviors are:

- `Game.SolidTile`: A completely solid tile.
- `Game.PlatformTile`: A tile that is only solid on the top but can still be walked and jumped though, and jump-crouched though from the top.
- `SemisolidTile`: Like `PlatformTile` but cannot be jump crouched through from the top.

## File structure

Tiles are stored in `Content/Tiles`. They consist of 2 files plus the load file.

- `Block.meml`: The configuration of the tile, see [tile config](#tile-config) for more info.
- `Block.meml.ase`: The graphics for the tile.
- `Block.meml.load`: How the tile should be loaded, this should always be the same.

## Tile config

| Name       | Type     | Description                                                                                                                                                                                                                                                    |
| ---------- | -------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `id`       | `string` | The tile id, this should never change or there will be consequences. String format should only contain `a-z` and `-` to avoid confusion. If the tile is from a mod the id automatically be pretended to the id of the mod to avoid collisions with other mods. |
| `name`     | `string` | The tile name, this can change with no consequence.                                                                                                                                                                                                            |
| `tileSize` | `string` | Tile tile size, always `[ 8 8 ]`.                                                                                                                                                                                                                              |
