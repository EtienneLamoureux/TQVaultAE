# Mods (custom maps)

TQVaultAE shows modded characters, items and vaults with their own mod's data, next to regular ones, in one session.
Technical details: [MODLINK.md](MODLINK.md).

* [Link a character](#LinkCharacter)
* [Incomplete items](#Incomplete)
* [Link an item](#ItemLink)
* [Link a vault](#VaultLink)
* [Rules](#Rules)

---

### <a id="LinkCharacter"></a>Link a character
Character menu > **Mod link...**. **Suggest** picks the installed mod that has the character's unknown records; **Link**
saves the choice.

![Mod link dialog](mods/mod-link-dialog.png)

The character list shows the mod and the link state: **Linked**, **Not linked** or **Not installed**.

![Character list](mods/character-list.png)

---

### <a id="Incomplete"></a>Incomplete items
An item whose records the loaded data lacks says so in its tooltip. Without its base record, it shows as a warning tile.

![Incomplete item tooltip](mods/incomplete-tooltip.png)
![Warning tile in a bag](mods/incomplete-bag.png)

---

### <a id="ItemLink"></a>Link an item
A linked item is read with its mod's data (then the base game's), whatever character is shown. Its tooltip names the mod.

![Tooltip of an item linked to a mod](mods/item-tooltip-mod.png)

Right-click > **Modded** to choose it. **(suggested)** marks the mods having the item's records.

![Modded menu of an item](mods/item-modded-menu.png)

---

### <a id="VaultLink"></a>Link a vault
Vault tools button > **Modded vault**. The vault list shows the mod, and the vault's items use its data.

![Vault tools menu](mods/vault-tools-menu.png)
![A vault linked to a mod, with a regular character shown](mods/modded-vault.png)

---

### <a id="Rules"></a>Rules
- An item has its own link, or follows its vault's link.
- An incomplete item gets a mod as its own link when that mod resolves it: on loading its linked character, on linking
  its vault, or when put in a linked vault (drop, **Move Item To**, import, paste). If it no longer fits once resolved,
  a drop leaves it incomplete; other ways move it to a free spot.
- The link follows the item (moves, copies, removed relics) and is kept by [exports](IMPORT_EXPORT.md).
- Character files cannot store an item's link: keep linked items in a vault.
