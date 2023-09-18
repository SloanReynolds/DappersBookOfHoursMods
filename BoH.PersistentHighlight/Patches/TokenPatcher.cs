using System;
using System.Collections.Generic;
using BoH.PersistentHighlight.MonoBehaviours;
using HarmonyLib;
using SecretHistories;
using SecretHistories.Abstract;
using SecretHistories.Entities;
using SecretHistories.Manifestations;
using SecretHistories.UI;
using SecretHistories.UI.Aspects;
using UnityEngine.EventSystems;

namespace BoH.PersistentHighlight.Patches {
	[HarmonyPatch]
	internal class TokenPatcher {
		private static DapperHighlighter _dapperHighlighter = null;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(AureateElementFrame), "OnPointerEnter")]
		static void Token_OnPointerEnter(AureateElementFrame __instance, PointerEventData eventData) {
			_InitHighlighter();

			//Console.WriteLine(__instance.gameObject.name + " ElementFrame OnPointerEnter");

			if (!eventData.dragging)
				_dapperHighlighter.SetCurrentToken(Reflection.GetPrivateField<Element>("_aspect",__instance));
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Token), "OnPointerExit")]
		static void Token_OnPointerExit(Token __instance, PointerEventData eventData) {
			_InitHighlighter();

			if (!eventData.dragging)
				_dapperHighlighter.SetCurrentToken(null);
		}

		//[HarmonyPrefix]
		//[HarmonyPatch(typeof(Meniscate), "AttentionPlsOnElementsWIthAspects")]
		//static void AttentionPlsOnElementsWIthAspects(Meniscate __instance, Dictionary<string, string> withAspects) {
		//	foreach (var item in withAspects) {
		//		Console.WriteLine($"{item.Key} => {item.Value}");
		//	}
		//}


		private static void _InitHighlighter() {
			if (_dapperHighlighter == null)
				_dapperHighlighter = Plugin.I.DapperBox.GetComponent<DapperHighlighter>();
		}

	}
}
