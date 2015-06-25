﻿/* 
QuickExit
Copyright 2015 Malah

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>. 
*/

using System;
using System.IO;
using UnityEngine;

namespace QuickExit {
	public class QSettings : MonoBehaviour {

		public readonly static QSettings Instance = new QSettings();

		internal static string FileConfig = KSPUtil.ApplicationRootPath + "GameData/" + Quick.MOD + "/Config.txt";

		[Persistent] internal bool CountDown = true;
		[Persistent] internal bool AutomaticSave = true;
		[Persistent] internal string Key = "f7";

		#if GUI
		[Persistent] internal bool StockToolBar = true;
		[Persistent] internal bool StockToolBar_inlast = false;
		[Persistent] internal bool BlizzyToolBar = true;
		#endif

		public void Save() {
			ConfigNode _temp = ConfigNode.CreateConfigFromObject(this, new ConfigNode());
			_temp.Save(FileConfig);
			Quick.Log ("Settings Saved");
		}

		public void Load() {
			if (File.Exists (FileConfig)) {
				try {
					ConfigNode _temp = ConfigNode.Load (FileConfig);
					ConfigNode.LoadObjectFromConfig (this, _temp);
					Quick.Log ("Settings Loaded");
				} catch {
					Save ();
				}
			} else {
				Save ();
			}
		}
	}
}