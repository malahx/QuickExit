/* 
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
using System.Collections.Generic;
using UnityEngine;

namespace QuickExit {

	public class QGUI : MonoBehaviour {
		#if GUI
		internal static bool WindowSettings = false;
		private static Rect RectSettings = new Rect();

		internal static void RefreshRect() {
			RectSettings.x = (Screen.width - RectSettings.width) / 2;
			RectSettings.y = (Screen.height - RectSettings.height) / 2;
		}
		#endif

		internal static void Lock(bool activate, ControlTypes Ctrl) {
			if (HighLogic.LoadedSceneIsFlight) {
				FlightDriver.SetPause (activate);
				if (activate) {
					InputLockManager.SetControlLock (ControlTypes.CAMERACONTROLS | ControlTypes.MAP, "Lock" + Quick.MOD);
					return;
				}
			} else if (HighLogic.LoadedSceneIsEditor) {
				if (activate) {
					EditorLogic.fetch.Lock(true, true, true, "EditorLock" + Quick.MOD);
					return;
				} else {
					EditorLogic.fetch.Unlock ("EditorLock" + Quick.MOD);
				}
			}
			if (activate) {
				InputLockManager.SetControlLock (Ctrl, "Lock" + Quick.MOD);
				return;
			} else {
				InputLockManager.RemoveControlLock ("Lock" + Quick.MOD);
			}
			if (InputLockManager.GetControlLock ("Lock" + Quick.MOD) != ControlTypes.None) {
				InputLockManager.RemoveControlLock ("Lock" + Quick.MOD);
			}
			if (InputLockManager.GetControlLock ("EditorLock" + Quick.MOD) != ControlTypes.None) {
				InputLockManager.RemoveControlLock ("EditorLock" + Quick.MOD);
			}
		}

		#if GUI
		internal static void Settings() {
			SettingsSwitch ();
			if (!WindowSettings) {
				QStockToolbar.Instance.Reset ();
				QuickExit.BlizzyToolbar.Reset ();
				QSettings.Instance.Save ();
			}
		}
		internal static void SettingsSwitch() {
			WindowSettings = !WindowSettings;
			QStockToolbar.Instance.Set (WindowSettings);
			Lock (WindowSettings, ControlTypes.All);
			//QuickExit.Instance.MODialog = null;
			QuickExit.Instance.popupDialog = null;
		}

		internal static void OnGUI() {
			if (WindowSettings) {
				GUI.skin = HighLogic.Skin;
				RefreshRect ();
				RectSettings = GUILayout.Window (1248597845, RectSettings, DrawSettings, Quick.MOD + Quick.VERSION, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
			}
		}

		private static void DrawSettings(int id) {
			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();
			bool _bool = GUILayout.Toggle (QSettings.Instance.StockToolBar, "Use the Stock ToolBar", GUILayout.Width(210));
			if (_bool != QSettings.Instance.StockToolBar) {
				QSettings.Instance.StockToolBar = _bool;
				RectSettings.height = 0;
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(5);
			if (QSettings.Instance.StockToolBar) {
				GUILayout.BeginHorizontal ();
				GUILayout.Space (30);
				QSettings.Instance.StockToolBar_inlast = GUILayout.Toggle (QSettings.Instance.StockToolBar_inlast, "Put QuickExit in Stock", GUILayout.Width (180));
				GUILayout.EndHorizontal ();
				GUILayout.Space (5);
			}
			if (QBlizzyToolbar.isAvailable) {
				GUILayout.BeginHorizontal();
				QSettings.Instance.BlizzyToolBar = GUILayout.Toggle (QSettings.Instance.BlizzyToolBar, "Use the Blizzy ToolBar", GUILayout.Width(210));
				GUILayout.EndHorizontal();
				GUILayout.Space(5);
			}
			GUILayout.BeginHorizontal();
			QSettings.Instance.AutomaticSave = GUILayout.Toggle (QSettings.Instance.AutomaticSave, "Automatic save before exit", GUILayout.Width(210));
			GUILayout.EndHorizontal();
			GUILayout.Space(5);
			GUILayout.BeginHorizontal();
			QSettings.Instance.CountDown = GUILayout.Toggle (QSettings.Instance.CountDown, "Count Down", GUILayout.Width(210));
			GUILayout.EndHorizontal();
			GUILayout.Space(5);
			GUILayout.BeginHorizontal();
			GUILayout.Label ("Key to exit: ", GUILayout.ExpandWidth(true));
			GUILayout.Space(5);
			QSettings.Instance.Key = GUILayout.TextField (QSettings.Instance.Key, GUILayout.Width (100));
			GUILayout.EndHorizontal();
			GUILayout.Space(5);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button ("Exit", GUILayout.Width(40), GUILayout.Height(30))) {
				Settings();
				QuickExit.Instance.Exit ();
			}
			GUILayout.Space(5);
			if (GUILayout.Button ("Close", GUILayout.ExpandWidth(true) ,GUILayout.Height(30))) {
				try {
					Input.GetKey(QSettings.Instance.Key);
				} catch {
					Quick.Log ("Wrong key: " + QSettings.Instance.Key);
					QSettings.Instance.Key = "f7";
				}
				Settings ();
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(5);
			GUILayout.EndVertical();
		}
		#endif
	}
}