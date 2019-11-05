using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace TQVaultAE.Domain.Entities
{
	public class SkillRecord
	{
		internal static readonly Encoding Encoding1252 = Encoding.GetEncoding(1252);

		public string skillName { get; set; }
		public int skillLevel { get; set; }
		public int skillEnabled { get; set; }
		public int skillSubLevel { get; set; }
		public int skillActive { get; set; }
		public int skillTransition { get; set; }

		/// <summary>
		/// Binary serialize
		/// </summary>
		/// <returns></returns>
		public byte[] ToBinary(int beginBlockValue, int endBlockValue)
		{
			var array = new[] {

				BitConverter.GetBytes("begin_block".Length),
				Encoding1252.GetBytes("begin_block"),
				BitConverter.GetBytes(beginBlockValue),

				BitConverter.GetBytes(nameof(skillName).Length),
				Encoding1252.GetBytes(nameof(skillName)),
				BitConverter.GetBytes(skillName.Length),
				Encoding1252.GetBytes(skillName),

				BitConverter.GetBytes(nameof(skillLevel).Length),
				Encoding1252.GetBytes(nameof(skillLevel)),
				BitConverter.GetBytes(skillLevel),

				BitConverter.GetBytes(nameof(skillEnabled).Length),
				Encoding1252.GetBytes(nameof(skillEnabled)),
				BitConverter.GetBytes(skillEnabled),

				BitConverter.GetBytes(nameof(skillSubLevel).Length),
				Encoding1252.GetBytes(nameof(skillSubLevel)),
				BitConverter.GetBytes(skillSubLevel),

				BitConverter.GetBytes(nameof(skillActive).Length),
				Encoding1252.GetBytes(nameof(skillActive)),
				BitConverter.GetBytes(skillActive),

				BitConverter.GetBytes(nameof(skillTransition).Length),
				Encoding1252.GetBytes(nameof(skillTransition)),
				BitConverter.GetBytes(skillTransition),

				BitConverter.GetBytes("end_block".Length),
				Encoding1252.GetBytes("end_block"),
				BitConverter.GetBytes(endBlockValue),

			}.SelectMany(arr => arr).ToArray();

			return array;
		}
	}
}
