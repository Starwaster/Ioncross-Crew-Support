using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using KSP.UI;
using KSP.UI.Screens;

namespace IoncrossKerbal
{
	// Consulted Codepoet's CLS for guidance on using the stock toolbar system
	// Consulted RealChute & MechJeb2 for button implementation
	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	public class IonToolbar : MonoBehaviour
	{
		#region Fields
		private Rect windowPosition;
		private GUIStyle windowStyle = null;
		private GUIStyle labelStyle = null;
		private GUIStyle windowStyleCenter = null;

		private GUISkin skins = HighLogic.Skin;
		private int id = Guid.NewGuid().GetHashCode();
		//private bool visible = false, showing = true;
		//private Rect window = new Rect(), button = new Rect();
		private Texture2D buttonTexture = new Texture2D(32, 32);
		private string IonVersionString = "";
		#endregion

		#region Properties
		private static Vector3 mousePos = Vector3.zero;
		private bool weLockedInputs = false;
		private GUIStyle _buttonStyle = null;
		private GUIStyle buttonStyle
		{
			get
			{
				if (_buttonStyle == null)
				{
					_buttonStyle = new GUIStyle(skins.button);
					_buttonStyle.onNormal = _buttonStyle.hover;
				}
				return _buttonStyle;
			}
		}
		#endregion

		private ApplicationLauncherButton IonToolbarButton = null;
		private bool visible = false;

		IonToolbar()
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
			IonVersionString = string.Format("{0}.{1}.{2}", fileVersionInfo.FileMajorPart, fileVersionInfo.FileMinorPart, fileVersionInfo.FileBuildPart);
			//Melificent.height /= 2;
			//Melificent.width /= 2;
		}

		void Awake()
		{
			// Set up the stock toolbar
			windowPosition = new Rect(0, 0, 360, 480);

			windowStyle = new GUIStyle(HighLogic.Skin.window);
			windowStyle.stretchHeight = true;
			windowStyleCenter = new GUIStyle(HighLogic.Skin.window);
			windowStyleCenter.alignment = TextAnchor.MiddleCenter;
			labelStyle = new GUIStyle(HighLogic.Skin.label);
			labelStyle.fixedHeight = labelStyle.lineHeight + 4f;

			GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
			GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);
			GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequestedForAppLauncher);
		}

		public void OnGUI()
		{
			GUI.skin = HighLogic.Skin;

			if (HighLogic.LoadedSceneIsEditor) PreventEditorClickthrough();
			if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneHasPlanetarium) PreventInFlightClickthrough();


			Draw();
		}

		void Draw()
		{
			if (visible)
			{
				//Set the GUI Skin
				//GUI.skin = HighLogic.Skin;

				windowPosition = GUILayout.Window(id, windowPosition, OnWindow, "Ioncross Settings " + IonVersionString, windowStyle, GUILayout.MinHeight(20), GUILayout.ExpandHeight(true));
			}
		}

		bool MouseIsOverWindow()
		{
			mousePos = Input.mousePosition;
			mousePos.y = Screen.height - mousePos.y;
			return windowPosition.Contains(mousePos);
		}

		void PreventEditorClickthrough()
		{
			bool mouseOverWindow = MouseIsOverWindow();
			if (visible && !weLockedInputs && mouseOverWindow && !Input.GetMouseButton(1))
			{
				EditorLogic.fetch.Lock(true, true, true, "IonMenuLock");
				weLockedInputs = true;
			}
			if (weLockedInputs && (!mouseOverWindow || !visible))
			{
				EditorLogic.fetch.Unlock("IonMenuLock");
				weLockedInputs = false;
			}
		}

		void PreventInFlightClickthrough()
		{
			bool mouseOverWindow = MouseIsOverWindow();
			if (visible && !weLockedInputs && mouseOverWindow && !Input.GetMouseButton(1))
			{
				//InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS | ControlTypes.MAP, "DREMenuLock");
				InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, "IonMenuLock");
				weLockedInputs = true;
			}
			if (weLockedInputs && (!mouseOverWindow || !visible))
			{
				InputLockManager.RemoveControlLock("IonMenuLock");
				weLockedInputs = false;
			}
		}

		void OnGUIAppLauncherReady()
		{
			if (ApplicationLauncher.Ready && IonToolbarButton == null)
			{
				ApplicationLauncher.AppScenes visibleInScenes =
					ApplicationLauncher.AppScenes.FLIGHT |
					ApplicationLauncher.AppScenes.MAPVIEW |
					ApplicationLauncher.AppScenes.SPACECENTER |
					ApplicationLauncher.AppScenes.TRACKSTATION;
				IonToolbarButton = ApplicationLauncher.Instance.AddModApplication(onAppLaunchToggleOn,
																					   onAppLaunchToggleOff,
																					   null,
																					   null,
																					   null,
																					   null,
																					   visibleInScenes,
																					   (Texture)GameDatabase.Instance.GetTexture("IoncrossCrewSupport/Assets/ion_icon_off", false));
			}
			else
				print("OnGUIAppLauncherReady fired but AppLauncher not ready or button already created!");
		}

		void OnGameSceneLoadRequestedForAppLauncher(GameScenes SceneToLoad)
		{
		}

		void OnGUIAppLauncherDestroyed()
		{
			print("onGUIAppLauncherDestroyed() called");
			if (IonToolbarButton != null)
			{
				ApplicationLauncher.Instance.RemoveModApplication(IonToolbarButton);
				IonToolbarButton = null;
			}
		}

		void onAppLaunchToggleOn()
		{
			print("onAppLaunchToggleOn() called");
			IonToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture("IoncrossCrewSupport/Assets/ion_icon_on", false));
			visible = true;
		}

		void onAppLaunchToggleOff()
		{
			print("onAppLaunchToggleOff() called");
			IonToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture("IoncrossCrewSupport/Assets/ion_icon_off", false));
			visible = false;
		}

		public void OnDestroy()
		{
			// Remove the stock toolbar button
			print("OnDestroy() called - destroying button");
			GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
			if (IonToolbarButton != null)
				ApplicationLauncher.Instance.RemoveModApplication(IonToolbarButton);
		}

		private void OnWindow(int windowID)
		{
			//GUILayout.BeginVertical ();
			//GUILayout.BeginHorizontal ();
			//GUILayout.Button(;
			//GUILayout.Label("Ioncross Crew Support: " 
			//GUILayout.Label ("Ioncross Stuff");
			IonLifeSupportScenario.Instance.isLifeSupportEnabled = GUILayout.Toggle(IonLifeSupportScenario.Instance.isLifeSupportEnabled, "Enable Ioncross Crew Support for this game save", skins.toggle);
			IonLifeSupportScenario.Instance.isThermalEnabled = GUILayout.Toggle(IonLifeSupportScenario.Instance.isThermalEnabled, "Enable Crew Pod Heating (Kerbals can die of overheating or freezing)", skins.toggle);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUI.DragWindow();
		}

		static void print(string msg)
		{
			MonoBehaviour.print("[IoncrossCrewSupport.IonToolBar] " + msg);
		}
	}
}

