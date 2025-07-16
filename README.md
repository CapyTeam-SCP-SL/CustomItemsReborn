
# CustomItemsReborn

## Description
CustomItemsReborn is a plugin for SCP: Secret Laboratory that introduces a variety of unique custom items to enhance gameplay. The plugin consists of two primary components, all contained within a single DLL with no external dependencies:
1. **Custom Items**: A collection of 13 custom items (detailed below) that server hosts can integrate into their servers. These items can be assigned to subclasses using the [Advanced Subclassing](https://github.com/steven4547466/AdvancedSubclassing) plugin, spawned at predefined map locations, or distributed via commands.
2. **API for Developers**: A robust API for developers to create their own custom items. The API handles item tracking, supports non-standard clip sizes for weapons, and provides overrideable methods for custom event handling, requiring minimal effort from developers.

### Item List
| Item Name | Item ID | Description |
|:----------|:--------|:------------|
| AM-119 | AntiMemeticPills | Pills that protect against Amnesia effects, particularly SCP-096's face recognition, for a configurable duration. |
| DeflectorShield | DeflectorShield | A shield that deflects incoming bullets for a set duration, reflecting damage back to attackers with a configurable multiplier. |
| EM-119 | EmpGrenade | An EMP grenade that opens doors and disables electronics (e.g., Tesla gates, lights) within its blast radius for a configurable duration. |
| GL-119 | GrenadeLauncher | A weapon that launches explosive grenades that detonate on impact, optionally consuming grenades from the player's inventory. |
| IG-119 | ImplosionGrenade | A grenade that pulls players toward its explosion center with minimal damage, configurable for suction strength and duration. |
| LJ-119 | LethalInjection | A syringe that instantly kills a target or the user if misused, with configurable delay and penalties. |
| LC-119 | LuckyCoin | A coin that, when flipped, grants random luck-based effects (e.g., movement boost or damage resistance) with configurable duration and cooldown. |
| MG-119 | MediGun | A gun that heals friendly players or cures SCP-049-2 instances with repeated shots, with configurable healing parameters. |
| SR-119 | SniperRifle | A modified E-11 rifle with a scope and extended barrel, firing high-damage sniper rounds with a configurable damage multiplier and clip size. |
| TG-119 | TranquilizerGun | A gun that fires non-lethal tranquilizing darts, rendering targets unconscious with configurable duration and SCP resistance. |
| SCP-2818 | Scp2818 | A firearm that propels the user as a projectile in the aimed direction, with configurable movement parameters and optional despawn. |
| SCP-714 | Scp714 | A jade ring that protects against specific SCPs and effects, reduces stamina, and reflects damage to SCP-049, with configurable properties. |
| SCP-1499 | Scp1499 | A gas mask that teleports the user to another dimension for a configurable duration, returning them under specific conditions. |

### Item Configurations
All item properties, including text messages (e.g., `PickupBroadcast`, `ChangeHint`, and item-specific messages like `TakeOffMessage` for SCP-714), are configurable via a `global.yml` file located at `~/.config/EXILED/Configs/CustomItems` on Linux or `%AppData%\EXILED\Configs\CustomItems` on Windows. The configuration file path and name can be customized in the plugin's main configuration file (`Config.cs`) for each server, allowing different settings across multiple servers.

Each item's configuration includes detailed, self-explanatory fields for properties such as durations, damage modifiers, and text messages. For example:
- **PickupBroadcast**: The message displayed when an item is picked up.
- **ChangeHint**: The hint shown when the item is selected in the inventory.

The `global.yml` file is automatically generated with default values if it does not exist, ensuring ease of setup.

### Commands
| Command | Arguments | Permissions | Description |
|:--------|:----------|:------------|:------------|
| `rci list` | None | `citems.list` | Lists the names and IDs of all installed and enabled custom items on the server. |
| `rci info` | `<item name/id>` | `citems.info` | Displays detailed information about a specific item, including name, ID, and base type. |
| `rci spawn` | `<item name/id> <location/player/coordinates>` | `citems.spawn` | Spawns the specified item at a predefined location, a player's position, or specific coordinates. |
| `rci give` | `<item name/id> [player]` | `citems.give` | Gives the specified item to the indicated player. If no player is specified, the item is given to the command issuer. In-game RA command only. |
| `rci role` | `<player> <role>` | `citems.role` | Assigns a specified role (e.g., Scp173, Scientist) to the indicated player. Has a 3-second cooldown. |

### Valid Spawn Location Names
The following locations are valid for item spawn configurations and the `rci spawn` command. Names must be entered exactly as listed to avoid configuration errors:
```
Inside012
Inside012Bottom
Inside012Locker
Inside049Armory
Inside079Secondary
Inside096
Inside173Armory
Inside173Bottom
Inside173Connector
InsideEscapePrimary
InsideEscapeSecondary
InsideIntercom
InsideLczArmory
InsideLczCafe
InsideNukeArmory
InsideSurfaceNuke
Inside079First
Inside173Gate
Inside914
InsideGateA
InsideGateB
InsideGr18
InsideHczArmory
InsideHid
InsideHidLeft
InsideHidRight
InsideLczWc
InsideServersBottom
```

### Attachment Names
The following attachment names are supported for weapon customization (e.g., for `SniperRifle`):
```
None
IronSights
DotSight
HoloSight
NightVisionSight
AmmoSight
ScopeSight
StandardStock
ExtendedStock
RetractedStock
LightweightStock
HeavyStock
RecoilReducingStock
Foregrip
Laser
Flashlight
AmmoCounter
StandardBarrel
ExtendedBarrel
SoundSuppressor
FlashHider
MuzzleBrake
MuzzleBooster
StandardMagFMJ
StandardMagAP
StandardMagJHP
ExtendedMagFMJ
ExtendedMagAP
ExtendedMagJHP
DrumMagFMJ
DrumMagAP
DrumMagJHP
LowcapMagFMJ
LowcapMagAP
LowcapMagJHP
CylinderMag4
CylinderMag6
CylinderMag8
CarbineBody
RifleBody
ShortBarrel
ShotgunChoke
ShotgunExtendedBarrel
NoRifleStock
```

### API Notes (For Developers)
The CustomItemsReborn API simplifies the creation of custom items by handling item tracking, non-standard clip sizes, and event management. To create a custom item:

1. **Create a Class**: Inherit from `CustomItem`, `CustomWeapon`, or `CustomGrenade` based on the item type.
2. **Override Methods**: Implement `SubscribeEvents` and `UnsubscribeEvents` to register/unregister event handlers. Use `Check` to verify if an item belongs to your class:
   ```csharp
   if (Check(ev.Player.CurrentItem))
   {
       // Handle your item's logic
   }
   ```
3. **Register the Item**: Instantiate and register your item using:
   ```csharp
   new SomeItem(ItemType.SomeType, 42).RegisterCustomItem();
   ```
   For weapons, include the clip size in the constructor.

**Example Custom Item**:
```csharp
public class SomeItem : CustomItem
{
    public SomeItem(ItemType type, int id) : base(type, id)
    {
    }

    public override string Id => "SomeItemID";
    public override string Name => "SomeItemName";
    public override ItemType BaseType => ItemType.SomeType;

    public override void Initialize()
    {
        var config = Plugin.Instance.Config.ItemConfigs.SomeItem;
        PickupBroadcast = config.PickupBroadcast;
        ChangeHint = config.ChangeHint;
        // Initialize other properties from config
        SpawnInRoom(RoomType.SomeRoom, Vector3.zero);
    }

    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.SomeEvent += OnSomeEvent;
        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.SomeEvent -= OnSomeEvent;
        base.UnsubscribeEvents();
    }

    private void OnSomeEvent(SomeEventEventArgs ev)
    {
        if (Check(ev.Player.CurrentItem))
        {
            // Implement item logic
        }
    }
}
```

**Registration**:
```csharp
public class Plugin : Plugin<Config>
{
    public override void OnEnabled()
    {
        Timing.CallDelayed(5f, () => new SomeItem(ItemType.SomeType, 42).RegisterCustomItem());
    }
}
```

**Important**: Register items with a 5-second delay in `OnEnabled` to ensure all server components and plugins are initialized.

**Notes**:
- Each item class acts as a manager for all instances of that item type, tracking `SyncItemInfo` and `Pickup` objects.
- Use `Check` to verify item ownership instead of casting (`SyncItemInfo` or `Pickup` cannot be cast to your custom item type).
- To check for items managed by other classes, iterate through `CustomItems.API.API.GetInstalledItems()` and use `Check`.

### Recent Updates
- **New Role Command**: Added the `rci role` command to assign roles to players, enhancing server administration capabilities.
- **Configurable Text**: All text-based properties (e.g., `PickupBroadcast`, `ChangeHint`, and item-specific messages like `TakeOffMessage` for SCP-714) are now configurable via the `global.yml` file.
- **Expanded Item List**: The plugin now supports 13 items, with `SCP-127`, `SG-119`, `Rock`, and `C4-119` removed or deprecated in favor of new items like `SCP-714`, `SCP-1499`, and `SCP-2818`.
- **Improved API**: Enhanced support for item initialization from configuration files, ensuring all properties are customizable without modifying code.
- **Bug Fixes**: Improved error handling and logging for items like `SCP-1499` and `SCP-2818` to prevent crashes and ensure reliable behavior.

### Credits
- **NeonWizard**: Original `TranquilizerGun` concept.
- **Killer0992**: Original `Shotgun` concept.
- **Dimenzio**: `SpawnGrenade` method.
- **Michal78900**: `SCP-1499` implementation.
- **SebasCapo**: API rework contributions.
- **Rozy**: API rework
