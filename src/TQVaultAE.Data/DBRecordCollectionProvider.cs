using System.IO;
using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Data
{
	public class DBRecordCollectionProvider : IDBRecordCollectionProvider
	{
		public DBRecordCollectionProvider()
		{ }

		/// <summary>
		/// Writes all variables into a file.
		/// </summary>
		/// <param name="drc">source</param>
		/// <param name="baseFolder">Path in the file.</param>
		/// <param name="fileName">file name to be written</param>
		public void Write(DBRecordCollection drc, string baseFolder, string fileName = null)
		{
			// construct the full path
			string fullPath = Path.Combine(baseFolder, drc.Id);
			string destinationFolder = Path.GetDirectoryName(fullPath);

			if (fileName != null)
			{
				fullPath = Path.Combine(baseFolder, fileName);
				destinationFolder = baseFolder;
			}

			// Create the folder path if necessary
			if (!Directory.Exists(destinationFolder))
				Directory.CreateDirectory(destinationFolder);

			// Open the file
			using (StreamWriter outStream = new StreamWriter(fullPath, false))
			{
				// Write all the variables
				foreach (Variable variable in drc)
				{
					outStream.WriteLine(variable.ToString());
				}
			}
		}
	}
}
