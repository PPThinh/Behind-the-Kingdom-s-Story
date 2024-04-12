using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace RapidIconUIC
{
	[Serializable]
	public class IconData
	{
		public List<Icon> icons;

		public IconData()
		{
			icons = new List<Icon>();
		}
	}
}