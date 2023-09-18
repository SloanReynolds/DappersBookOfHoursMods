using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using SecretHistories.Assets.Scripts.Application.UI.Settings;
using SecretHistories.Constants;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BoH.BetterCursorZoom.Patches {
	[HarmonyPatch]
	internal class ZoomPatcher {
		private const float NEW_FARTHEST = -4800f;
		private static Camera _cam;

		private const float ZOOM_SPEED_CLOSE = 2f;
		private const float ZOOM_SPEED_FAR = 20f;
		private const float ZOOM_SMOOTHING_DECAY = 1.12f; //Bigger = shorter Logarithmic smoothing; 3f is roughly 3 frames of smoothing (almost feels like none though), 1.12f is 28 frames- but only around 15 frames are truly noticeable.
		private const float ZOOM_STOP = 0.2f; //Zoom will stop when the abs(momentum) is less than this

		private static float _zoomMomentum = 0f;
		private static Vector2 _zoomTarget = Vector2.zero;
		
		[HarmonyPrefix]
		[HarmonyPatch(typeof(CamOperator), "Update")]
		static void CamOperator_Update(CamOperator __instance) {
			if (_zoomMomentum < -ZOOM_STOP || _zoomMomentum > ZOOM_STOP) {
				ZoomToCursor(__instance);
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(CamOperator), "Awake")]
		static void CamOperator_Awake(CamOperator __instance) {
			__instance.ZOOM_Z_FARTHEST = NEW_FARTHEST;
			_cam = __instance.GetComponent<Camera>();
			_cam.farClipPlane = NEW_FARTHEST * -1 + 600;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CamOperator), "WarpMouseCursor")]
		static bool CamOperator_WarpMouseCursor(CamOperator __instance) {
			//My mouse getting moved around is extremely jarring-- I will nope out of this one
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CamOperator), "OnZoomEvent")]
		static bool CamOperator_OnZoomEvent(CamOperator __instance, ZoomLevelEventArgs args) {
			if (args.AbsoluteTargetZoomLevel != ZoomLevel.Unspecified ||
				!Reflection.GetPrivateField<SettingWatcherBool>("_zoomToCursorWatcher", __instance).GetValue()) {
				//Make sure the zoom keys function as normal, and if the cursor zoom option is off, just use the original.
				return true;
			}

			if (args.CurrentZoomInput == 0) {
				//No reason to continue with this- Unity will send two events for each mousewheel "click". One for the amount per click, and one letting you know the click has stopped.
				return false;
			}

			_zoomMomentum += args.CurrentZoomInput;

			//Cache the last Zoomed Location so the camera doesn't wiggle during the momentum decay
			Vector2 screenPos = Mouse.current.position.ReadValue();

			var navLimits = Reflection.GetPrivateField<RectTransform>("navigationLimits", __instance);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(navLimits, screenPos, _cam, out var localPoint);
			_zoomTarget = localPoint;

			return false;
		}

		private static void ZoomToCursor(CamOperator instance) {
			var localPoint = _zoomTarget;

			float camHeight = _cam.transform.position.z;
			//We need more sensitive zooming as we get close, and less sensitive farther out- This should really be quadratic in nature, but Lerp is close enough.
			float zoomSpeed = Mathf.Lerp(ZOOM_SPEED_FAR, ZOOM_SPEED_CLOSE, (camHeight - instance.ZOOM_Z_FARTHEST) / (instance.ZOOM_Z_CLOSE - instance.ZOOM_Z_FARTHEST));

			float vDelta = _zoomMomentum / ZOOM_SMOOTHING_DECAY * zoomSpeed * -1;
			_zoomMomentum /= ZOOM_SMOOTHING_DECAY;
			//Make sure we can't zoom further than the zoom boundaries.
			vDelta = Mathf.Clamp(vDelta, instance.ZOOM_Z_FARTHEST - camHeight, instance.ZOOM_Z_CLOSE - camHeight);

			Vector2 hDistanceToTarget = new Vector2(_cam.transform.position.x, _cam.transform.position.y) - localPoint;
			Vector2 hDelta = hDistanceToTarget * (vDelta / camHeight);

			Vector3 newCamPosition = new Vector3(hDelta.x, hDelta.y, vDelta) + new Vector3(_cam.transform.localPosition.x, _cam.transform.localPosition.y, _cam.transform.position.z);

			instance.SetCurrentZoomHeight(newCamPosition.z);
			DoPointCameraAtNow(instance, newCamPosition);
			//The regular PointCameraAt routine is jittery as all heck with the old cursor zoom- I personally won't use any smoothing, but included the decaying momentum version so it fits thematically with the game's current systems
		}

		private static void DoPointCameraAtNow(CamOperator instance, Vector2 pointAt) {
			pointAt = Reflection.InvokePrivateMethodWithArgs<Vector3>("ClampToNavigationRect", instance, new Vector3(pointAt.x, pointAt.y, 0f));
			Reflection.InvokePrivateMethodWithArgs("SetCameraPositionXY", instance, pointAt);
			Reflection.SetPrivateField("smoothTargetPositionXY", instance, pointAt);
			Reflection.SetPrivateField("_forcedCameraMotion", instance, null);
		}
	}
}
