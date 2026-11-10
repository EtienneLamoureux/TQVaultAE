# Character editor

Edit a character's attributes, masteries and skills in a window drawn like the game's, and read the statistics of the
game's character window without starting the game.

> **Note:** this is a "character editing" feature. Enable it with **Allow Character Editing Features** in the settings
> window. Do not edit a character that is loaded in the game.

## Open it

1. Select a character.
2. Click **Edit** on the stash panel.

Nothing is written before **OK**. **Cancel** drops every change.

## Character page

![Character page](charedit/character_page.png)

The left panel is the game's character window, with its three tabs **I**, **II** and **III**. The right panel holds
the editing options.

### Attributes

While attribute points are left, a `+` button shows next to each attribute.

| Click on `+` | Effect |
|---|---|
| Left | Spends one point |
| Right | Takes one point back |
| Shift + left | Spends all the points |
| Shift + right | Back to the starting value |

**Undo** puts the attributes back as they were when the editor opened.
**Reset Attributes** gives back every point spent since level 1.

### Leveling

Tick **Enable Leveling** to change the level, the XP, the unused skill points and the unlocked difficulty.

### Masteries

The list shows each mastery with its number of skills and points. **Reset Masteries** removes both masteries and
refunds their points.

## Statistics

The values are computed with the game's formulas from the attributes, the equipped items, the item sets and the
skills.

| Tab | Shows |
|---|---|
| I | Attributes, combined armor, main resistances, average damage, attack speed, DPS |
| II | Secondary resistances, regeneration, energy cost and recharge reduction, casting and movement speed |
| III | Damage and duration bonuses, monster race bonuses, play statistics |

![Tab II](charedit/stats_tab2.png) ![Tab III](charedit/stats_tab3.png)

Hover an icon or a value for its details, as in the game:

![Statistics tool tip](charedit/stats_tooltip.png)

Two options change what is counted:

| Option | Effect |
|---|---|
| **Difficulty** | The difficulty played: Epic and Legendary lower the resistances |
| **Include buffs** | Counts the auras and buffs of the skills as if they were active; else only the passive skills count |

## Mastery pages

One page per mastery, laid out like the game's skill window.

![Mastery page](charedit/mastery_page.png)

Choose the mastery and its level with the two boxes at the top right, or click the `+` button under the mastery bar.

| Click on a skill | Effect |
|---|---|
| Left | +1 point |
| Right | -1 point |
| Shift + left | Highest level the points allow |
| Shift + right | Lowest level |

The game's rules apply: a skill needs its mastery tier and the skill before it.

The number under a skill reads `level / maximum`. The level includes the bonuses of the equipment, so it can exceed
the maximum.

The border of a skill tells its state:

| Border | Meaning |
|---|---|
| Green | A point can be added |
| Gold | No point left |
| Blue | At its maximum |
| Gray | Mastery level too low |
| Red | Needs the skill before it |

Hover a skill for the game's tool tip, with the bonus coming from the equipment:

![Skill tool tip](charedit/skill_tooltip.png)

The bar at the bottom shows the unused skill points.
