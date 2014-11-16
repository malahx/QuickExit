/* 
QuickExit
Copyright 2014 Malah

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
using System.Threading;
using UnityEngine;

namespace QuickExit {
	[KSPAddon(KSPAddon.Startup.EditorAny | KSPAddon.Startup.TrackingStation | KSPAddon.Startup.Flight | KSPAddon.Startup.SpaceCentre, false)]
	public class QuickExit : MonoBehaviour {

		public static string VERSION = "1.00";
		public static string MOD = "QuickExit";

		private static bool isdebug = true;

		private Texture StockToolBar_Texture = (Texture)GameDatabase.Instance.GetTexture ("QuickExit/Textures/StockToolBar", false);
		private string BlizzyToolBar_TexturePath = "QuickExit/Textures/BlizzyToolBar";
		private string File_settings = KSPUtil.ApplicationRootPath + "GameData/QuickExit/PluginData/QuickExit/QuickExit.cfg";

		private static bool isConfig = false;

		private ApplicationLauncherButton StockToolBar_Button;
		private IButton BlizzyToolBar_Button;
		private GUIStyle TextStyle = new GUIStyle ();
		private GUIStyle TextStyleExitIn = new GUIStyle ();
		private static int i = 5;
		private Rect Window_Rect = new Rect();
		public static bool isExit = false;
		public static bool isInSave = false;
		private static Thread ThWaitForExit;
		// GameSkin doesn't work with popupdialog.
		private string[] GUIWhiteList = {
			"MainMenuSkin",
			"KSP window 1",
			"KSP window 2",
			"KSP window 3",
			"KSP window 5",
			"KSP window 7"
		};

		[Persistent]
		private bool AutomaticSave = false;
		[Persistent]
		private string Key = "f7";
		[Persistent]
		private bool StockToolBar = true;
		[Persistent]
		private bool BlizzyToolBar = true;
		[Persistent]
		private string ActiveGUI = HighLogic.Skin.name;

		private bool isBlizzyToolBar {
			get {
				return (ToolbarManager.ToolbarAvailable && ToolbarManager.Instance != null);
			}
		}
		private bool CanSavegame {
			get {
				bool _CanSavegame = false;
				if (HighLogic.LoadedSceneIsGame) {
					_CanSavegame = !isInSave;
					if (_CanSavegame) {
						string _savegame = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/persistent.sfs";
						if (System.IO.File.Exists (_savegame)) {
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
								if (FlightGlobals.ActiveVessel.IsClearToSave() == ClearToSaveStatus.CLEAR) {
									goto RETURN;
								}
							}
							_CanSavegame = false;
						}
					}
				}
				RETURN:
				return _CanSavegame;
			}
		}

		private void Awake() {
			Load ();
			GameEvents.onGameStateSaved.Add (OnGameStateSaved);
			GameEvents.onGameStateSave.Add (OnGameStateSave);
			GameEvents.onGUIApplicationLauncherReady.Add (OnGUIApplicationLauncherReady);
			if (BlizzyToolBar && HighLogic.LoadedSceneIsGame) {
				BlizzyToolBar_Init ();
			}

			TextStyle.stretchWidth = true;
			TextStyle.stretchHeight = true;
			TextStyle.alignment = TextAnchor.MiddleCenter;
			TextStyle.fontStyle = FontStyle.Bold;
			TextStyle.normal.textColor = Color.white;

			TextStyleExitIn.stretchWidth = true;
			TextStyleExitIn.stretchHeight = true;
			TextStyleExitIn.alignment = TextAnchor.MiddleCenter;
			TextStyleExitIn.fontSize = (Screen.height/20);
			TextStyleExitIn.fontStyle = FontStyle.Bold;
			TextStyleExitIn.normal.textColor = Color.red;
		}
		private void OnDestroy() {
			GameEvents.onGameStateSaved.Remove (OnGameStateSaved);
			GameEvents.onGameStateSave.Remove (OnGameStateSave);
			GameEvents.onGUIApplicationLauncherReady.Remove (OnGUIApplicationLauncherReady);
			StockToolBar_Destroy ();
			if (BlizzyToolBar && HighLogic.LoadedSceneIsGame) {
				BlizzyToolBar_Destroy ();
			}
		}


		// EVENEMENTS DIVERS
		private void OnGameStateSaved(Game game) {
			isInSave = false;
			if (isExit) {
				i = 5;
				myDebug ("Game Saved");
			}
		}
		private void OnGameStateSave(ConfigNode configNode) {
			isInSave = true;
		}


		// GESTION DES TOOLBARS
		private void OnGUIApplicationLauncherReady() {
			if (StockToolBar) {
				StockToolBar_Init ();
			}
		}
		private void BlizzyToolBar_Init() {
			if (isBlizzyToolBar && BlizzyToolBar_Button == null) {
				BlizzyToolBar_Button = ToolbarManager.Instance.add("QuickExit", "QuickExit");
				BlizzyToolBar_Button.TexturePath = BlizzyToolBar_TexturePath;
				BlizzyToolBar_Button.ToolTip = "QuickExit";
				BlizzyToolBar_Button.OnClick += (e) => Dialog();
			}
		}
		private void BlizzyToolBar_Destroy() {
			if (isBlizzyToolBar && BlizzyToolBar_Button != null) {
				BlizzyToolBar_Button.Destroy ();
			}
		}
		private void StockToolBar_Init() {
			if (StockToolBar_Button == null && ApplicationLauncher.Ready) {
				StockToolBar_Button = ApplicationLauncher.Instance.AddModApplication (Dialog, null, null, null, null, null, ApplicationLauncher.AppScenes.ALWAYS, StockToolBar_Texture);
			}
		}
		private void StockToolBar_Destroy() {
			if (StockToolBar_Button != null) {
				ApplicationLauncher.Instance.RemoveModApplication (StockToolBar_Button);
				StockToolBar_Button = null;
			}
		}


		// GESTION DU POPUP
		private void Dialog() {
			Clear();
			myDebug("Dialog");
			Lock ();
			DialogOption[] options = new DialogOption[3];
			options[0] = new DialogOption("Oh noooo!", Clear);
			options[1] = new DialogOption("Configurations!", Config);
			options[2] = new DialogOption("Exit, now!", Exit);
			MultiOptionDialog diag = new MultiOptionDialog ("Are you sure you want to exit KSP?", windowTitle: "QuickExit", skin: AssetBase.GetGUISkin (ActiveGUI), options: options);
			PopupDialog.SpawnPopupDialog (diag, true, AssetBase.GetGUISkin (ActiveGUI));
		}
		private void Config() {
			myDebug("Config");
			isConfig = true;
		}
		private void Clear() {
			myDebug("Clear");
			UnLock ();
			if (StockToolBar && StockToolBar_Button != null && ApplicationLauncher.Ready) {
				if (StockToolBar_Button.State == RUIToggleButton.ButtonState.TRUE) {
					StockToolBar_Button.SetFalse ();
				}
			}
			isConfig = false;
			isExit = false;
			i = 5;
		}
		private void Exit() {
			myDebug("Exit");
			isExit = true;
			if (AutomaticSave && HighLogic.LoadedSceneIsGame) {
				if (CanSavegame) {
					myDebug ("Savegame");
					GamePersistence.SaveGame ("persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);
				} else {
					myDebug ("Can't Save");
					i = 10;
				}
				Start_WaitForExit ();
			} else {
				ExitNow();
			}
		}
		private static void ExitNow() {
			i = 5;
			if (isInSave) {
				CantQuitInSave ();
				return;
			}
			myDebug("ExitNow");
			Application.Quit ();
		}
		private static void CantQuitInSave() {
			myDebug("CantQuitInSave");
			ScreenMessages.PostScreenMessage ("[" + MOD + "] Can't exit while a savegame is in progress.", 10, ScreenMessageStyle.LOWER_CENTER);
			isExit = true;
			Start_WaitForExit ();
		}
		private static void Start_WaitForExit() {
			if (ThWaitForExit != null) {
				if (!ThWaitForExit.IsAlive) {
					ThWaitForExit.Abort ();
				}
			}
			ThWaitForExit = new Thread (new ThreadStart (WaitForExit));
			ThWaitForExit.Start ();
		}
		private static void WaitForExit() {
			try {
				while (isExit) {
					myDebug ("Exit in " + i + "s.");
					if (i <= 0) {
						ExitNow ();
						break;
					}
					Thread.Sleep (1000);
					i--;
				}
			}
			finally {
				myDebug ("Thread ended");
			}
		}
		private void Lock() {
			if (HighLogic.LoadedSceneIsFlight) {
				FlightDriver.SetPause (true);
			}
			InputLockManager.SetControlLock(ControlTypes.All, "QuickExit");
		}
		private void UnLock() {
			if (HighLogic.LoadedSceneIsFlight) {
				FlightDriver.SetPause (false);
			}
			InputLockManager.RemoveControlLock("QuickExit");
		}

		// GESTION DE LA TOUCHE EXIT
		private void Update() {
			if (!isConfig) {
				if (Input.GetKeyDown (Key)) {
					Dialog ();
				}
			}
		}


		// AFFICHAGE DE L'INTERFACE
		private void OnGUI() {
			if (isConfig) {
				GUI.skin = AssetBase.GetGUISkin(ActiveGUI);
				Window_Rect = new Rect ((Screen.width - Window_Rect.width) / 2, (Screen.height - Window_Rect.height) / 2, Window_Rect.width, Window_Rect.height);
				Window_Rect = GUILayout.Window (1248597845, Window_Rect, DrawSettings, MOD + " v" + VERSION, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
			}
			if (isExit) {
				string _label;
				if (!isInSave) {
					_label = "Exit in " + i + "s";
				} else {
					_label = "Waiting the savegame ...";
				}
				if (!CanSavegame) {
					_label += Environment.NewLine + "Can't save!";
				}
				_label += Environment.NewLine + "Push on " + Key + " to abort the operation.";
				GUILayout.BeginArea (new Rect (0, 0, Screen.width, Screen.height), TextStyleExitIn);
				GUILayout.Label (_label, TextStyleExitIn);
				GUILayout.EndArea ();
			}
		}


		// INTERFACE DE PARAMETRAGE
		private void DrawSettings(int id) {
			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();
			StockToolBar = GUILayout.Toggle (StockToolBar, "Use the Stock ToolBar", GUILayout.Width(210));
			GUILayout.EndHorizontal();
			GUILayout.Space(5);
			if (isBlizzyToolBar) {
				GUILayout.BeginHorizontal();
				BlizzyToolBar = GUILayout.Toggle (BlizzyToolBar, "Use the Blizzy ToolBar", GUILayout.Width(210));
				GUILayout.EndHorizontal();
				GUILayout.Space(5);
			}
			GUILayout.BeginHorizontal();
			AutomaticSave = GUILayout.Toggle (AutomaticSave, "Automatic save before exit", GUILayout.Width(210));
			GUILayout.EndHorizontal();
			GUILayout.Space(5);
			GUILayout.BeginHorizontal();
			GUILayout.Label ("Key to exit: ", GUILayout.ExpandWidth(true));
			GUILayout.Space(5);
			Key = GUILayout.TextField (Key, GUILayout.Width (100));
			GUILayout.EndHorizontal();
			GUILayout.Space(5);
			GUILayout.BeginHorizontal();
			ChooseSkin ();
			GUILayout.EndHorizontal();
			GUILayout.Space(5);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button ("Exit", GUILayout.Width(40), GUILayout.Height(30))) {
				isConfig = false;
				Exit ();
			}
			GUILayout.Space(5);
			if (GUILayout.Button ("Close", GUILayout.ExpandWidth(true) ,GUILayout.Height(30))) {
				if (StockToolBar && StockToolBar_Button == null) {
					StockToolBar_Init ();
				}
				if (!StockToolBar && StockToolBar_Button != null) {
					StockToolBar_Destroy ();
				}
				if (BlizzyToolBar && BlizzyToolBar_Button == null) {
					BlizzyToolBar_Init ();
				}
				if (!BlizzyToolBar && BlizzyToolBar_Button != null) {
					BlizzyToolBar_Destroy ();
				}
				try {
					Input.GetKey(Key);
				} catch {
					myDebug ("Wrong key: " + Key);
					Key = "f7";
				}
				Save ();
				Clear ();
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(5);
			GUILayout.EndVertical();
		}


		// CHOIX DE L'INTERFACE
		private void ChooseSkin() {
			if (GUILayout.Button ("◄", GUILayout.Width(20), GUILayout.Height(25))) {
				int _i = Array.FindIndex (GUIWhiteList, item => item == ActiveGUI);
				_i--;
				if (_i < 0) {
					_i = GUIWhiteList.Length -1;
				}
				ActiveGUI = GUIWhiteList[_i];
				GUI.skin = AssetBase.GetGUISkin(ActiveGUI);
				Window_Rect = new Rect ((Screen.width - Window_Rect.width) / 2, (Screen.height - Window_Rect.height) / 2, Window_Rect.width, 0);
			}
			GUILayout.Label ("Skin: " + ActiveGUI, TextStyle, GUILayout.ExpandWidth(true), GUILayout.Height(30));
			if (GUILayout.Button ("►", GUILayout.Width (20), GUILayout.Height(25))) {
				int _i = Array.FindIndex (GUIWhiteList, item => item == ActiveGUI);
				_i++;
				if (_i >= GUIWhiteList.Length) {
					_i = 0;
				}
				ActiveGUI = GUIWhiteList[_i];
				GUI.skin = AssetBase.GetGUISkin(ActiveGUI);
				Window_Rect = new Rect ((Screen.width - Window_Rect.width) / 2, (Screen.height - Window_Rect.height) / 2, Window_Rect.width, 0);
			}
		}


		// GESTION DE LA CONFIGURATION
		public void Save() {
			ConfigNode _temp = ConfigNode.CreateConfigFromObject(this, new ConfigNode());
			_temp.Save(File_settings);
			myDebug("Save");
		}
		public void Load() {
			if (System.IO.File.Exists (File_settings)) {
				ConfigNode _temp = ConfigNode.Load (File_settings);
				ConfigNode.LoadObjectFromConfig (this, _temp);
				myDebug("Load");
			}
		}


		// AFFICHAGE DES MESSAGES SUR LA CONSOLE
		private static void myDebug(string String) {
			if (isdebug) {
				Debug.Log (MOD + "(" + VERSION + "): " + String);
			}
		}
	}
}