using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using TQVaultAE.Data;
using TQVaultAE.Entities;
using TQVaultAE.Entities.Results;
using TQVaultAE.Logs;
using TQVaultAE.Presentation;
using static TQVaultAE.Data.ItemProvider;

namespace TQVaultAE.Services
{
	public class ItemService
	{
		private readonly log4net.ILog Log = null;
		private readonly SessionContext userContext = null;

		public ItemService(SessionContext userContext)
		{
			Log = Logger.Get(this);
			this.userContext = userContext;
		}

		public ToFriendlyNameResult GetFriendlyNames(Item itm, FriendlyNamesExtraScopes? scopes = null)
		{
			var result = ItemProvider.GetFriendlyNames(itm, scopes);
			return result;
		}


	}
}
