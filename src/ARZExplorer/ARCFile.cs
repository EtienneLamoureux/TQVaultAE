using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using zlib;

namespace ARZExplorer
{
    public class ARCFile
    {
		// holds data about a file stored in an ARC file.
		private class ARCPartEntry
		{
			public int fileOffset; // offset within the ARC file
			public int compressedSize; // compressed size
			public int realSize; // size when uncompressed
		};

		private class ARCDirEntry
		{
			public String filename;
			public int storageType; // Added by VillageIdiot sets whether the data is compressed (3) or stored (1)
			public int fileOffset; // offset within the ARC file
			public int compressedSize; // compressed size
			public int realSize; // size when uncompressed
			public ARCPartEntry[] parts; // part data

			public bool IsActive 
            {
                get
                { 
                    if(storageType == 1) return true;
					else return parts != null; 
                }
			}
		};

		private bool   m_fileHasBeenRead;
		private string m_filename;
		private Hashtable m_dir;
        private string[] m_keys;
        public static ARCFile arcFile = null;

		public ARCFile(string filename)
		{
			m_filename = filename;
			m_fileHasBeenRead = false;
			m_dir = null;
		}

		public string Filename
        {
            get
            {
                return m_filename; 
            }
		}

		// Reads data from an ARC file and puts it into a Byte array (or NULL if not found)
	    public byte[] GetData(string dataID) 
        {
		    if(!m_fileHasBeenRead) ReadARCToC();
		    if(m_dir == null) 
            {
			    return null; // could not read the file
		    }

    		// First normalize the filename
	    	dataID = NormalizeRecordPath(dataID);
    		// Find our file in the toc.
    		// First strip off the leading folder since it is just the ARC name
	    	int firstPathDelim = dataID.IndexOf('\\');
		    if(firstPathDelim != -1) 
            {
			    dataID = dataID.Substring(firstPathDelim+1);
    		}

    		// Now see if this file is in the toc.
	    	ARCDirEntry dirEntry = (ARCDirEntry) m_dir[dataID];

    		if(dirEntry == null) 
            {
		    	return null; // record not found
		    }

		    // Now open the ARC file and read in the record
		    FileStream arcFile = new FileStream(m_filename, FileMode.Open, FileAccess.Read);
		    try
            {
			    // Allocate memory for the uncompressed data
    			byte[] ans = new byte[dirEntry.realSize];

	    		// Now process each part of this record
		    	int ipos = 0;

			    if((dirEntry.storageType == 1) && (dirEntry.compressedSize == dirEntry.realSize)) 
                {
				    arcFile.Seek(dirEntry.fileOffset, SeekOrigin.Begin);
				    arcFile.Read(ans, 0, dirEntry.realSize);
			    }
    			else
                {
	    			for(int ipart = 0; ipart < dirEntry.parts.Length; ++ipart) 
                    {
		    			// seek to the part we want
			    		arcFile.Seek(dirEntry.parts[ipart].fileOffset, SeekOrigin.Begin);

    					// Create a decompression stream
	    				ZInputStream zinput = new ZInputStream(arcFile);

					    int len;
					    int partLen = 0;
    					while((len = zinput.read (ans, ipos, ans.Length - ipos)) > 0)
                        {
	    					ipos += len;
		    				partLen += len;
			    			// break out of the read loop if we have processed this part completely.
				    		if (partLen >= dirEntry.parts[ipart].realSize) break;
					    }
				    }
			    }
			    return ans;
		    }
		    finally 
            {
			    if(arcFile != null) arcFile.Close();
		    }
	    }

        public bool Read()
        {
		    try
		    {
    			if (!m_fileHasBeenRead) ReadARCToC();
                return (m_dir != null);
            }
            catch (Exception)
		    {
			    return false;
		    }
        }

