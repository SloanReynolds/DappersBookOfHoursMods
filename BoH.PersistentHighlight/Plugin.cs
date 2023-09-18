using System.Linq.Expressions;
using BepInEx;
using BoH.PersistentHighlight.MonoBehaviours;
using HarmonyLib;
using UnityEngine;

namespace BoH.PersistentHighlight {
	[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	public class Plugin : BaseUnityPlugin {
		public static Plugin I = null;

		//public static Plugin I = null;
		private GameObject _dapperBox = null;
		public GameObject DapperBox => _dapperBox ?? _InitDapperBox();

		void Awake() {
			I = this;
			Harmony.CreateAndPatchAll(typeof(Patches.TokenPatcher), PluginInfo.PLUGIN_GUID);
		}

		private GameObject _InitDapperBox() {
			if (_dapperBox == null) {
				_dapperBox = GameObject.Find("DapperSingletonBox");
				if (_dapperBox == null) {
					_dapperBox = new GameObject("DapperSingletonBox");
				}
				if (_dapperBox.GetComponent<DapperHighlighter>() == null) {
					_dapperBox.AddComponent<DapperHighlighter>();
				}
			}

			return _dapperBox;
		}
	}
}