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
using UnityEngine;

namespace QuickExit {

	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	public class QuickExit : QExit {
		public static QuickExit Instance;
		#if GUI
		[KSPField(isPersistant = true)] internal static QBlizzyToolbar BlizzyToolbar;
		#endif
		private void Awake() {
			if (Instance != null) {
				Destroy (this);
			}
			Instance = this;
			#if GUI
			if (BlizzyToolbar == null) BlizzyToolbar = new QBlizzyToolbar ();
			#endif
			Init ();
			GameEvents.onGameStateSaved.Add (OnGameStateSaved);
			GameEvents.onGameStateSave.Add (OnGameStateSave);
		}

		private void Start() {
			QSettings.Instance.Load ();
			#if GUI
			BlizzyToolbar.Start ();
			#endif
		}

		private void OnDestroy() {
			#if GUI
			BlizzyToolbar.OnDestroy ();
			#endif
			GameEvents.onGameStateSaved.Remove (OnGameStateSaved);
			GameEvents.onGameStateSave.Remove (OnGameStateSave);
		}
	}
}