using System;
using System.Collections.Generic;
using System.Linq;
using TQVaultAE.Entities;
using TQVaultAE.Data;
using TQVaultAE.Presentation;

namespace TQVaultAE.Services.Models.Search
{

	internal interface IItemPredicate
	{
		bool Apply(Item item);
	}

	internal class ItemTruePredicate : IItemPredicate
	{
		public bool Apply(Item item)
		{
			return true;
		}

		public override string ToString()
		{
			return "true";
		}
	}

	internal class ItemFalsePredicate : IItemPredicate
	{
		public bool Apply(Item item)
		{
			return false;
		}

		public override string ToString()
		{
			return "false";
		}
	}

	internal class ItemAndPredicate : IItemPredicate
	{
		internal readonly List<IItemPredicate> predicates;

		public ItemAndPredicate(params IItemPredicate[] predicates)
		{
			this.predicates = predicates.ToList();
		}

		public ItemAndPredicate(IEnumerable<IItemPredicate> predicates)
		{
			this.predicates = predicates.ToList();
		}

		public bool Apply(Item item)
		{
			return predicates.TrueForAll(predicate => predicate.Apply(item));
		}

		public override string ToString()
		{
			return "(" + String.Join(" && ", predicates.ConvertAll(p => p.ToString()).ToArray()) + ")";
		}
	}


	internal class ItemOrPredicate : IItemPredicate
	{
		internal readonly List<IItemPredicate> predicates;

		public ItemOrPredicate(params IItemPredicate[] predicates)
		{
			this.predicates = predicates.ToList();
		}

		public ItemOrPredicate(IEnumerable<IItemPredicate> predicates)
		{
			this.predicates = predicates.ToList();
		}

		public bool Apply(Item item)
		{
			return predicates.Exists(predicate => predicate.Apply(item));
		}

		public override string ToString()
		{
			return "(" + String.Join(" || ", predicates.ConvertAll(p => p.ToString()).ToArray()) + ")";
		}
	}

	internal class ItemNamePredicate : IItemPredicate
	{
		internal readonly string name;

		public ItemNamePredicate(string type)
		{
			this.name = type;
		}

		public bool Apply(Item item)
		{
			return ItemProvider.ToFriendlyName(item).ToUpperInvariant().Contains(name.ToUpperInvariant());
		}

		public override string ToString()
		{
			return $"Name({name})";
		}
	}

	internal class ItemTypePredicate : IItemPredicate
	{
		internal readonly string type;

		public ItemTypePredicate(string type)
		{
			this.type = type;
		}

		public bool Apply(Item item)
		{
			return item.ItemClass.ToUpperInvariant().Contains(type.ToUpperInvariant());
		}

		public override string ToString()
		{
			return $"Type({type})";
		}
	}

	internal class ItemQualityPredicate : IItemPredicate
	{
		internal readonly string quality;

		public ItemQualityPredicate(string quality)
		{
			this.quality = quality;
		}

		public bool Apply(Item item)
		{
			return ItemStyleHelper.Translate(item.ItemStyle).ToUpperInvariant().Contains(quality.ToUpperInvariant());
		}

		public override string ToString()
		{
			return $"Quality({quality})";
		}
	}

	internal class ItemAttributePredicate : IItemPredicate
	{
		internal readonly string attribute;

		public ItemAttributePredicate(string attribute)
		{
			this.attribute = attribute;
		}

		public bool Apply(Item item)
		{
			var att = string.Join(" ", ItemProvider.GetAttributes(item, true).ToArray());
			return att.ToUpperInvariant().Contains(attribute.ToUpperInvariant());
		}

		public override string ToString()
		{
			return $"Attribute({attribute})";
		}
	}
}
