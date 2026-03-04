# Application of Software Design Patterns in Video Game Development

## Game Description

This is a turn-based strategy game focused on exploring a haunted sunken ship.
The board on which the player moves is a hexagonal grid, and the entire gameplay revolves around it.

Each turn, the player has a certain number of **Action Points (AP)** available, which can be spent to perform various actions.

Movement and the basic attack are always available, while other actions must be unlocked during gameplay.

The player can also encounter special types of treasure that grant permanent bonuses. These bonuses provide passive effects and can modify how certain actions behave.

While exploring the ship, the player will occasionally be attacked by enemies, mostly sea creatures. Combat is turn-based and rotates between participants.

The player is limited by oxygen and consumes one unit of oxygen per turn. This encourages efficient use of turns.

When the player runs out of oxygen, they must surface to replenish it. However, upon returning to the ship, everything has changed — all creatures become more monstrous versions of themselves.

---

## Architecture

### Character

`Character` is an abstract class that defines all properties and methods that a character must contain.

**Key attributes:**

* `currentAP`, `perTurnAP`, `maxAP` – current action points, action points gained per turn, and maximum action points
* `maxHealth` and `currentHealth` – maximum and current health points

The `Character` class is inherited by `PlayerCharacter` and `NonPlayerCharacter`, which further define player-specific and enemy-specific attributes.

---

### Actions

Each action that a player or enemy can perform is implemented as its own class.

Every action inherits from the abstract base class `IAction`, which defines the structure of an action. All actions are implemented using the **Command** pattern.

Each action defines:

* Its execution conditions
* Logic for updating the context
* The exact execution behavior
* How the UI should be updated during preparation and execution

Actions also call various hooks for nearly every character activity (movement, dealing damage, moving enemies, etc.). These hooks are later used by other systems and follow the **Template Method** pattern.

*(Code intentionally left unchanged.)*

---

### List of Actions

| Action Name         | Type   | Cost | Targeting       | Range     | Description                                                             | Cooldown        | Minigame |
| ------------------- | ------ | ---- | --------------- | --------- | ----------------------------------------------------------------------- | --------------- | -------- |
| Basic Attack        | Attack | 2    | Single target   | Melee     | Deal 6 damage                                                           | None            | 1        |
| Pull Enemy          | Skill  | 2    | Skillshot       | 3 tiles   | Throw your anchor in any direction and pull anything it hits toward you | 2 turns         | 2        |
| Pull Self           | Skill  | 2    | Single target   | 3 tiles   | Throw your anchor at an unoccupied tile and pull yourself toward it     | 2 turns         | 2        |
| Whirlpool           | Attack | 2    | AOE             | Melee     | Deal 5 damage to all adjacent enemies                                   | 1 turn          | 3        |
| Torpedostorm        | Attack | 3    | AOE / Skillshot | 3 tiles   | Spin up to 3 tiles, dealing 5 damage to adjacent enemies along the way  | Once per combat | 3        |
| Maelstrom           | Attack | 3    | AOE             | 2 tiles   | Create a maelstrom that deals 3 damage and pulls enemies toward you     | 1 turn          | 3        |
| Launch Torpedo      | Attack | 3    | Single target   | 2–3 tiles | Deal 5 damage, push enemy back 1 tile, pull yourself to their position  | 2 turns         | 2        |
| Spare Chain         | Skill  | 1    | Single target   | Melee     | Tie up an enemy, preventing movement on their next turn                 | 2 turns         | 1        |
| Force Slam          | Attack | 2    | Single target   | Melee     | Deal 6 damage and push the enemy 1 tile in any direction                | 2 turns         | 2        |
| SINK                | Attack | 3    | Single target   | Melee     | Combo attack dealing 5 damage three times                               | Once per combat | 4        |
| Charge              | Attack | 3    | Skillshot       | 4 tiles   | Launch yourself; on hit deal 6 damage and push the enemy back 1 tile    | 3 turns         | 4        |
| Turn Off the Engine | Attack | 0    | Single target   | Melee     | Disable enemy until you act; your next action costs +2 AP               | 2 turns         | 1        |

---

### Player

`IPlayerState` is an abstract class that describes the player’s current state and represents the **State** pattern.

The player can be in one of three states:

* **DefaultTurnState** – the default state when it is the player’s turn
* **TargetingState** – the state entered when the player selects an action
* **WaitingForTurnState** – the state while waiting for the next turn

---

### Targeting State

`TargetingState` is the most important of the three. It represents a combination of the **State** and **Strategy** patterns.

From preparing one action, the player can transition directly into preparing another action without writing separate states for each action.

