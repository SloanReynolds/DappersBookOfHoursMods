using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace BoH.BetterCursorZoom {
	[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	public class Plugin : BaseUnityPlugin {
		//public static Plugin I = null;

		void Awake() {
			//I = this;
			Harmony.CreateAndPatchAll(typeof(Patches.ZoomPatcher), PluginInfo.PLUGIN_GUID);
		}
	}
}