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
using System.IO;
using System.Threading;
using System.Linq;
using UnityEngine;

namespace QuickExit {
	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	public class QuickExit : MonoBehaviour {
		public static string VERSION = "1.20";
		public static string MOD = "QuickExit";

		private static bool isdebug = true;

		private string StockToolBar_TexturePath = "QuickExit/Textures/StockToolBar";
		private string BlizzyToolBar_TexturePath = "QuickExit/Textures/BlizzyToolBar";
		private string File_settings = KSPUtil.ApplicationRootPath + "GameData/QuickExit/Config.txt";

		private static bool isConfig = false;

		private ApplicationLauncherButton StockToolBar_Button;
		private IButton BlizzyToolBar_Button;

		[KSPField(isPersistant = true)]
		private static MultiOptionDialog MODialog;

		private GUIStyle TextStyle;
		private GUIStyle TextStyleExitIn;
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
		private bool StockToolBar_inlast = false;
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

			TextStyle = new GUIStyle ();
			TextStyle.stretchWidth = true;
			TextStyle.stretchHeight = true;
			TextStyle.alignment = TextAnchor.MiddleCenter;
			TextStyle.fontStyle = FontStyle.Bold;
			TextStyle.normal.textColor = Color.white;

			TextStyleExitIn = new GUIStyle ();
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
				Log ("Game Saved");
			}
		}
		private void OnGameStateSave(ConfigNode configNode) {
			isInSave = true;
		}

		// GESTION DES TOOLBARS
		private void OnGUIApplicationLauncherReady() {
			if (StockToolBar && !StockToolBar_inlast) {
				StockToolBar_Init ();
			}
		}
		private void BlizzyToolBar_Init() {
			if (isBlizzyToolBar && BlizzyToolBar_Button == null) {
				BlizzyToolBar_Button = ToolbarManager.Instance.add(MOD, MOD);
				BlizzyToolBar_Button.TexturePath = BlizzyToolBar_TexturePath;
				BlizzyToolBar_Button.ToolTip = MOD;
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
				Texture2D _texture = GameDatabase.Instance.GetTexture (StockToolBar_TexturePath, false);
				if (!StockToolBar_inlast) {
					StockToolBar_Button = ApplicationLauncher.Instance.AddModApplication (Dialog, null, null, null, null, null, ApplicationLauncher.AppScenes.ALWAYS, _texture);
				} else {
					StockToolBar_Button = ApplicationLauncher.Instance.AddApplication (Dialog, null, null, null, null, null, _texture);
				}
			}
		}
		private void StockToolBar_Destroy() {
			if (StockToolBar_Button != null) {
				ApplicationLauncher.Instance.RemoveModApplication (StockToolBar_Button);
				ApplicationLauncher.Instance.RemoveApplication (StockToolBar_Button);
				StockToolBar_Button = null;
			}
		}

		// GESTION DU POPUP
		private void Dialog() {
			Clear();
			Log("Dialog");
			Lock ();
			DialogOption[] options = new DialogOption[3];
			options[0] = new DialogOption(string.Format("Oh noooo! ({0})", GameSettings.MODIFIER_KEY.primary.ToString()), Clear);
			options[1] = new DialogOption("Configurations!", Config);
			options [2] = new DialogOption (string.Format ("Exit, {2}! ({0} + {1})", GameSettings.MODIFIER_KEY.primary.ToString (), Key, (AutomaticSave && HighLogic.LoadedSceneIsGame ? "in " + i + "s" : "now")), Exit);
			MODialog = new MultiOptionDialog ("Are you sure you want to exit KSP?", windowTitle: "QuickExit", skin: AssetBase.GetGUISkin (ActiveGUI), options: options);
			PopupDialog.SpawnPopupDialog (MODialog, true, AssetBase.GetGUISkin (ActiveGUI));
		}
		private void Config() {
			Log("Config");
			isConfig = true;
		}
		private void Clear() {
			Log("Clear");
			UnLock ();
			if (StockToolBar && StockToolBar_Button != null && ApplicationLauncher.Ready) {
				if (StockToolBar_Button.State == RUIToggleButton.ButtonState.TRUE) {
					StockToolBar_Button.SetFalse ();
				}
			}
			isConfig = false;
			isExit = false;
			i = 5;
			MODialog = null;
		}

		// GESTION DE LA SORTIE DE KSP
		private void Exit() {
			Log("Exit");
			isExit = true;
			if (AutomaticSave && HighLogic.LoadedSceneIsGame) {
				if (CanSavegame) {
					Log ("Savegame");
					GamePersistence.SaveGame ("persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);
				} else {
					Log ("Can't Save");
					i = 10;
				}
				Start_WaitForExit ();
			} else {
				ExitNow();
			}
		}
		private static void ExitNow() {
			if (isInSave) {
				i = 5;
				CantQuitInSave ();
				return;
			}
			Log("ExitNow");
			Application.Quit ();
		}
		private static void CantQuitInSave() {
			Log("CantQuitInSave");
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
					Log ("Exit in " + i + "s.");
					if (i <= 0) {
						ExitNow ();
						break;
					}
					Thread.Sleep (1000);
					i--;
				}
			}
			finally {
				Log ("Thread ended");
			}
		}
		private void Lock() {
			if (HighLogic.LoadedSceneIsFlight) {
				FlightDriver.SetPause (true);
			}
			InputLockManager.SetControlLock(ControlTypes.All, MOD);
		}
		private void UnLock() {
			if (HighLogic.LoadedSceneIsFlight) {
				FlightDriver.SetPause (false);
			}
			InputLockManager.RemoveControlLock(MOD);
		}

		// GESTION DES RACCOURCIS ET DE LA TOOLBAR
		private void Update() {
			if (MODialog != null) {
				if (MODialog.Options.Length > 0) {
					if (GameSettings.MODIFIER_KEY.GetKeyDown ()) {
						if (MODialog.Options.Length > 0) {
							MODialog.Options [0].OptionSelected ();
						}
					}
				}
			}
			if (Input.GetKeyDown (Key)) {
				if (GameSettings.MODIFIER_KEY.GetKey ()) {
					if (!isExit) {
						Clear ();
						Lock ();
						Exit ();
					} else {
						Clear ();
					}
				} else if (!isConfig) {
					Dialog ();
				}
			} 
			if (StockToolBar_inlast) {
				if (StockToolBar && StockToolBar_Button == null && ApplicationLauncher.Ready && MessageSystem.Ready) {
					if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER) {
						if (ContractsApp.Instance == null) {
							return;
						}
					}
					StockToolBar_Init ();
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
					if (i > 0) {
						_label = "Exit in " + i + "s";
					} else {
						_label = "Exiting, bye ...";
					}
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
			if (StockToolBar) {
				GUILayout.BeginHorizontal ();
				GUILayout.Space (30);
				StockToolBar_inlast = GUILayout.Toggle (StockToolBar_inlast, "Put QuickExit in Stock", GUILayout.Width (180));
				GUILayout.EndHorizontal ();
				GUILayout.Space (5);
			}
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
				if (StockToolBar) {
					StockToolBar_Destroy ();
					StockToolBar_Init ();
				} else {
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
					Log ("Wrong key: " + Key);
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
			Log("Save");
		}
		public void Load() {
			if (File.Exists (File_settings)) {
				ConfigNode _temp = ConfigNode.Load (File_settings);
				if (_temp.HasValue ("AutomaticSave") && _temp.HasValue ("StockToolBar") && _temp.HasValue ("StockToolBar_inlast") && _temp.HasValue ("BlizzyToolBar") && _temp.HasValue ("ActiveGUI") && _temp.HasValue ("Key")) {
					ConfigNode.LoadObjectFromConfig (this, _temp);
					Log("Load");
					return;
				}
			}
			Save ();
		}


		// AFFICHAGE DES MESSAGES SUR LA CONSOLE
		private static void Log(string String) {
			if (isdebug) {
				Debug.Log (MOD + "(" + VERSION + "): " + String);
			}
		}
	}
}