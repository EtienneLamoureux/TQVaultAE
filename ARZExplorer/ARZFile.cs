using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Collections;
using System.IO;
using zlib;

namespace ARZExplorer
{
    public class ARZFile
    {
		// The different data-types a variable can be.
		public enum VariableDataType
		{
			Integer = 0, // values will be Int32
			Float = 1, // values will be Single
			StringVar = 2, // Values will be string
			Boolean = 3, // Values will be Int32
			Unknown = 4 // values will be Int32
		};

		// A variable within a DB Record
		public class Variable
		{
			private string m_variableName;
			private VariableDataType m_dataType;
			private object[] m_value; // the values
		
			public Variable(string variableName, VariableDataType dataType, int numValues)
			{
				m_variableName = variableName;
				m_dataType = dataType;
				m_value = new object[numValues];
			}

			public string Name
            {
                get
                {
                    return m_variableName;
                }
			}
			
			public VariableDataType DataType 
            {
				get 
                { 
                    return m_dataType;
                }
			}

			public int NumValues
            {
				get 
                {
                    return m_value.Length;
                }
			}

			public object this [int index]
            {
				get
               {
                    return m_value[index]; 
                }
				set
                {
                    m_value[index] = value; 
                }
			}

			// throws exception if value is not the correct type
            public int GetInt32(int i)
			{
				return Convert.ToInt32(m_value[i]);
			}
            public float GetFloat(int i)
			{
				return Convert.ToSingle(m_value[i]);
			}
            public string GetString(int i)
			{
				return Convert.ToString(m_value[i]);
			}

            public override string ToString() 
            {
    	    	// Format is name,val1;val2;val3;val4;...;valn,
    	    	// First set our val format string based on the data type
        		string formatSpec = "{0}";
		        if(DataType == VariableDataType.Float) formatSpec = "{0:f6}";
        		StringBuilder ans = new StringBuilder(64);
		        ans.Append(Name);
        		ans.Append(",");

		        for (int i = 0; i < NumValues; ++i) 
                {
        			if (i > 0) ans.Append (";");
			        ans.AppendFormat(CultureInfo.InvariantCulture, formatSpec, m_value[i]);
    		    }
		        ans.Append(",");
        		return ans.ToString();
	        }

            public string ToStringValue()
            {
        		// Format is name,val1;val2;val3;val4;...;valn,
        		// First set our val format string based on the data type
        		string formatSpec = "{0}";
        		if(DataType == VariableDataType.Float) formatSpec = "{0:f6}";
        		StringBuilder ans = new StringBuilder(64);
		        for(int i = 0; i < NumValues; ++i) 
                {
        			if(i > 0) ans.Append(", ");
			        //ans.Append(m_value[i]);
                    ans.AppendFormat(CultureInfo.InvariantCulture, formatSpec, m_value[i]);
        		}
        		return ans.ToString();
	        }

            /*public string ToStringNice() 
            {
		        StringBuilder ans = new StringBuilder(64);
        		ans.Append(Database.sDb.GetItemAttributeFriendlyText(Name));
		        ans.Append(": ");
		        ans.Append(ToStringValue());
	        	return ans.ToString();
        	}*/
		};

        public class DBRecord
		{
			private string m_id;
			private string m_recordType;
			private Hashtable m_variables;
		
			public DBRecord(string id, string recordType)
			{
				m_id = id;
				m_recordType = recordType;
				m_variables = new Hashtable();
			}

			public string ID
            {
                get
                {
                    return m_id; 
                }
			}

			public string RecordType
            {
                get
                    {
                    return m_recordType; 
                }
			}

            public Variable this[string variableName] 
            {
                get
                {
                    return (Variable)m_variables[variableName];
                }   
			}

			public int Count
            {
                get
                {
                    return m_variables.Count; 
                }
			}

			public void Set(Variable v) { m_variables.Add(v.Name, v); }
			// Returns the variables collection
			public ICollection GetVariableEnumerable() { return m_variables.Values; }

