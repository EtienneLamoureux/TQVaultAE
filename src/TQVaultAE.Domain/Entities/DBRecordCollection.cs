//-----------------------------------------------------------------------
// <copyright file="DBRecordCollection.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Domain.Entities;

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

/// <summary>
/// Class for encapsulating a record in the database.
/// </summary>
public class DBRecordCollection : IEnumerable<Variable>
{
	/// <summary>
	/// Dictionary which holds all of the variables, by case-insensitive name (variable names are ASCII).
	/// </summary>
	private Dictionary<string, Variable> variables;

	/// <summary>
	/// Initializes a new instance of the DBRecordCollection class.
	/// </summary>
	/// <param name="id">string: ID for this record.</param>
	/// <param name="recordType">string: type for this record</param>
	/// <param name="capacity">Number of variables the record will hold, to size the dictionary once.</param>
	public DBRecordCollection(RecordId id, string recordType, int capacity = 0)
	{
		this.Id = id;
		this.RecordType = recordType;
		this.variables = new Dictionary<string, Variable>(capacity, StringComparer.OrdinalIgnoreCase);
	}

	/// <summary>
	/// Gets the ID for this record.
	/// </summary>
	public RecordId Id { get; private set; }

	/// <summary>
	/// The database the record was read from: a record a mod overrides, read from the mod's database, is the mod's.
	/// </summary>
	public RecordSource Source { get; init; } = RecordSource.BaseGame;

	/// <summary>
	/// Gets the RecordType
	/// </summary>
	public string RecordType { get; private set; }

	/// <summary>
	/// Gets the number of variables in the hashtable.
	/// </summary>
	public int Count =>this.variables.Count;

	/// <summary>
	/// Gets a Variable from the hashtable.
	/// </summary>
	/// <param name="variableName">Name of the variable we are looking up.</param>
	/// <returns>Returns a Variable from the hashtable.</returns>
	public Variable? this[string variableName]
		=> this.variables.TryGetValue(variableName, out var val) ? val : null;

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
	IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

	/// <summary>
	/// Adds a variable to the hashtable.
	/// </summary>
	/// <param name="variable">Variable that we are adding.</param>
	public void Set(Variable variable) 
		=> this.variables.Add(variable.Name, variable);

	/// <summary>
	/// Gets the integer value for the variable, or 0 if the variable does not exist.
	/// throws exception of the variable is not integer type
	/// </summary>
	/// <param name="variableName">Name of the variable we are looking up.</param>
	/// <param name="index">Offset of the value in the array since a variable can have multiple values.</param>
	/// <returns>Returns the integer value for the variable, or 0 if the variable does not exist.</returns>
	public int GetInt32(string variableName, int index)
		=> this.variables.TryGetValue(variableName, out var val) ? val.GetInt32(index) : 0;

	/// <summary>
	/// Gets the float value for the variable, or 0 if the variable does not exist.
	/// throws exception of the variable is not float type
	/// </summary>
	/// <param name="variableName">Name of the variable we are looking up.</param>
	/// <param name="index">Offset of the value in the array since a variable can have multiple values.</param>
	/// <returns>Returns the float value for the variable, or 0 if the variable does not exist.</returns>
	public float GetSingle(string variableName, int index)
		=> this.variables.TryGetValue(variableName, out var val) ? val.GetSingle(index) : 0.0F;

	/// <summary>
	/// Gets the string value for the variable, or empty string if the variable does not exist.
	/// </summary>
	/// <param name="variableName">Name of the variable we are looking up.</param>
	/// <param name="index">Offset of the value in the array since a variable can have multiple values.</param>
	/// <returns>Returns the string value for the variable, or empty string if the variable does not exist.</returns>
	public string GetString(string variableName, int index)
		=> this.variables.TryGetValue(variableName, out var val) ? val.GetString(index) ?? string.Empty : string.Empty;

	/// <summary>
	/// Gets all of the string values for a particular variable entry
	/// since some values can have multiple entries.
	/// </summary>
	/// <param name="variableName">Name of the variable.</param>
	/// <returns>Returns a string array of the string values.</returns>
	public string[]? GetAllStrings(string variableName)
	{
		if (!this.variables.TryGetValue(variableName, out var variable))
			return null;

		string[] ansArray = new string[variable.NumberOfValues];
		for (int i = 0; i < variable.NumberOfValues; ++i)
			ansArray[i] = variable.GetString(i);

		return ansArray;
	}

	/// <summary>
	/// Groups numbered variables by their number: <c>randomizerName3</c> and <c>randomizerWeight3</c> for the prefixes
	/// <c>randomizerName</c> and <c>randomizerWeight</c> (case ignored). Slot <c>i</c> of a group holds the variable of
	/// <c>prefixes[i]</c>, null when the record has none. Variables that <paramref name="include"/> rejects are left
	/// out, and the groups keep the order in which each number first has a variable.
	/// </summary>
	/// <param name="include">Which variables count.</param>
	/// <param name="prefixes">Names of the numbered variables, without their number.</param>
	/// <returns>The groups, keyed by the number (the end of the variable names).</returns>
	public Dictionary<string, Variable?[]> GroupNumberedVariables(Func<Variable, bool> include, params string[] prefixes)
	{
		var groups = new Dictionary<string, Variable?[]>();
		foreach (var variable in this)
		{
			int slot = Array.FindIndex(prefixes, prefix => variable.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
			if (slot < 0 || !include(variable))
				continue;

			var number = variable.Name.Substring(prefixes[slot].Length);
			if (!groups.TryGetValue(number, out var group))
				groups.Add(number, group = new Variable?[prefixes.Length]);

			group[slot] = variable;
		}

		return groups;
	}

}