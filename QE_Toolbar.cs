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
using UnityEngine;

namespace QuickExit {
	public class QToolbar {

		private static bool StockToolBar {
			get {
				return QSettings.Instance.StockToolBar;
			}
		}
		private static bool BlizzyToolBar {
			get {
				return QSettings.Instance.BlizzyToolBar;
			}
		}
		private static bool StockToolBar_ModApp {
			get {
				return !QSettings.Instance.StockToolBar_inlast;
			}
		}
		private ApplicationLauncher.AppScenes StockToolBar_AppScenes = ApplicationLauncher.AppScenes.ALWAYS;
		public static string StockToolBar_TexturePath = Quick.MOD + "/Textures/StockToolBar";
		public static string BlizzyToolBar_TexturePath = Quick.MOD + "/Textures/BlizzyToolBar";

		private void onTrue() { 
			QExit.Dialog ();
		}




		internal static QToolbar Instance {
			get;
			private set;
		}

		private Texture2D StockToolBar_Texture;

		private ApplicationLauncherButton StockToolBar_Button;

		private IButton BlizzyToolBar_Button;

		internal static bool isBlizzyToolBar {
			get {
				return ToolbarManager.ToolbarAvailable && ToolbarManager.Instance != null;
			}
		}

		internal static bool isStockToolBar {
			get {
				return HighLogic.LoadedSceneIsGame && ApplicationLauncher.Instance != null;
			}
		}

		internal static bool isModApp(ApplicationLauncherButton button) {
			bool _hidden;
			return ApplicationLauncher.Instance.Contains (button, out _hidden);
		}

		internal static void Awake() {
			QToolbar.Instance = new QToolbar ();
		}

		internal void Start() {
			StockToolBar_Texture = GameDatabase.Instance.GetTexture (StockToolBar_TexturePath, false);
			if (BlizzyToolBar && HighLogic.LoadedSceneIsGame) {
				BlizzyToolBar_Init ();
			}
		}

		internal void OnDestroy() {
			GameEvents.onGUIApplicationLauncherUnreadifying.Remove(StockToolBar_Destroy);
			StockToolBar_Destroy ();
			BlizzyToolBar_Destroy ();
			QToolbar.Instance = null;
		}

		internal static IEnumerator AddAppLauncherAfter () {
			if (QToolbar.Instance == null) {
				yield break;
			}
			if (!StockToolBar) {
				yield break;
			}
			while (!isStockToolBar) {
				yield return 0;
			}
			if (!StockToolBar_ModApp) {
				while (MessageSystem.Instance == null) {
					yield return 0;
				}
				while (MessageSystem.Instance.appLauncherButton == null) {
					yield return 0;
				}
				if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER) {
					while (ContractsApp.Instance == null) {
						yield return 0;
					}
					while (ContractsApp.Instance.appLauncherButton == null) {
						yield return 0;
					}
				}

				if (HighLogic.LoadedSceneIsFlight) {
					while (ResourceDisplay.Instance == null) {
						yield return 0;
					}
					while (ResourceDisplay.Instance.appLauncherButton == null) {
						yield return 0;
					}
					if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX) {
						while (CurrencyWidgetsApp.FindObjectOfType (typeof(CurrencyWidgetsApp)) == null) {
							yield return 0;
						}
						CurrencyWidgetsApp _currencyWidgetsApp = (CurrencyWidgetsApp)CurrencyWidgetsApp.FindObjectOfType (typeof(CurrencyWidgetsApp));
						while (_currencyWidgetsApp.appLauncherButton == null) {
							yield return 0;
						}
					}
				}
				if (HighLogic.LoadedSceneIsEditor) {
					while (EngineersReport.Ready && EngineersReport.Instance == null) {
						yield return 0;
					}
					while (EngineersReport.Instance.appLauncherButton == null) {
						yield return 0;
					}
				}
			}
			QToolbar.Instance.OnGUIApplicationLauncherReady ();
		}

		internal void OnGUIApplicationLauncherReady() {
			if (StockToolBar) {
				StockToolBar_Init ();
			}
		}

		private void BlizzyToolBar_Init() {
			if (!isBlizzyToolBar) {
				return;
			}
			if (BlizzyToolBar_Button == null) {
				BlizzyToolBar_Button = ToolbarManager.Instance.add(Quick.MOD, Quick.MOD);
				BlizzyToolBar_Button.TexturePath = BlizzyToolBar_TexturePath;
				BlizzyToolBar_Button.ToolTip = Quick.MOD;
				BlizzyToolBar_Button.OnClick += (e) => onTrue();
			}
		}

		private void BlizzyToolBar_Destroy() {
			if (!isBlizzyToolBar) {
				return;
			}
			if (BlizzyToolBar_Button != null) {
				BlizzyToolBar_Button.Destroy ();
				BlizzyToolBar_Button = null;
			}
		}

		private void StockToolBar_Init() {
			if (!isStockToolBar) {
				return;
			}
			if (StockToolBar_Button == null) {
				if (StockToolBar_ModApp) {
					StockToolBar_Button = ApplicationLauncher.Instance.AddModApplication (onTrue, null, null, null, null, null, StockToolBar_AppScenes, StockToolBar_Texture);
				} else {
					StockToolBar_Button = ApplicationLauncher.Instance.AddApplication (onTrue, null, null, null, null, null, StockToolBar_Texture);
					ApplicationLauncher.Instance.DisableMutuallyExclusive (StockToolBar_Button);
				}
				GameEvents.onGUIApplicationLauncherUnreadifying.Add(StockToolBar_Destroy);
			}
		}

		private void StockToolBar_Destroy() {
			if (!isStockToolBar) {
				return;
			}
			if (StockToolBar_Button != null) {
				ApplicationLauncher.Instance.RemoveModApplication (StockToolBar_Button);
				ApplicationLauncher.Instance.RemoveApplication (StockToolBar_Button);
				StockToolBar_Button = null;
			}
		}

		private void StockToolBar_Destroy(GameScenes gameScenes) {
			StockToolBar_Destroy ();
		}

		internal void StockToolBar_Set(bool SetTrue, bool force = false) {
			if (!isStockToolBar) {
				return;
			}
			if (StockToolBar_Button != null) {
				if (SetTrue) {
					if (StockToolBar_Button.State == RUIToggleButton.ButtonState.FALSE) {
						StockToolBar_Button.SetTrue (force);
					}
				} else {
					if (StockToolBar_Button.State == RUIToggleButton.ButtonState.TRUE) {
						StockToolBar_Button.SetFalse (force);
					}
				}
			}
		}

		internal void Reset() {
			if (StockToolBar_Button != null) {
				StockToolBar_Set (false);
				if (!StockToolBar || (StockToolBar && (StockToolBar_ModApp && !isModApp(StockToolBar_Button)) || (!StockToolBar_ModApp && isModApp(StockToolBar_Button)))) {
					StockToolBar_Destroy ();
				}
			}
			if (StockToolBar) {
				StockToolBar_Init ();
			}
			if (BlizzyToolBar) {
				BlizzyToolBar_Init ();
			} else {
				BlizzyToolBar_Destroy ();
			}
		}
	}
}