			// Returns a short descriptive string: recordID,recordType,numVariables
			public string ToShortString()
			{
				return string.Format("{0},{1},{2}", m_id, m_recordType, Count);
			}

			// Returns the value for the variable, or 0 if the variable does not exist.
			// throws exception of the variable is not integer type
			public int GetInt32(string variableName, int i)
			{
				Variable v = (Variable) m_variables[variableName];
				if(v == null) return 0;
				return v.GetInt32(i);
			}
			public float GetFloat(string variableName, int i)
			{
                Variable v = (Variable) m_variables[variableName];
				if(v == null) return 0;
				return v.GetFloat(i);
			}
			// returns "" if the variable does not exist
			public string GetString(string variableName, int i)
			{
                Variable v = (Variable) m_variables[variableName];
				if(v == null) return "";
				string ans = v.GetString(i);
				if (ans == null) return "";
				return ans;
			}

			// Added by VillageIdiot
			// Gets all of the string values for a particular variable entry
			public string[] GetAllStrings (string variableName) 
            {
                Variable v = (Variable) m_variables[variableName];
				if(v == null) return null;
				string[] ansArray =  new string[v.NumValues];
				for(int i = 0; i < v.NumValues ; ++i) 
                {
					ansArray[i] = v.GetString(i);
				}
				return ansArray;
			}

            public void Write(string baseFolder)
            {
                Write(baseFolder, null);
            }

	        public void Write(string baseFolder, string fileName) 
            {
		        // construct the full path
                string fullPath;
                string destFolder;
                if (fileName == null)
                {
                    fullPath = Path.Combine(baseFolder, ID);
                    destFolder = Path.GetDirectoryName(fullPath);
                }
                else
                {
                    fullPath = Path.Combine(baseFolder, fileName);
                    destFolder = baseFolder;
                }

		        // Create the folder path if necessary
		        if(!Directory.Exists(destFolder)) 
                {
			        Directory.CreateDirectory(destFolder);
        		}

        		// Open the file
		        StreamWriter outStream = new StreamWriter(fullPath, false);
        		try 
                {
        			// Write all the variables
                    foreach ( Variable v in GetVariableEnumerable() )
                    {
				        outStream.WriteLine(v.ToString());
        			}
        		}
		        finally 
                {
			        outStream.Close();
		        }
	        }
        };

	private
		class RecordInfo
		{
			private int m_offset;
			private int m_compressedSize;
			private int m_idstringIndex;
			private string m_id;
			private string m_recordType;
			private int m_crap1; // some sort of timestamp?
			private int m_crap2; // some sort of timestamp?

			public RecordInfo ()
			{
				m_offset = 0;
				m_compressedSize = 0;
				m_idstringIndex = -1;
				m_id = null;
				m_recordType = "";
				m_crap1 = 0;
				m_crap2 = 0;
			}

			public string ID 
            {
                get
                {
                    return m_id; 
                }
			}
			public string RecordType 
            {
                get
                {
                    return m_recordType; 
                }
			}

	        public void Decode(BinaryReader inReader, int baseOffset, ARZFile arzFile) 
            {
	        	// Record Entry Format
        		// 0x0000 int32 stringEntryID (dbr filename)
        		// 0x0004 int32 string length
        		// 0x0008 string (record type)
        		// 0x00?? int32 offset
		        // 0x00?? int32 length in bytes
        		// 0x00?? int32 timestamp?
		        // 0x00?? int32 timestamp?
        		m_idstringIndex = inReader.ReadInt32();
		        m_recordType = ReadCString(inReader);

		        m_offset = inReader.ReadInt32() + baseOffset;
		        m_compressedSize = inReader.ReadInt32();
		        m_crap1 = inReader.ReadInt32();
		        m_crap2 = inReader.ReadInt32();

		        // Get the ID string
		        m_id = arzFile.Getstring(m_idstringIndex);
	        }