        public void Write(string baseFolder, string record, string fileName)
        {
            try
            {
                if (!m_fileHasBeenRead) ReadARCToC();

                string dataID = string.Concat(Path.GetFileNameWithoutExtension(m_filename), "\\", record);
                byte[] data = GetData(dataID);
                if (data == null) return;

                string dest = baseFolder;
                if (!dest.EndsWith("\\")) dest = string.Concat(dest, "\\");
                dest = string.Concat(dest, fileName);

                // If there is a sub directory in the arc file then we need to create it.
                if (!Directory.Exists(Path.GetDirectoryName(dest))) Directory.CreateDirectory(Path.GetDirectoryName(dest));

                FileStream outStream = new FileStream(dest, FileMode.Create, FileAccess.Write);
                try
                {
                    outStream.Write(data, 0, data.Length);
                }
                finally
                {
                    outStream.Close();
                }
            }
            catch (Exception)
            {
                return;
            }
        }

	    public bool ExtractARCFile(string dest)
	    {
		    try
		    {
    			if (!m_fileHasBeenRead) ReadARCToC();

                foreach ( DictionaryEntry de in m_dir )
			    {
				    ARCFile.ARCDirEntry dirEntry = (ARCDirEntry) de.Value;

				    string dataID = string.Concat(Path.GetFileNameWithoutExtension(m_filename), "\\", dirEntry.filename);
    				byte[] data = GetData(dataID);

	    			string filename = dest;
		    		if(!filename.EndsWith("\\")) filename = string.Concat(filename, "\\");
			    	filename = string.Concat(filename, dirEntry.filename);

                    // If there is a sub directory in the arc file then we need to create it.
                    if (!Directory.Exists(Path.GetDirectoryName(filename))) Directory.CreateDirectory(Path.GetDirectoryName(filename));

    				FileStream outStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
	    			try 
                    {
		    			outStream.Write(data, 0, data.Length);
			    	}
				    finally 
                    {
                        outStream.Close();
				    }
			    }

			    return true;
		    }
            catch (Exception)
		    {
			    // silently eat these errors
			    return false;
		    }
	    }

	
		// Read the table of contents of the ARC file
	    private void ReadARCToC()
	    {
		    // Format of an ARC file
		    // 0x08 - 4 bytes = # of files
    		// 0x0C - 4 bytes = # of parts
	    	// 0x18 - 4 bytes = offset to directory structure
		    //
    		// Format of directory structure
	    	// 4-byte int = offset in file where this part begins
		    // 4-byte int = size of compressed part
    		// 4-byte int = size of uncompressed part
	    	// these triplets repeat for each part in the arc file
		    // After these triplets are a bunch of null-terminated strings
    		// which are the sub filenames.
	    	// After the subfilenames comes the subfile data:
		    // 4-byte int = 3 == indicates start of subfile item  (maybe compressed flag??)
    		//				1 == maybe uncompressed flag??
	    	// 4-byte int = offset in file where first part of this subfile begins
		    // 4-byte int = compressed size of this file
    		// 4-byte int = uncompressed size of this file
	    	// 4-byte crap
		    // 4-byte crap
    		// 4-byte crap
	    	// 4-byte int = numParts this file uses
		    // 4-byte int = part# of first part for this file (starting at 0).
    		// 4-byte int = length of filename string
	    	// 4-byte int = offset in directory structure for filename
                
    		m_fileHasBeenRead = true;

    		try {
	    		FileStream arcFile = new FileStream(m_filename, FileMode.Open, FileAccess.Read);
		    	BinaryReader reader = new BinaryReader(arcFile);
			    try 
                {            
				    // check the file header
    				if(reader.ReadByte() != 0x41) return ;  // A
	    			if(reader.ReadByte() != 0x52) return ;  // R
		    		if(reader.ReadByte() != 0x43) return ;  // C

				    if(arcFile.Length < 0x21) return ;

    				reader.BaseStream.Seek(0x08, SeekOrigin.Begin);
	    			int numEntries = reader.ReadInt32();
		    		int numParts = reader.ReadInt32();

	    			ARCPartEntry[] parts = new ARCPartEntry[numParts];
		    		ARCDirEntry[] records = new ARCDirEntry[numEntries];

    				reader.BaseStream.Seek(0x18, SeekOrigin.Begin);
	    			int tocOffset = reader.ReadInt32();

			    	if(arcFile.Length < (tocOffset + 4 * 3)) return ;

				    // Read in all of the part data
    				reader.BaseStream.Seek(tocOffset, SeekOrigin.Begin);
	    			int i;
		    		for(i = 0; i < numParts; ++i) 
                    {
                        parts[i] = new ARCPartEntry();
					    parts[i].fileOffset = reader.ReadInt32();
    					parts[i].compressedSize = reader.ReadInt32();
	    				parts[i].realSize = reader.ReadInt32();
				    }

	    			// Now record this offset so we can come back and read in the filenames after we have
    				// read in the file records
		    		int fileNamesOffset = (int) arcFile.Position;
			    	// Now seek to the location where the file record data is
				    // This offset is from the end of the file.
    				int fileRecordOffset = 44 * numEntries;

			    	arcFile.Seek(-1 * fileRecordOffset, SeekOrigin.End);
				    for (i = 0; i < numEntries; ++i) 
                    {
					    records[i] = new ARCDirEntry();
					    int storageType = reader.ReadInt32(); // storageType = 3 - compressed / 1- non compressed

					    // Added by VillageIdiot to support stored types
    					records[i].storageType = storageType;
	    				records[i].fileOffset = reader.ReadInt32();
		    			records[i].compressedSize = reader.ReadInt32();
			    		records[i].realSize = reader.ReadInt32();
				    	int crap = reader.ReadInt32(); // crap

    					crap = reader.ReadInt32(); // crap
		    			crap = reader.ReadInt32(); // crap

				    	int np = reader.ReadInt32();
					    if (np < 1) 
                        {
						    records[i].parts = null;
	    				}
		    			else {
			    			records[i].parts = new ARCPartEntry[np];
				    	}

					    int firstPart = reader.ReadInt32();
					    crap = reader.ReadInt32(); // filename length

	    				crap = reader.ReadInt32(); // filename offset

					    if(storageType != 1 && records[i].IsActive) 
                        {
						    for(int ip = 0; ip < records[i].parts.Length; ++ip) 
                            {
							    records[i].parts[ip] = parts[ip + firstPart];
						    }
					    }
				    }

				    // Now read in the record names
				    arcFile.Seek(fileNamesOffset, SeekOrigin.Begin);
				    byte[] buffer = new byte[2048];
				    ASCIIEncoding ascii = new ASCIIEncoding();
				    for(i = 0; i < numEntries; ++i)
                    {
    					// only Active files have a filename entry
	    				if(records[i].IsActive) 
                        {
		    				// For each string, read bytes until I hit a 0x00 byte.
				    		int bufSize = 0;

					    	while((buffer[bufSize++] = reader.ReadByte ()) != 0x00) 
                            {
							    if (buffer[bufSize-1] == 0x03)
                                { // god damn it
								    arcFile.Seek (-1, SeekOrigin.Current); // backup
    								bufSize--;
	    							buffer[bufSize] = 0x00;
			    					break;
				    			}
						    }
						
						    string newfile;
						    if(bufSize >= 1) 
                            {
							    // Now convert the buffer to a String
							    char[] chars = new char[ascii.GetCharCount(buffer, 0, bufSize-1)];
							    ascii.GetChars(buffer, 0, bufSize-1, chars, 0);
							    newfile = new string(chars);
						    }
						    else
                            {
							    newfile = string.Format("Null File {0}", i);
						    }

						    records[i].filename = NormalizeRecordPath(newfile);
					    }   
				    }

				    // Now convert the array of records into a hash table.
				    Hashtable hash = new Hashtable(numEntries);

				    for (i = 0; i < numEntries; ++i) 
                    {
					    if (records[i].IsActive) 
                        {
						    hash.Add(records[i].filename, records[i]);
					    }
				    }
				    m_dir = hash;

                    BuildKeyTable();
			    }
			    finally 
                {
				    reader.Close();
			    }
		    }
            catch (IOException) 
            {
			    // silently eat these errors
		    }
	    }
        string NormalizeRecordPath(string recordID)
        {
            // lowercase it
            string ans = recordID.ToLower(System.Globalization.CultureInfo.InvariantCulture);
            // replace any '/' with '\\'
            ans = ans.Replace('/', '\\');
            return ans;
        }

        private void BuildKeyTable()
        {
            int index = 0;
            m_keys = new string[m_dir.Count];
            foreach (string filename in m_dir.Keys)
            {
                m_keys[index] = filename;
                index++;
            }
            Array.Sort(m_keys);
        }

        public string[] KeyTable
        {
            get
            {
                return m_keys;
            }
        }
    }
}
