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
using System.Collections;
using System.IO;
using UnityEngine;

namespace QuickExit {
	public class QExit : Quick {

		private DateTime Date = DateTime.Now;
		internal PopupDialog popupDialog;
		private int i = 5;
		internal bool isExit = false;
		protected bool isInSave = false;
		protected GUIStyle TextStyleExitIn;

		protected bool CanDraw {
			get {
				return isExit && CanCount;
			}
		}

		protected bool CanCount {
			get {
				return (QSettings.Instance.AutomaticSave && HighLogic.LoadedSceneIsGame && !CanSavegame) || QSettings.Instance.CountDown;
			}
		}


		protected bool CanSavegame {
			get {
				if (!HighLogic.LoadedSceneIsGame) {
					return false;
				}
				bool _CanSavegame = false;
				_CanSavegame = !isInSave;
				if (_CanSavegame) {
					string _savegame = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/persistent.sfs";
					if (File.Exists (_savegame)) {
						FileInfo _info = new FileInfo (_savegame);
						_CanSavegame = !_info.IsReadOnly;
					}
				}

				if (_CanSavegame) {
					if (PauseMenu.canSaveAndExit != ClearToSaveStatus.CLEAR) {
						_CanSavegame = false;
					}
				}

				if (_CanSavegame) {
					if (HighLogic.LoadedSceneIsFlight) {
						if (FlightGlobals.ready) {
							if (FlightGlobals.ActiveVessel.IsClearToSave () == ClearToSaveStatus.CLEAR) {
								goto RETURN;
							}
						}
						_CanSavegame = false;
					}
				}
				RETURN:
				return _CanSavegame;
			}
		}

		protected void Init() {
			TextStyleExitIn = new GUIStyle ();
			TextStyleExitIn.stretchWidth = true;
			TextStyleExitIn.stretchHeight = true;
			TextStyleExitIn.alignment = TextAnchor.MiddleCenter;
			TextStyleExitIn.fontSize = (Screen.height/20);
			TextStyleExitIn.fontStyle = FontStyle.Bold;
			TextStyleExitIn.normal.textColor = Color.red;
		}

		protected void OnGameStateSaved(Game game) {
			isInSave = false;
			if (isExit) {
				Log ("Game Saved");
				if (!QSettings.Instance.CountDown) {
					ExitNow ();
				}
			}
		}

		protected void OnGameStateSave(ConfigNode configNode) {
			isInSave = true;
			if (isExit) {
				Log ("Save Game");
			}

		}

		protected void Update() {
			if (popupDialog != null) {
				if (GameSettings.MODIFIER_KEY.GetKeyDown ()) {
					Clear ();
				}
			}
			if (Input.GetKeyDown (QSettings.Instance.Key)) {
				if (GameSettings.MODIFIER_KEY.GetKey ()) {
					if (!isExit) {
						Exit ();
					} else {
						Clear ();
					}
					#if GUI
				} else if (!QGUI.WindowSettings) {
					#else
					} else {
					#endif
					if (popupDialog == null) {
						Dialog ();
					}
				}
			}
		}

		protected void OnGUI() {
			#if GUI
			QGUI.OnGUI ();
			#endif
			if (!CanDraw) {
				return;
			}
			string _label;
			if (!isInSave || !HighLogic.LoadedSceneIsGame) {
				if (i > 0) {
					_label = "Exit in " + i + "s";
					if ((DateTime.Now - Date).TotalSeconds >= 1) {
						Date = DateTime.Now;
						i--;
					}
				} else {
					_label = "Exiting, bye ...";
					ExitNow ();
				}
			} else {
				_label = "Waiting the savegame ...";
			}
			if (!CanSavegame && HighLogic.LoadedSceneIsGame) {
				_label += Environment.NewLine + "Can't save!";
			}
			_label += Environment.NewLine + "Push on " + QSettings.Instance.Key + " to abort the operation.";
			GUILayout.BeginArea (new Rect (0, 0, Screen.width, Screen.height), TextStyleExitIn);
			GUILayout.Label (_label, TextStyleExitIn);
			GUILayout.EndArea ();
		}

		public void Dialog() {
			Clear();
			QGUI.Lock (true, ControlTypes.All);
			#if GUI
			if (QStockToolbar.Instance != null) {
				QStockToolbar.Instance.Set (true);
			}
			DialogOption[] options = new DialogOption[3];
			#else
			DialogOption[] options = new DialogOption[2];
			#endif
			options[0] = new DialogOption(string.Format("Oh noooo! ({0})", GameSettings.MODIFIER_KEY.primary.ToString()), Clear);
			#if GUI
			options [1] = new DialogOption ("Configurations!", QGUI.Settings);
			options [2] = new DialogOption (string.Format ("Exit, {2}! ({0} + {1})", GameSettings.MODIFIER_KEY.primary.ToString (), QSettings.Instance.Key, (CanCount ? "in " + (CanSavegame || !HighLogic.LoadedSceneIsGame ? 5 : 10) + "s" : "now")), Exit);
			#else
			options [1] = new DialogOption (string.Format ("Exit, {2}! ({0} + {1})", GameSettings.MODIFIER_KEY.primary.ToString (), QSettings.Instance.Key, (CanCount ? "in " + (CanSavegame || !HighLogic.LoadedSceneIsGame ? 5 : 10) + "s" : "now")), Exit);
			#endif
			MultiOptionDialog _MODialog = new MultiOptionDialog ("Are you sure you want to exit KSP?", windowTitle: "QuickExit", skin: HighLogic.Skin, options: options);
			popupDialog = PopupDialog.SpawnPopupDialog (_MODialog, true, HighLogic.Skin);
			Log("Dialog");
		}

		private void Clear() {
			QGUI.Lock (false, ControlTypes.All);
			if (popupDialog != null) {
				popupDialog.Dismiss ();
				popupDialog = null;
			}
			isExit = false;
			i = 5;
			#if GUI
			if (QGUI.WindowSettings) {
				QGUI.Settings ();
			}
			if (QStockToolbar.Instance != null) {
				QStockToolbar.Instance.Set (false);
			}
			#endif
			Log ("Clear");
		}

		public void Exit() {
			Clear ();
			Date = DateTime.Now;
			isExit = true;
			QGUI.Lock (true, ControlTypes.All);
			if (QSettings.Instance.AutomaticSave && HighLogic.LoadedSceneIsGame) {
				if (CanSavegame) {
					GamePersistence.SaveGame ("persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);
				} else {
					i = 10;
					Log("Can't Save game");
				}
				return;
			}
			if (!QSettings.Instance.CountDown) {
				ExitNow ();
				return;
			}
			Log ("Exit");
		}

		protected void ExitNow() {
			if (isInSave) {
				CantQuitInSave ();
				return;
			}
			Application.Quit ();
			Log("Exit Now!");
		}

		protected void CantQuitInSave() {
			ScreenMessages.PostScreenMessage ("[" + MOD + "] Can't exit while a savegame is in progress.", 10, ScreenMessageStyle.LOWER_CENTER);
			isExit = true;
			i = 5;
			Log("Can't quit in save");
		}
	}
}