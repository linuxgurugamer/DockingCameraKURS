using KSP.IO;
using OLDD_camera.Camera;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace OLDD_camera
{
	[KSPAddon(KSPAddon.Startup.Flight, true)]
	public class DockCamToolbarButton : MonoBehaviour
	{
		private static IButton toolbarButton;
		private static PluginConfiguration config;
		private static Rect settingsWindowPosition;
		private static Rect lastWindowPosition;
		private static Rect toolbarWindowPosition;
		private const int WindowTextureWidth = 256;
		public static bool WindowVisiblityOn;
		private static bool blizzyToolbarAvailable;
		private static bool showWindow;
		private static bool shadersToUse0 = true;
		private static bool shadersToUse1;
		private static bool shadersToUse2;
		private static bool dist2500 = true;
		private static bool dist9999;
		public static bool FCS;

		private readonly int _modulePartCameraId = "PartCameraModule".GetHashCode();

		private static readonly VesselRanges DefaultRanges = PhysicsGlobals.Instance.VesselRangesDefault;

		private readonly VesselRanges.Situation _myRanges = new VesselRanges.Situation(10000f, 10000f, 2500f, 2500f);

		private static List<Vessel> _vesselsWithCamera = new List<Vessel>();

        public static DockCamToolbarButton dockCamToolbarButton;


	    private void DockCamClickHandler()
        {
            WindowVisiblityOn = !WindowVisiblityOn;
        }

        public void Awake()
		{
            dockCamToolbarButton = this;

            blizzyToolbarAvailable = ToolbarManager.ToolbarAvailable;

			if (blizzyToolbarAvailable)
			{
				toolbarButton = ToolbarManager.Instance.add("DockCamera", "dockCam");
				toolbarButton.TexturePath = "DockingCam/Icons/DockingCamIcon";
				toolbarButton.ToolTip = "Show/Hide Docking Camera";
				toolbarButton.Visibility = new GameScenesVisibility(new GameScenes[]
				{
                    GameScenes.FLIGHT
                });
				toolbarButton.Visible = true;
				toolbarButton.Enabled = true;
                toolbarButton.OnClick += e => DockCamClickHandler();

            }
		}

		public void Start()
		{
			LoadWindowData();
			settingsWindowPosition.y = (toolbarWindowPosition.yMax);
	
			if (HighLogic.LoadedSceneIsFlight)
			{
				GameEvents.onVesselCreate.Add(new EventData<Vessel>.OnEvent(NewVesselCreated));
				GameEvents.onVesselChange.Add(new EventData<Vessel>.OnEvent(NewVesselCreated));
				GameEvents.onVesselLoaded.Add(new EventData<Vessel>.OnEvent(NewVesselCreated));
			}
		}

		public void Update()
		{
			if ( GameSettings.MODIFIER_KEY.GetKey(false) && Input.GetKeyDown(KeyCode.Alpha1))
			{
				WindowVisiblityOn = !WindowVisiblityOn;
			}
			showWindow = (WindowVisiblityOn && HighLogic.LoadedSceneIsFlight && !FlightGlobals.ActiveVessel.isEVA && !MapView.MapIsEnabled);

			if (shadersToUse0)
			{
				BaseKspCamera.ShadersToUse = 0;
			}
			else
			{
				if (shadersToUse1)
				{
					BaseKspCamera.ShadersToUse = 1;
				}
				else
				{
					if (shadersToUse2)
					{
						BaseKspCamera.ShadersToUse = 2;
					}
				}
			}
		}

		private void OnGUI()
		{
			OnWindowOLDD();
		}

		private static void OnWindowOLDD()
		{
			if (!showWindow)
			{
				toolbarWindowPosition.width = (256f);
				int count = _vesselsWithCamera.Count;
				int num = 20 * count;
				toolbarWindowPosition.height = ((float)(140 + num + 10));
				toolbarWindowPosition = Util.ConstrainToScreen(GUI.Window(2222, toolbarWindowPosition, new GUI.WindowFunction(DrawOnWindowOLDD), "OLDD Camera Settings"), 100);
			}
		}

		public static void DrawOnWindowOLDD(int windowID)
		{
			float load = FlightGlobals.ActiveVessel.vesselRanges.landed.load;
			if ( GUI.Toggle(new Rect(20f, 20f, 44f, 20f), dist2500, "2250"))
			{
				dist2500 = true;
				dist9999 = false;
				if ( load > 3333f && dist2500)
				{
					GameEvents.onVesselChange.Fire(FlightGlobals.ActiveVessel);
				}
			}
			if ( GUI.Toggle(new Rect(80f, 20f, 44f, 20f), dist9999, "9999"))
			{
				dist9999 = true;
				dist2500 = false;
				if ( load < 3333f && dist9999)
				{
					GameEvents.onVesselChange.Fire(FlightGlobals.ActiveVessel);
				}
			}
			string text = "Unload at: " + FlightGlobals.ActiveVessel.vesselRanges.landed.load;
			GUI.Label(new Rect(140f, 20f, 100f, 20f), text, Styles.guiStyleLabelWhiteBold);
			if ( GUI.Toggle(new Rect(20f, 40f, 222f, 20f), shadersToUse0, "Shaders pack Full (7 choices)"))
			{
				BaseKspCamera.ShadersToUse = 0;
				shadersToUse0 = true;
				shadersToUse1 = false;
				shadersToUse2 = false;
				SaveWindowData();
			}
			if ( GUI.Toggle(new Rect(20f, 60f, 222f, 20f), shadersToUse1, "Shaders pack Noisy (2 choices)"))
			{
				BaseKspCamera.ShadersToUse = 1;
				shadersToUse0 = false;
				shadersToUse1 = true;
				shadersToUse2 = false;
				SaveWindowData();
			}
			if ( GUI.Toggle(new Rect(20f, 80f, 222f, 20f), shadersToUse2, "Shaders pack Standart (3 choices)"))
			{
				BaseKspCamera.ShadersToUse = 2;
				shadersToUse0 = false;
				shadersToUse1 = false;
				shadersToUse2 = true;
				SaveWindowData();
			}
			if ( FCS = GUI.Toggle(new Rect(20f, 100f, 222f, 20f), FCS, "Forced Camera Shutdown"))
			{
				SaveWindowData();
			}
			GUI.Label(new Rect(2f, 120f, 256f, 24f), "- Vessels with camera in range -", Styles.guiStyleGreenLabelBold);
			List<Vessel> vesselsWithCamera = _vesselsWithCamera;
			vesselsWithCamera.Remove(FlightGlobals.ActiveVessel);
			if ( vesselsWithCamera.Count > 0)
			{
				foreach (Vessel current in vesselsWithCamera)
				{
					int num = vesselsWithCamera.IndexOf(current) + 1;
					float num2 = Mathf.Round(Vector3.Distance(current.transform.position, FlightGlobals.ActiveVessel.transform.position));
					string text2 = current.RevealSituationString();
					string text3 = string.Format("{0}. {1} ({2:N} m) - {3} ", new object[]
					{
						num,
						current.vesselName,
						num2,
						text2
					});
					if ( num2 <= load)
					{
						GUI.Label(new Rect(20f, (float)(120 + 20 * num), 222f, 20f), text3, Styles.guiStyleGreenLabelStandart);
					}
					else
					{
						GUI.Label(new Rect(20f, (float)(120 + 20 * num), 222f, 20f), text3);
					}
				}
			}
			GUI.DragWindow();
			if ( toolbarWindowPosition.x != lastWindowPosition.x || toolbarWindowPosition.y != lastWindowPosition.y)
			{
				lastWindowPosition.x = (toolbarWindowPosition.x);
				lastWindowPosition.y = (toolbarWindowPosition.y);
				SaveWindowData();
			}
		}

		private void NewVesselCreated(Vessel data)
		{
			List<Vessel> vessels = FlightGlobals.Vessels;
			_vesselsWithCamera = GetVesselsWithCamera(vessels);
			if ( !dist2500)
			{
				foreach (Vessel current in _vesselsWithCamera)
				{
					current.vesselRanges.subOrbital = _myRanges;
					current.vesselRanges.landed = _myRanges;
					current.vesselRanges.escaping = _myRanges;
					current.vesselRanges.orbit = _myRanges;
					current.vesselRanges.prelaunch = _myRanges;
					current.vesselRanges.splashed = _myRanges;
				}
			}
			else
			{
				foreach (Vessel current2 in _vesselsWithCamera)
				{
					current2.vesselRanges.subOrbital = DefaultRanges.subOrbital;
					current2.vesselRanges.landed = DefaultRanges.landed;
					current2.vesselRanges.escaping = DefaultRanges.escaping;
					current2.vesselRanges.orbit = DefaultRanges.orbit;
					current2.vesselRanges.prelaunch = DefaultRanges.prelaunch;
					current2.vesselRanges.splashed = DefaultRanges.splashed;
				}
			}
		}

		public List<Vessel> GetVesselsWithCamera(List<Vessel> allVessels)
		{
			List<Vessel> list = new List<Vessel>();
			foreach (Vessel current in allVessels)
			{
				if ( current.Parts.Count > 0)
				{
                    if ( /* current.vesselType == null || */ current.vesselType == VesselType.Flag || current.vesselType == VesselType.Unknown)
                        return list;
					foreach (Part current2 in current.Parts)
					{
						if ( current2.Modules.Contains(_modulePartCameraId))
						{
                            if (list.Contains(current))
                                return list;
							list.Add(current);
						}
					}
				}
			}
			return list;
		}

		private static void SaveWindowData()
		{
			config.SetValue("toolbarWindowPosition", toolbarWindowPosition);
			config.SetValue("shadersToUse0", shadersToUse0);
			config.SetValue("shadersToUse1", shadersToUse1);
			config.SetValue("shadersToUse2", shadersToUse2);
			config.SetValue("FCS", FCS);
			config.save();
		}

		private static void LoadWindowData()
		{
			config = PluginConfiguration.CreateForType<DockCamToolbarButton>(null);
			config.load();
			Rect rect = default(Rect);
			toolbarWindowPosition = config.GetValue<Rect>("toolbarWindowPosition", rect);
			shadersToUse0 = config.GetValue<bool>("shadersToUse0", shadersToUse0);
			shadersToUse1 = config.GetValue<bool>("shadersToUse1", shadersToUse1);
			shadersToUse2 = config.GetValue<bool>("shadersToUse2", shadersToUse2);
			FCS = config.GetValue<bool>("FCS", FCS);
		}

		private void OnDestroy()
		{
			if ( toolbarButton != null)
			{
				toolbarButton.Destroy();
			}
			GameEvents.onVesselCreate.Remove(new EventData<Vessel>.OnEvent(NewVesselCreated));
			GameEvents.onVesselChange.Remove(new EventData<Vessel>.OnEvent(NewVesselCreated));
			GameEvents.onVesselLoaded.Remove(new EventData<Vessel>.OnEvent(NewVesselCreated));
		}
	}
}
