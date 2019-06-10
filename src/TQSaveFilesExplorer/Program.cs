using AutoMapper;
using TQ.SaveFilesExplorer.Entities;
using TQ.SaveFilesExplorer.Entities.Players;
using TQ.SaveFilesExplorer.Entities.TransferStash;
using System;
using System.Collections.Generic;
using System.Linq;
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
			Mapper.Initialize(cfg =>
			{
				cfg.CreateMap<TQFileRecord, TQFilePlayerRecord>().ReverseMap();
				cfg.CreateMap<TQFileRecord, TQFilePlayerTransferStashRecord>().ReverseMap();
			});

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
	}
}
