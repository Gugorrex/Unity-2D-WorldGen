# Config
## Custom Tile
workaround to manually create tiles (unity doesn't allow otherwise)
## Config Relationship
<b>1</b> TilemapConfig : <b>n</b> HeightConfig <br>
``1 : n`` relationship
## Tilemap Config
There should only be one tilemap config for an entire world generation run.
Multiple tilemap configs can still be exchanged for each run for testing purposes.
## Height Config
There are multiple reasons why we can not define the tile heights directly in the 
tilemap config:
- some tiles may not be height specific, e.g. flowers, world structure tiles (like brick walls)
- multiple tiles can exist for a certain height, but their use depends on other
    circumstances e.g. biomes

Instead we want to choose a height config depending on which node of the
generation tree we are executing.

The height config always uses a subset of the tiles defined in tilemap config.

<hr>

## Future / Brainstorming
### Thoughts on height config setup for future biomes implementation:
- Some nodes of the generation tree are executed to determine in which biome we are.
    Then the corresponding height config is selected for the next node executions.
  - Contra: in this way height config / biomes may be entire chunk or nothing
    but we may want more organic biomes
- The generation tree config could save a map of ``biome : height config``
  - Pro: allows more organic biomes
  - Contra: needs deeper implementation inside generation algorithms, so that they
    can choose the corresponding height config in runtime / during execution.