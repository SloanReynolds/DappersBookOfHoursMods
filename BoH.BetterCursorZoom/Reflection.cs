using System;
using System.Collections.Generic;
using System.Reflection;

namespace BoH.BetterCursorZoom {
	internal static class Reflection {
		private static Dictionary<string, FieldInfo> _privateFields = new Dictionary<string, FieldInfo>();
		private static Dictionary<string, MethodInfo> _privateMethods = new Dictionary<string, MethodInfo>();

		internal static T GetPrivateField<T>(string fieldName, object instance) {
			if (!_privateFields.ContainsKey(fieldName)) {
				_privateFields.Add(fieldName, instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance));
			}
			return (T)_privateFields[fieldName].GetValue(instance);
		}

		internal static void InvokePrivateMethodWithArgs(string methodName, object instance, params object[] args) {
			//Console.WriteLine("Invoking " + methodName);
			if (!_privateMethods.ContainsKey(methodName)) {
				_privateMethods.Add(methodName, instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance));
			}
			_privateMethods[methodName].Invoke(instance, args);
		}

		internal static T InvokePrivateMethodWithArgs<T>(string methodName, object instance, params object[] args) {
			//Console.WriteLine($"Invoking {methodName} (return as {typeof(T)})");
			if (!_privateMethods.ContainsKey(methodName)) {
				_privateMethods.Add(methodName, instance.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance));
			}
			var thing = _privateMethods[methodName].Invoke(instance, args);
			//Console.WriteLine(thing.GetType());
			return (T)_privateMethods[methodName].Invoke(instance, args);
		}

		internal static void SetPrivateField(string fieldName, object instance, object value) {
			if (!_privateFields.ContainsKey(fieldName)) {
				_privateFields.Add(fieldName, instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance));
			}
			_privateFields[fieldName].SetValue(instance, value);
		}
	}
}