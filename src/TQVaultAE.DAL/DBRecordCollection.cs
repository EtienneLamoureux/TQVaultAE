//-----------------------------------------------------------------------
// <copyright file="DBRecordCollection.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.DAL
{
	using System.Collections;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;

	/// <summary>
	/// Class for encapsulating a record in the database.
	/// </summary>
	public class DBRecordCollection : IEnumerable<Variable>
	{
		/// <summary>
		/// Dictionary which holds all of the variables.
		/// </summary>
		private Dictionary<string, Variable> variables;

		/// <summary>
		/// Initializes a new instance of the DBRecordCollection class.
		/// </summary>
		/// <param name="id">string: ID for this record.</param>
		/// <param name="recordType">string: type for this record</param>
		public DBRecordCollection(string id, string recordType)
		{
			this.Id = id;
			this.RecordType = recordType;
			this.variables = new Dictionary<string, Variable>();
		}

		/// <summary>
		/// Gets the ID for this record.
		/// </summary>
		public string Id { get; private set; }

		/// <summary>
		/// Gets the RecordType
		/// </summary>
		public string RecordType { get; private set; }

		/// <summary>
		/// Gets the number of variables in the hashtable.
		/// </summary>
		public int Count
		{
			get
			{
				return this.variables.Count;
			}
		}

		/// <summary>
		/// Gets a Variable from the hashtable.
		/// </summary>
		/// <param name="variableName">Name of the variable we are looking up.</param>
		/// <returns>Returns a Variable from the hashtable.</returns>
		public Variable this[string variableName]
		{
			get
			{
				try
				{
					return this.variables[variableName.ToUpperInvariant()];
				}
				catch (KeyNotFoundException)
				{
					return null;
				}
			}
		}

		/// <summary>
		/// Enumerates all of the variables in this DBrecord
		/// </summary>
		/// <returns>Each Variable in the record.</returns>
		public IEnumerator<Variable> GetEnumerator()
		{
			foreach (KeyValuePair<string, Variable> kvp in this.variables)
			{
				yield return kvp.Value;
			}
		}

		/// <summary>
		/// Non Generic enumerable interface.
		/// </summary>
		/// <returns>Generic interface implementation.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		/// <summary>
		/// Adds a variable to the hashtable.
		/// </summary>
		/// <param name="variable">Variable that we are adding.</param>
		public void Set(Variable variable)
		{
			this.variables.Add(variable.Name.ToUpperInvariant(), variable);
		}

		/// <summary>
		/// Returns a short descriptive string in the format of:
		/// recordID,recordType,numVariables
		/// </summary>
		/// <returns>string: recordID,recordType,numVariables</returns>
		public string ToShortString()
		{
			return string.Format(CultureInfo.CurrentCulture, "{0},{1},{2}", this.Id, this.RecordType, this.Count);
		}

		/// <summary>
		/// Gets the integer value for the variable, or 0 if the variable does not exist.
		/// throws exception of the variable is not integer type
		/// </summary>
		/// <param name="variableName">Name of the variable we are looking up.</param>
		/// <param name="index">Offset of the value in the array since a variable can have multiple values.</param>
		/// <returns>Returns the integer value for the variable, or 0 if the variable does not exist.</returns>
		public int GetInt32(string variableName, int index)
		{
			try
			{
				return this.variables[variableName.ToUpperInvariant()].GetInt32(index);
			}
			catch (KeyNotFoundException)
			{
				return 0;
			}
		}

		/// <summary>
		/// Gets the float value for the variable, or 0 if the variable does not exist.
		/// throws exception of the variable is not float type
		/// </summary>
		/// <param name="variableName">Name of the variable we are looking up.</param>
		/// <param name="index">Offset of the value in the array since a variable can have multiple values.</param>
		/// <returns>Returns the float value for the variable, or 0 if the variable does not exist.</returns>
		public float GetSingle(string variableName, int index)
		{
			try
			{
				return this.variables[variableName.ToUpperInvariant()].GetSingle(index);
			}
			catch (KeyNotFoundException)
			{
				return 0.0F;
			}
		}

		/// <summary>
		/// Gets the string value for the variable, or empty string if the variable does not exist.
		/// </summary>
		/// <param name="variableName">Name of the variable we are looking up.</param>
		/// <param name="index">Offset of the value in the array since a variable can have multiple values.</param>
		/// <returns>Returns the string value for the variable, or empty string if the variable does not exist.</returns>
		public string GetString(string variableName, int index)
		{
			Variable variable;

			if (variables.ContainsKey(variableName.ToUpperInvariant()))
			{
				variable = this.variables[variableName.ToUpperInvariant()];
			}
			else
			{
				return string.Empty;
			}

			string answer = variable.GetString(index);
			if (answer == null)
			{
				return string.Empty;
			}

			return answer;
		}

		/// <summary>
		/// Gets all of the string values for a particular variable entry
		/// since some values can have multiple entries.
		/// </summary>
		/// <param name="variableName">Name of the variable.</param>
		/// <returns>Returns a string array of the string values.</returns>
		public string[] GetAllStrings(string variableName)
		{
			Variable variable;
			try
			{
				variable = this.variables[variableName.ToUpperInvariant()];
			}
			catch (KeyNotFoundException)
			{
				return null;
			}

			string[] ansArray = new string[variable.NumberOfValues];
			for (int i = 0; i < variable.NumberOfValues; ++i)
			{
				ansArray[i] = variable.GetString(i);
			}

			return ansArray;
		}

		/// <summary>
		/// Writes all variables into a file.
		/// </summary>
		/// <param name="baseFolder">Path in the file.</param>
		/// <param name="fileName">file name to be written</param>
		public void Write(string baseFolder, string fileName = null)
		{
			// construct the full path
			string fullPath = Path.Combine(baseFolder, this.Id);
			string destinationFolder = Path.GetDirectoryName(fullPath);

			if (fileName != null)
			{
				fullPath = Path.Combine(baseFolder, fileName);
				destinationFolder = baseFolder;
			}

			// Create the folder path if necessary
			if (!Directory.Exists(destinationFolder))
			{
				Directory.CreateDirectory(destinationFolder);
			}

			// Open the file
			using (StreamWriter outStream = new StreamWriter(fullPath, false))
			{
				// Write all the variables
				foreach (Variable variable in this)
				{
					outStream.WriteLine(variable.ToString());
				}
			}
		}
	}
}