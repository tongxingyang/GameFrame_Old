﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIFrameWork
{
	public class Tools
	{

		public const string UI_PREFABPATH = "UIPrefab/";
		public static Type GetUIScripeByType(WindowType type)
		{
			Type ret = null;
			switch (type)
			{
				case WindowType.Login:
					//ret = typeof("脚本名");
					break;
			}
			return ret;
		}

		public static string GetPrefabPathByType(WindowType type)
		{
			string _path = String.Empty;
			switch (type)
			{
				case WindowType.Login:
					_path = UI_PREFABPATH + "/Login";
					break;
			}
			return _path;
		}
	}
}