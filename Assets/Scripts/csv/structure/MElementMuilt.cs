using CSV;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using BattleFramework;

namespace MMO
{
	[System.Serializable]
	public class MElementMuilt : BaseCSVStructure
	{
		[CsvColumn (CanBeNull = true)]
		public string element;
		[CsvColumn (CanBeNull = true)]
		public int red;
		[CsvColumn (CanBeNull = true)]
		public int green;
		[CsvColumn (CanBeNull = true)]
		public int blue;
		[CsvColumn (CanBeNull = true)]
		public int white;
		[CsvColumn (CanBeNull = true)]
		public int black;
		[CsvColumn (CanBeNull = true)]
		public int none;
		[CsvColumn (CanBeNull = true)]
		public int cut;
		[CsvColumn (CanBeNull = true)]
		public int destroy;
		[CsvColumn (CanBeNull = true)]
		public int bulge;
	}
}
