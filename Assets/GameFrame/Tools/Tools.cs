﻿using System;
using System.Collections;
using System.Collections.Generic;
using UIFrameWork;
using UnityEngine;


	public class Tools
	{

		public const string UI_PREFABPATH = "UIPrefab/";
		public static Type GetUIScripeByType(WindowType type)
		{
			Type ret = null;
			switch (type)
			{
				case WindowType.LoginAndRegister:
				    ret = typeof(LoginAndRegister);
					break;
                case WindowType.GameInfo:
                ret = typeof(GameInfo);
                break;

        }
			return ret;
		}

		public static string GetPrefabPathByType(WindowType type)
		{
			string _path = String.Empty;
			switch (type)
			{
				case WindowType.LoginAndRegister:
					//_path = UI_PREFABPATH + "/Login";
					_path = "LoginAndRegister";
					break;
			    case WindowType.GameInfo:
			        //_path = UI_PREFABPATH + "/Login";
			        _path = "GameInfo";
			        break;
        }
			return _path;
		}
	}

