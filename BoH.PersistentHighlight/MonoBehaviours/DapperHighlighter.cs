using System;
using System.Collections.Generic;
using System.Linq;
using SecretHistories.Entities;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace BoH.PersistentHighlight.MonoBehaviours {
	internal class DapperHighlighter : MonoBehaviour {
		private Element _hoveredElement = null;
		private List<Element> _selectedElements = new List<Element>();
		private bool isPulsing = false;
		private int lastPulse = 0;

		internal void SetCurrentToken(Element instance) {
			_hoveredElement = instance;
		}

		void Update() {
			if (Keyboard.current[Key.K].wasPressedThisFrame) {
				if (_hoveredElement != null) {
					if (!_selectedElements.Contains(_hoveredElement)) {
						_selectedElements.Add(_hoveredElement);
						//Console.WriteLine(string.Join(", ", _selectedElements));
					}
				}
			} else if (Keyboard.current[Key.L].wasPressedThisFrame) {
				isPulsing = !isPulsing;
			} else if (Keyboard.current[Key.J].wasPressedThisFrame) {
				_selectedElements.Clear();
			}

			if (_selectedElements.Count == 0) {
				isPulsing = false;
			}

			if (isPulsing) {
				if (Time.frameCount - lastPulse > 80) {
					foreach (Token token in FindTokensWithAspectsInWorld(_selectedElements)) {
						token.AttentionPls();
					}
					lastPulse = Time.frameCount;
				}
				//Console.WriteLine(lastPulse);
			}
		}

		List<Token> FindTokensWithAspectsInWorld(List<Element> aspects) {
			var dict = new Dictionary<string, string>();
			foreach(var aspect in aspects) {
				dict.Add(aspect.Id, 1.ToString());
				//Console.WriteLine($"{aspect.Id} => {1.ToString()}");
			}
			return Watchman.Get<HornedAxe>().FindTokensWithAspectsInWorld(dict);
		}

		private static DapperLog _Log = new DapperLog();
		public static void DebugObject(GameObject go, string preface = "") {
			_Log.Write($"GameObject Dump: {go.name}     [Scene: {SceneManager.GetActiveScene().name}]");
			Transform parent = go.transform.parent;
			if (parent != null) {
				_Log.Write("=> Parents:");
				while (parent != null) {
					_LogObject(parent.gameObject, "=> ");

					parent = parent.parent;
				}
			}
			if (preface != "") {
				_Log.Write($"----{preface}----");
			}
			_RecurseLogObj(go);
			_Log.Flush();
		}
		private static void _RecurseLogObj(GameObject go, List<GameObject> filter = null, string depth = "") {
			//If there's a filter, and this current object is not in it, we stop.
			if (filter != null && !filter.Contains(go)) {
				return;
			}

			_LogObject(go, depth);

			foreach (Transform transform in go.transform) {
				_RecurseLogObj(transform.gameObject, filter, ">>" + depth);
			}
		}

		private static void _LogObject(GameObject go, string prefix = "") {
			_Log.Write("");

			_Log.Write($"{prefix}({(go.activeInHierarchy ? 'a' : 'i')}, {go.gameObject.scene.buildIndex}) {go.name} [{go.layer}, {go.tag}] @ {go.transform.position.x}, {go.transform.position.y} ({go.transform.localPosition.x}, {go.transform.localPosition.y})");

			string[] p = go.GetComponents<Component>()
				.Select(com => com.GetType().ToString())
				.ToArray();

			_Log.Write($"{prefix}  <{string.Join(", ", p)}>");
		}
	}
}
