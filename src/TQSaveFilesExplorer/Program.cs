using AutoMapper;
using TQ.SaveFilesExplorer.Entities;
using TQ.SaveFilesExplorer.Entities.Players;
using TQ.SaveFilesExplorer.Entities.TransferStash;
using System;
using System.Windows.Forms;

namespace TQ.SaveFilesExplorer
{
	static class Program
	{
		/// <summary>
		/// Point d'entrée principal de l'application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			var config = new MapperConfiguration(cfg => {
				cfg.CreateMap<TQFileRecord, TQFilePlayerRecord>().ReverseMap();
				cfg.CreateMap<TQFileRecord, TQFilePlayerTransferStashRecord>().ReverseMap();
			});

			var mapper = config.CreateMapper();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm(mapper));
		}
	}
}
