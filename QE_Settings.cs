﻿/* 
QuickExit
Copyright 2016 Malah

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
	public class QSettings : QuickExit {

		[KSPField(isPersistant = true)]	private static readonly QSettings instance = new QSettings ();
		public static QSettings Instance {
			get {
				if (!instance.isLoaded) {
					instance.Load ();
				}
				return instance;
			}
		}

		internal static string FileConfig = KSPUtil.ApplicationRootPath + "GameData/" + MOD + "/Config.txt";

		[KSPField(isPersistant = true)]	private bool isLoaded = false;

		[Persistent] internal bool Debug = true;
		[Persistent] internal bool CountDown = true;
		[Persistent] internal bool AutomaticSave = true;
		[Persistent] internal string Key = "f7";
		[Persistent] internal bool StockToolBar = true;
		[Persistent] internal bool StockToolBar_ModApp = true;
		[Persistent] internal bool BlizzyToolBar = true;

		public void Save() {
			ConfigNode _temp = ConfigNode.CreateConfigFromObject(this, new ConfigNode());
			_temp.Save(FileConfig);
			Log ("Settings Saved", "QSettings", true);
		}

		public void Load() {
			if (File.Exists (FileConfig)) {
				try {
					ConfigNode _temp = ConfigNode.Load (FileConfig);
					ConfigNode.LoadObjectFromConfig (this, _temp);
				} catch {
					Save ();
				}
				Log ("Settings Loaded", "QSettings", true);
			} else {
				Save ();
			}
			isLoaded = true;
		}
	}
}