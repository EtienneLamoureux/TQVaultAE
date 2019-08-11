using System;
using System.Collections.Generic;
using System.Linq;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Domain.Search
{

	public interface IItemPredicate
	{
		bool Apply(ToFriendlyNameResult item);
	}

	public class ItemTruePredicate : IItemPredicate
	{
		public bool Apply(ToFriendlyNameResult item) => true;

		public override string ToString() => "true";
	}

	public class ItemFalsePredicate : IItemPredicate
	{
		public bool Apply(ToFriendlyNameResult item) => false;

		public override string ToString() => "false";
	}

	public class ItemAndPredicate : IItemPredicate
	{
		public readonly List<IItemPredicate> predicates;

		public ItemAndPredicate(params IItemPredicate[] predicates)
			=> this.predicates = predicates.ToList();

		public ItemAndPredicate(IEnumerable<IItemPredicate> predicates)
			=> this.predicates = predicates.ToList();

		public bool Apply(ToFriendlyNameResult item)
			=> predicates.TrueForAll(predicate => predicate.Apply(item));

		public override string ToString()
			=> string.Concat("(", String.Join(" && ", predicates.ConvertAll(p => p.ToString()).ToArray()), ")");
	}


	public class ItemOrPredicate : IItemPredicate
	{
		public readonly List<IItemPredicate> predicates;

		public ItemOrPredicate(params IItemPredicate[] predicates)
			=> this.predicates = predicates.ToList();

		public ItemOrPredicate(IEnumerable<IItemPredicate> predicates)
			=> this.predicates = predicates.ToList();

		public bool Apply(ToFriendlyNameResult item)
			=> predicates.Exists(predicate => predicate.Apply(item));

		public override string ToString()
			=> string.Concat('(', String.Join(" || ", predicates.ConvertAll(p => p.ToString()).ToArray()), ')');
	}

	public class ItemNamePredicate : IItemPredicate
	{
		public readonly string name;

		public ItemNamePredicate(string type)
			=> this.name = type;

		public bool Apply(ToFriendlyNameResult item)
			=> item.FullNameBagTooltipClean.ToUpperInvariant().Contains(name.ToUpperInvariant());

		public override string ToString()
			=> $"Name({name})";
	}

	public class ItemTypePredicate : IItemPredicate
	{
		public readonly string type;

		public ItemTypePredicate(string type)
			=> this.type = type;

		public bool Apply(ToFriendlyNameResult item)
			=> item.Item.ItemClass.ToUpperInvariant().Contains(type.ToUpperInvariant());

		public override string ToString()
			=> $"Type({type})";
	}

	public class ItemQualityPredicate : IItemPredicate
	{
		private readonly ITranslationService ItemStyleService;
		public readonly string quality;

		public ItemQualityPredicate(ITranslationService itemStyleService, string quality)
		{
			this.ItemStyleService = itemStyleService;
			this.quality = quality;
		}

		public bool Apply(ToFriendlyNameResult item)
			=> this.ItemStyleService.Translate(item.Item.ItemStyle).ToUpperInvariant().Contains(quality.ToUpperInvariant());

		public override string ToString()
			=> $"Quality({quality})";
	}

	public class ItemAttributePredicate : IItemPredicate
	{
		public readonly string attribute;

		public ItemAttributePredicate(string attribute)
			=> this.attribute = attribute;

		public bool Apply(ToFriendlyNameResult item)
			=> string.Join(" ", item.AttributesAll).ToUpperInvariant().Contains(attribute.ToUpperInvariant());

		public override string ToString()
			=> $"Attribute({attribute})";
	}
}
