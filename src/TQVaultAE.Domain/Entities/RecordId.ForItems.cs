using System;

namespace TQVaultAE.Domain.Entities;

public partial class RecordId
{

	#region IsRelic

	bool? _IsRelic;
	/// <summary>
	/// This <see cref="RecordId"/> leads to a Relic content.
	/// </summary>
	public bool IsRelic
	{
		get
		{
			if (_IsRelic is null)
				_IsRelic = (this.Dlc == GameDlc.TitanQuest && this.Normalized.Contains(@"RELICS") && !IsCharm) // Is base game
					|| this.Normalized.Contains(@"\RELICS\");// Is part of an extension
			return _IsRelic.Value;
		}
	}

	#endregion

	#region IsCharm

	bool? _IsCharm;
	/// <summary>
	/// This <see cref="RecordId"/> leads to a Charm content.
	/// </summary>
	public bool IsCharm
	{
		get
		{
			if (_IsCharm is null)
				_IsCharm = (this.Dlc == GameDlc.TitanQuest && this.Normalized.Contains(@"ANIMALRELICS")) // Is base game
					|| this.Normalized.Contains(@"\CHARMS\");// Is part of an extension
			return _IsCharm.Value;
		}
	}

	#endregion

	#region IsPotion

	bool? _IsPotion;
	/// <summary>
	/// This <see cref="RecordId"/> leads to a Potion content.
	/// </summary>
	public bool IsPotion
	{
		get
		{
			if (_IsPotion is null)
				_IsPotion = this.Normalized.Contains(@"ONESHOT\POTION");
			return _IsPotion.Value;
		}
	}

	#endregion

	#region IsQuestItem

	bool? _IsQuestItem;
	/// <summary>
	/// This <see cref="RecordId"/> leads to a Quest Item content.
	/// </summary>
	public bool IsQuestItem
	{
		get
		{
			if (_IsQuestItem is null)
				_IsQuestItem = (this.Dlc == GameDlc.TitanQuest && this.Normalized.Contains(@"QUEST")) // Is base game
					|| this.Normalized.Contains(@"QUESTS");// Is part of an extension
			return _IsQuestItem.Value;
		}
	}

	#endregion

	#region IsArtifact

	bool? _IsArtifact;
	/// <summary>
	/// This <see cref="RecordId"/> leads to an Artifact content.
	/// </summary>
	public bool IsArtifact
	{
		get
		{
			if (_IsArtifact is null)
				_IsArtifact = !this.IsFormulae && this.Normalized.Contains(@"\ARTIFACTS\");
			return _IsArtifact.Value;
		}
	}

	#endregion

	#region IsFormulae

	bool? _IsFormulae;
	/// <summary>
	/// This <see cref="RecordId"/> leads to an Arcane Formulae content.
	/// </summary>
	public bool IsFormulae
	{
		get
		{
			if (_IsFormulae is null)
				_IsFormulae = this.Normalized.Contains(@"\ARCANEFORMULAE\");
			return _IsFormulae.Value;
		}
	}

	#endregion

	#region IsParchment

	bool? _IsParchment;
	/// <summary>
	/// This <see cref="RecordId"/> leads to Parchment content.
	/// </summary>
	public bool IsParchment
	{
		get
		{
			if (_IsParchment is null)
				_IsParchment = this.Normalized.Contains(@"PARCHMENT");
			return _IsParchment.Value;
		}
	}

	#endregion

	#region IsScroll

	bool? _IsScroll;
	/// <summary>
	/// This <see cref="RecordId"/> leads to Scroll content.
	/// </summary>
	public bool IsScroll
	{
		get
		{
			if (_IsScroll is null)
				_IsScroll = this.Normalized.Contains(@"\SCROLLS\");
			return _IsScroll.Value;
		}
	}

	#endregion

	#region IsItem

	bool? _IsItem;
	/// <summary>
	/// This <see cref="RecordId"/> leads to Items content.
	/// </summary>
	public bool IsItem
	{
		get
		{
			if (_IsItem is null)
				_IsItem = this.Normalized.Contains(@"\ITEM\") || this.Normalized.Contains(@"\ITEMS\");
			return _IsItem.Value;
		}
	}

	#endregion

	#region IsEquipmentWeapon

	bool? _IsEquipmentWeapon;
	/// <summary>
	/// This <see cref="RecordId"/> leads to Equipment Weapon content.
	/// </summary>
	public bool IsEquipmentWeapon
	{
		get
		{
			if (_IsEquipmentWeapon is null)
				_IsEquipmentWeapon = this.Normalized.Contains(@"\EQUIPMENTWEAPON");// Exist with an S
			return _IsEquipmentWeapon.Value;
		}
	}

	#endregion

	#region IsEquipmentWeaponAxe

	bool? _IsEquipmentWeaponAxe;
	/// <summary>
	/// This <see cref="RecordId"/> leads to Equipment Axe content.
	/// </summary>
	public bool IsEquipmentWeaponAxe
	{
		get
		{
			if (_IsEquipmentWeaponAxe is null)
				_IsEquipmentWeaponAxe = IsEquipmentWeapon && this.Normalized.Contains(@"\AXE\");
			return _IsEquipmentWeaponAxe.Value;
		}
	}

	#endregion

	#region IsEquipmentWeaponBow

	bool? _IsEquipmentWeaponBow;
	/// <summary>
	/// This <see cref="RecordId"/> leads to Equipment Bow content.
	/// </summary>
	public bool IsEquipmentWeaponBow
	{
		get
		{
			if (_IsEquipmentWeaponBow is null)
				_IsEquipmentWeaponBow = IsEquipmentWeapon && this.Normalized.Contains(@"\BOW\");
			return _IsEquipmentWeaponBow.Value;
		}
	}

	#endregion

	#region IsEquipmentWeaponMace

	bool? _IsEquipmentWeaponMace;
	/// <summary>
	/// This <see cref="RecordId"/> leads to Equipment Bow content.
	/// </summary>
	public bool IsEquipmentWeaponMace
	{
		get
		{
			if (_IsEquipmentWeaponMace is null)
				_IsEquipmentWeaponMace = IsEquipmentWeapon && this.Normalized.Contains(@"\CLUB\");
			return _IsEquipmentWeaponMace.Value;
		}
	}

	#endregion

	#region IsEquipmentWeaponSpear

	bool? _IsEquipmentWeaponSpear;
	/// <summary>
	/// This <see cref="RecordId"/> leads to Equipment Spear content.
	/// </summary>
	public bool IsEquipmentWeaponSpear
	{
		get
		{
			if (_IsEquipmentWeaponSpear is null)
				_IsEquipmentWeaponSpear = IsEquipmentWeapon && this.Normalized.Contains(@"\SPEAR\");
			return _IsEquipmentWeaponSpear.Value;
		}
	}

	#endregion

	#region IsEquipmentWeaponStaff

	bool? _IsEquipmentWeaponStaff;
	/// <summary>
	/// This <see cref="RecordId"/> leads to Equipment Staff content.
	/// </summary>
	public bool IsEquipmentWeaponStaff
	{
		get
		{
			if (_IsEquipmentWeaponStaff is null)
				_IsEquipmentWeaponStaff = IsEquipmentWeapon && this.Normalized.Contains(@"\STAFF\");
			return _IsEquipmentWeaponStaff.Value;
		}
	}

	#endregion

	#region IsEquipmentWeaponThrown

	bool? _IsEquipmentWeaponThrown;
	/// <summary>
	/// This <see cref="RecordId"/> leads to Equipment Thrown content.
	/// </summary>
	public bool IsEquipmentWeaponThrown
	{
		get
		{
			if (_IsEquipmentWeaponThrown is null)
				_IsEquipmentWeaponThrown = IsEquipmentWeapon && this.Normalized.Contains(@"\1HRANGED\");
			return _IsEquipmentWeaponThrown.Value;
		}
	}

	#endregion

	#region IsEquipmentWeaponSword

	bool? _IsEquipmentWeaponSword;
	/// <summary>
	/// This <see cref="RecordId"/> leads to Equipment Sword content.
	/// </summary>
	public bool IsEquipmentWeaponSword
	{
		get
		{
			if (_IsEquipmentWeaponSword is null)
				_IsEquipmentWeaponSword = IsEquipmentWeapon && this.Normalized.Contains(@"\SWORD\");
			return _IsEquipmentWeaponSword.Value;
		}
	}

	#endregion

	#region IsEquipmentShield

	bool? _IsEquipmentShield;
	/// <summary>
	/// This <see cref="RecordId"/> leads to Equipment Shields content.
	/// </summary>
	public bool IsEquipmentShield
	{
		get
		{
			if (_IsEquipmentShield is null)
				_IsEquipmentShield = this.Normalized.Contains(@"\EQUIPMENTSHIELD") || (IsEquipmentWeapon && this.Normalized.Contains(@"\SHIELD\"));
			return _IsEquipmentShield.Value;
		}
	}

	#endregion

	#region IsEquipmentRing

	bool? _IsEquipmentRing;
	/// <summary>
	/// This <see cref="RecordId"/> leads to Equipment Rings content.
	/// </summary>
	public bool IsEquipmentRing
	{
		get
		{
			if (_IsEquipmentRing is null)
				_IsEquipmentRing = this.Normalized.Contains(@"\EQUIPMENTRING") || this.Normalized.Contains(@"\EQUIPMENTARMOR\RING\");
			return _IsEquipmentRing.Value;
		}
	}

	#endregion

	#region IsEquipmentHelm

	bool? _IsEquipmentHelm;
	/// <summary>
	/// This <see cref="RecordId"/> leads to Equipment Helm content.
	/// </summary>
	public bool IsEquipmentHelm
	{
		get
		{
			if (_IsEquipmentHelm is null)
				_IsEquipmentHelm = this.Normalized.Contains(@"\EQUIPMENTHELM") || this.Normalized.Contains(@"\EQUIPMENTARMOR\HELM\");
			return _IsEquipmentHelm.Value;
		}
	}

	#endregion

	#region IsEquipmentGreaves

	bool? _IsEquipmentGreaves;
	/// <summary>
	/// This <see cref="RecordId"/> leads to Equipment Greaves content.
	/// </summary>
	public bool IsEquipmentGreaves
	{
		get
		{
			if (_IsEquipmentGreaves is null)
				_IsEquipmentGreaves = this.Normalized.Contains(@"\EQUIPMENTGREAVES") || this.Normalized.Contains(@"\EQUIPMENTARMOR\GREAVES\");
			return _IsEquipmentGreaves.Value;
		}
	}

	#endregion

	#region IsEquipmentTorso

	bool? _IsEquipmentTorso;
	/// <summary>
	/// This <see cref="RecordId"/> leads to Equipment Torso content.
	/// </summary>
	public bool IsEquipmentTorso
	{
		get
		{
			if (_IsEquipmentTorso is null)
				_IsEquipmentTorso = (this.Dlc == GameDlc.TitanQuest && this.Normalized.Contains(@"\EQUIPMENTARMOR\"))
					|| (this.Dlc != GameDlc.TitanQuest && this.Normalized.Contains(@"\EQUIPMENTARMOR\TORSO\"));
			return _IsEquipmentTorso.Value;
		}
	}

	#endregion

	#region IsEquipmentAmulet

	bool? _IsEquipmentAmulet;
	/// <summary>
	/// This <see cref="RecordId"/> leads to Equipment Amulet content.
	/// </summary>
	public bool IsEquipmentAmulet
	{
		get
		{
			if (_IsEquipmentAmulet is null)
				_IsEquipmentAmulet = this.Normalized.Contains(@"\EQUIPMENTAMULET") || this.Normalized.Contains(@"\EQUIPMENTARMOR\AMULET\");

			return _IsEquipmentAmulet.Value;
		}
	}

	#endregion

	#region IsEquipmentArmband

	bool? _IsEquipmentArmband;
	/// <summary>
	/// This <see cref="RecordId"/> leads to Equipment Armband content.
	/// </summary>
	public bool IsEquipmentArmband
	{
		get
		{
			if (_IsEquipmentArmband is null)
				_IsEquipmentArmband = this.Normalized.Contains(@"\EQUIPMENTARMBAND") || this.Normalized.Contains(@"\EQUIPMENTARMOR\ARMBAND\");
			return _IsEquipmentArmband.Value;
		}
	}

	#endregion

}