	        public DBRecord Decompress(ARZFile arzFile) 
            {
		        // record variables have this format:
		        // 0x00 int16 specifies data type:
		        //		0x0000 = int - data will be an int32
		        //      0x0001 = float - data will be a Single
		        //		0x0002 = string - data will be an int32 that is index into string table
		        //      0x0003 = bool - data will be an int32
		        // 0x02 int16 specifies number of values (usually 1, but sometimes more (for arrays)
		        // 0x04 int32 key string ID (the id into the string table for this variable name
		        // 0x08 data value
		        byte[] data = DecompressBytes(arzFile);
		        // Lets dump the file to disk
		        //System.IO.FileStream dump = new System.IO.FileStream("recdump.dat", System.IO.FileMode.Create, System.IO.FileAccess.Write);
		        //try
		        //{
		        //	dump.Write(data, 0, data.Length);
		        //}
		        //finally
		        //{
		        //	dump.Close();
		        //}

        		int numDWords = data.Length / 4;
		        int numVariables = numDWords / 3;

        		if(data.Length % 4 != 0) 
                {
			        throw new ApplicationException(string.Format("Error while parsing arz record {0}, data Length = {1} which is not a multiple of 4", ID, (int)data.Length));
		        }

		        DBRecord record = new DBRecord(ID, RecordType);

		        // Create a memory stream to read the binary data
		        MemoryStream inStream = new MemoryStream(data, false);
		        BinaryReader inReader = new BinaryReader(inStream);
		        try 
                {
			        int i = 0;
			        while (i < numDWords)
                    {
				        int pos = (int) inReader.BaseStream.Position;
				        short dataType = inReader.ReadInt16();
				        short valCount = inReader.ReadInt16();
				        int variableID = inReader.ReadInt32();
				        string variableName = arzFile.Getstring(variableID);

				        if(variableName == null) 
                        {
					        throw new ApplicationException(string.Format("Error while parsing arz record {0}, variable is NULL", ID));
				        }

        				if(dataType < 0 || dataType > 3) 
                        {
		        			throw new ApplicationException(string.Format("Error while parsing arz record {0}, variable {2}, bad dataType {3}", ID, variableName, dataType));
				        }
				        Variable v = new Variable(variableName, (VariableDataType) dataType, valCount);

				        if(valCount < 1)
                        {
					        throw new ApplicationException (string.Format ("Error while parsing arz record {0}, variable {1}, bad valCount {2}", ID, variableName, valCount));
				        }

				        i += 2 + valCount; // increment our dword count

				        for(int j = 0; j < valCount; ++j) 
                        {
					        switch (v.DataType) {
						        case VariableDataType.Integer:
						        case VariableDataType.Boolean:
							        {
								        int val = inReader.ReadInt32();
								        v[j] = val;
								        break;
							        }
						        case VariableDataType.Float:
							        {
								        float val = inReader.ReadSingle();
								        v[j] = val;
								        break;
							        }
						        case VariableDataType.StringVar:
							        {
								        int id = inReader.ReadInt32();
								        string val = arzFile.Getstring(id);
								        if (val == null) val = "";
								        else val = val.Trim();
								        v[j] = val;
								        break;
							        }
						        default:
							        {
								        int val = inReader.ReadInt32();
								        v[j] = val;
								        break;
							        }
					        }
				        }
				        record.Set(v);
			        }
		        }
		        finally {
			        inReader.Close();
		        }
		        return record;
	        }
		
