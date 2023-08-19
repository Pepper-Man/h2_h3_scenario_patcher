
# H2 to H3 Scenario Converter
Converts data from an exported H2 scenario and transplants it into your ported H3 scenario!

# Currently converts:
- H2 multiplayer starting locations to H3 respawn point scenery. Includes team data.
- Weapon palette and placements. Weapon palette references will attempt to automatically use existing H3 weapon tags where applicable. Grenades included.
- All scenery types and placements, including variant names. Scenery type filepaths still use the H2 filepaths - change the path(s) in Guerilla once you have ported the item(s)!
- All trigger volumes, including names.
- All vehicle types and placements, including variant names. Vehicle palette references will attempt to automatically use existing H3 vehicle tags where applicable.
- All crates, including variant names. Crate type filepaths still use the H2 filepaths - change the path(s) in Guerilla once you have ported the item(s)!
- All (netgame) gamemode items (CTF flag spawns, territories, bomb spawns/goals, teleporter sender/receivers etc etc) to H3 gametype crates. Unused or unapplicable gametype objects, such as race checkpoints and headhunter bins, are included but replaced with temporary forerunner core crates for easy identification.
- All decal placements and types. Due to system incompatibilities between engines, decals may appear rotated and/or stretched incorrectly. To fix stretching, simply touch the rotation handle. Rotate with the handle to fix rotations where necessary.

# Requirements
* Requires [.NET 4.8 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48)

# Usage
* Download the latest release, or compile your own version.
* Extract H2 scenario to XML with `tool export-tag-to-xml`.
* Make sure the destination H3 scenario has no entries in the:
	* Scenery block
	* Scenery palette block
	* Weapons block
	* Weapons palette block
	* Trigger volume block
	* Vehicle block
	* Vehicle palette block
	* Crate block
	* Crate palette block
	* Decal block
	* Decal palette block
 * This is to avoid issues with the tool. If you have existing data you wish to keep, make a backup of your scenario, run the tool, then use Guerilla to transfer the converted block data over into your old scenario tag:
* I cannot distribute the required ManagedBlam.dll, so you will need to either:
    * Copy your Halo 3 ManagedBlam.dll (found in "H3EK\bin") into the same folder as this exe
    * Alternatively, simply place the files of this program directly into your "H3EK\bin" directory.
* Run this .exe, provide the file paths when prompted. The tool automatically creates a backup of the scenario tag before editing it.