Each action defines its own behavior and UI updates, while `TargetingState` simply delegates responsibility to the selected action.

* `DefaultTurnState` is effectively a `TargetingState` where the action is locked to movement, unless hovering over an enemy (in which case it is locked to Basic Attack).
* `WaitingForTurnState` implements all abstract methods but contains no functional logic, since the player cannot act during this state.

*(Code intentionally left unchanged.)*

---

### State Transitions

#### Default State

When the player's turn begins, they are in the default state. From here, they can:

* Perform basic movement or a basic attack (after which they return to the default state with reduced AP).
* Press a keybind or UI button for a specific action, entering its `TargetingState`.
* End their turn and transition into `WaitingForTurnState`.

---

#### Targeting State

When the player selects an action, they enter `TargetingState`.

The UI visualizes:

* The action’s range
* The current target
* The AP cost

In this state, the player can:

* Execute the prepared action (returning to default state)
* Cancel the action (returning to default state)
* Select another action (switching to a new `TargetingState`)
* End the turn (entering `WaitingForTurnState`)

---

#### Waiting State

While waiting for their turn, the player cannot perform any actions. They can only observe the UI and move the camera.

---

### PlayerManager

`PlayerManager` is a singleton class responsible for:

* Managing player states
* Handling user input
* Coordinating transitions between states

It inherits from Unity’s `MonoBehaviour`, allowing it to be attached to a GameObject.

It:

* Updates the current state every frame
* Handles click execution
* Resets states after successful actions
* Manages ability selection
* Handles turn start and end logic

*(Code intentionally left unchanged.)*

---

## Perks

Perks are permanent bonuses that enhance the player and modify how turns are played.

Each perk:

* Inherits from the abstract `Perk` class
* Activates itself (usually by subscribing to events)
* Deactivates itself (by unsubscribing)

Perks primarily rely on hooks triggered inside actions.

Example: **Kill Rush** – whenever you kill an enemy, restore 2 AP.

---

### List of Perks

| Perk Name     | Description                                                                                                             |
| ------------- | ----------------------------------------------------------------------------------------------------------------------- |
| Diver’s Curse | Whenever you move an enemy, mark them. When you deal damage to a marked enemy, deal 4 bonus damage and remove the mark. |
| Pullmaxxing   | Using **Pull Enemy** makes your next **Pull Self** cost 0 AP, and vice versa.                                           |
| One Two Punch | Moving an enemy into melee range makes your next **Basic Attack** cost 0 AP.                                            |
| Supercharge   | After you move, your next action deals +3 damage.                                                                       |
| Longer Chain  | Increase melee range by 1 tile and make skillshots infinite range.                                                      |
| Skilled       | Perfect minigame success reduces cooldown by 1 and doubles bonus damage.                                                |
| Kill Rush     | Restore 2 AP after killing an enemy.                                                                                    |
| Gain Momentum | Gain 1 stack per tile moved. On attack, consume stacks to deal bonus damage.                                            |
| MORE CHAINS   | After damaging an enemy, root them for 1 turn.                                                                          |

All perks interact with each other, enabling various interesting combinations.

---

## Status Effects

Status effects are similar to perks but can affect any character.

Key differences:

* They are temporary.
* They expire after several turns or when certain conditions are met.
* They can be buffs or debuffs.

Each status effect inherits from the `StatusEffect` base class.

Example: **Stun** – prevents the character from acting on their next turn.

---

### Basic Status Effects

| Status Effect | Description                     |
| ------------- | ------------------------------- |
| Stun          | Cannot act on the next turn.    |
| Root          | Cannot move on the next turn.   |
| Disarm        | Cannot attack on the next turn. |

---

## Enemies (Planned)

There will be multiple enemy types, each with its own behavior logic.

This will follow the **Strategy** pattern:
Each enemy has its own `enemyLogic`, and an `EnemyManager` simply calls `enemyLogic.Act()`. The rest of the system remains unchanged.

Additionally, each time the player surfaces and returns, enemies become more monstrous versions of themselves — up to three evolution stages per enemy.

To handle this, an **Abstract Factory** pattern will be used.
An `EnemyAbstractFactory` will instantiate enemies based on:

* The enemy type
* The current evolution stage (based on how many times the player has surfaced)

For example, a jellyfish enemy would have three progressively more monstrous versions.

---

## Additional Planned Features

* UI elements for displaying active perks and status effects (functionality exists, UI is missing)
* XP system for unlocking new actions and perks
* Room-based exploration system

---

All core systems described above are fully implemented unless otherwise stated.