        private byte[] DecompressBytes(ARZFile arzFile)
        {
    		// Read in the compressed data and decompress it, storing the results in a memorystream
	    	FileStream arzStream  = new FileStream(arzFile.m_filename, FileMode.Open, FileAccess.Read, FileShare.Read);
		    try 
            {
			    arzStream.Seek(m_offset, SeekOrigin.Begin);

    			// Create a decompression stream
	    		ZInputStream zinput = new ZInputStream(arzStream);
		    	// Create a memorystream to hold the decompressed data
			    MemoryStream outStream = new MemoryStream();
			    try 
                {
    				// Now decompress
	    			byte[] buffer = new byte[1024];
		    		int len;
			    	while((len = zinput.read(buffer, 0, 1024)) > 0) 
                    {
				    	outStream.Write(buffer, 0, len);
				    }

				    // Now create a final Byte array to hold the answer
    				byte[] ans = new byte[(int)zinput.TotalOut];

	    			// Copy the data into it
		    		Array.Copy(outStream.GetBuffer(), ans, (int) zinput.TotalOut);
				
			    	// Return the decompressed data
				    return ans;
			    }
			    finally
                {
				    outStream.Close();
			    }
		    }
		    finally 
            {
			    arzStream.Close();
		    }
	    }
    };

	    private	string m_filename;
		private string[] m_strings;
		private Hashtable m_recordInfo; // keyed by their id
		private Hashtable m_cache; // DBRecords cached as they get requested
        private string[] m_keys;

        public static ARZFile arzFile = null;
	
        public ARZFile(string filename)
		{
			m_filename = filename;
			m_cache = new Hashtable();
			m_recordInfo = null;
            m_keys = null;
		}

	    public bool Read()
	    {
    		StreamWriter outStream = null; // new System.IO.StreamWriter("arzOut.txt", false);
            try
            {
                // ARZ header file format
                //
                // 0x000000 int32
                // 0x000004 int32 start of dbRecord table
                // 0x000008 int32 size in bytes of dbRecord table
                // 0x00000c int32 numEntries in dbRecord table
                // 0x000010 int32 start of string table
                // 0x000014 int32 size in bytes of string table
                FileStream instream = new FileStream(m_filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                BinaryReader reader = new BinaryReader(instream);

                try
                {
                    int[] header = new int[6];

                    for (int i = 0; i < 6; ++i)
                    {
                        header[i] = reader.ReadInt32();
                        if (outStream != null) outStream.WriteLine("Header[{0}] = {1:n0} (0x{1:X})", i, header[i]);
                    }
                    int firstTableStart = header[1];
                    int firstTableSize = header[2];
                    int firstTableCount = header[3];
                    int secondTableStart = header[4];
                    int secondTableSize = header[5];

                    if (!ReadstringTable(secondTableStart, secondTableSize, reader, outStream))
                        throw new Exception("Error Reading String Table");
                    if (!ReadRecordTable(firstTableStart, firstTableCount, reader, outStream))
                        throw new Exception("Error Reading Record Table");

                    // 4 final int32's from file
                    // first int32 is numstrings in the stringtable
                    // second int32 is something ;)
                    // 3rd and 4th are crap (timestamps maybe?)
                    for (int i = 0; i < 4; ++i)
                    {
                        int val = reader.ReadInt32();
                        if (outStream != null) outStream.WriteLine("{0:n0} 0x{0:X}", val);
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    reader.Close();
                }

                // Let's examine *all* the records
                //IEnumerator recs = this.GetRecordIDEnumerator();
                //while(recs.MoveNext())
                //{
                //	DBRecord r = GetRecordUnCached((string) (recs.Current));
                //	out.WriteLine(r.ToShortstring());
                //}
            }
            catch (Exception)
            {
                return false;
            }
		    finally
		    {
			    if (outStream != null) outStream.Close();
    		}
            BuildKeyTable();
            return true;
	    }

        private void BuildKeyTable()
        {            
            int index = 0;
            m_keys = new string[m_recordInfo.Count];
            foreach ( string recordID in m_recordInfo.Keys)
            {
                m_keys[index] = recordID;
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

		public DBRecord Get_Item(string recordID) 
        {
			recordID = NormalizeRecordPath(recordID);
			DBRecord ans = (DBRecord) (m_cache[recordID]);
			if (ans == null)
			{
				RecordInfo rawRecord = (RecordInfo) m_recordInfo[recordID];
				if (rawRecord == null) return null; // record not found
	
                ans = rawRecord.Decompress(this);
				m_cache.Add(recordID, ans);
			}
			return ans;
		}

		// the number of DBRecords
		public int Count
        {
			get
            {
                return m_recordInfo.Count;
            }
		}

		// The Item property caches the DBRecords, which is great when you
		// are only using a few 100 (1000?) records and are requesting
		// them many times.  Not great if you are looping through all the
		// records as it eats alot of memory.  This method will create
		// the record on the fly if it is not in the cache so when you are
		// done with it, it can be reclaimed by the garbage collector.
		// Great for when you want to loop through all the records for
		// some reason.  It will take longer, but use less memory.
		public DBRecord GetRecordUnCached(string recordID)
		{
			recordID = NormalizeRecordPath(recordID);
			// If it is already in the cache no need not to use it
			DBRecord ans = (DBRecord) (m_cache[recordID]);
			if(ans != null) return ans;

			RecordInfo rawRecord = (RecordInfo) (m_recordInfo[recordID]);
			if(rawRecord == null) return null; // record not found

			return rawRecord.Decompress(this);
		}
		// Returns an enumerator that will cycle through all the recordID's stored in the DB.
		// you can then get each DBRecord you are interested in
		public IEnumerator GetRecordIDEnumerator() { return m_recordInfo.Keys.GetEnumerator(); }
        public ICollection GetRecordIDEnumerable() { return m_recordInfo.Keys; }
        
		// Retrieves a string from the string table.
		private string Getstring (int i)
		{
			return m_strings[i];
		}
	
        private bool ReadstringTable(int pos, int size, BinaryReader reader, StreamWriter outStream)
	    {
            try
            {
                // string Table Format
                // first 4 bytes is the number of entries
                // then
                // one string followed by another...
                reader.BaseStream.Seek(pos, SeekOrigin.Begin);
                int numstrings = reader.ReadInt32();

                m_strings = new string[numstrings];

                int finalPos = pos + size;

                if (outStream != null) outStream.WriteLine("stringTable located at 0x{1:X} numstrings= {0:n0}", numstrings, pos);

                for (int i = 0; i < numstrings; ++i)
                {
                    m_strings[i] = ReadCString(reader);

                    if (outStream != null) outStream.WriteLine("{0},{1}", i, m_strings[i]);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
	    }

    	private bool ReadRecordTable(int pos, int numEntries, BinaryReader reader, StreamWriter outStream)
        {
            try
            {
                m_recordInfo = new Hashtable((int)Math.Round(numEntries * 1.2), (float)1.0);
                reader.BaseStream.Seek(pos, SeekOrigin.Begin);

                if (outStream != null) outStream.WriteLine("RecordTable located at 0x{0:X}", pos);

                for (int i = 0; i < numEntries; ++i)
                {
                    RecordInfo r = new RecordInfo();
                    r.Decode(reader, 24, this); // 24 is the offset of where all record data begins

                    m_recordInfo.Add(NormalizeRecordPath(r.ID), r);

                    // output this record
                    if (outStream != null) outStream.WriteLine("{0},{1},{2}", i, r.ID, r.RecordType);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
	    }

        // Reads a string from the binary stream
        public static string ReadCString(BinaryReader reader)
        {
            // first 4 bytes is the string length, followed by the string.
            int len = reader.ReadInt32();

            // Convert the next len bytes into a string
            Encoding ascii = Encoding.GetEncoding(1252);

            byte[] rawData = reader.ReadBytes(len);

            char[] chars = new char[ascii.GetCharCount(rawData, 0, len)];
            ascii.GetChars(rawData, 0, len, chars, 0);

            string ans = new string(chars);

            return ans;
        }

        public static string NormalizeRecordPath(string recordID)
        {
            // lowercase it
            string ans = recordID.ToLower(System.Globalization.CultureInfo.InvariantCulture);
            // replace any '/' with '\\'
            ans = ans.Replace('/', '\\');
            return ans;
        }
    }
}
