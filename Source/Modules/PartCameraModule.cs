using OLDD_camera.Camera;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace OLDD_camera.Modules
{
	internal class PartCameraModule : PartModule, ICamPart
	{
		

		[KSPField(guiActive = true, guiActiveEditor = false, guiName = "Camera", isPersistant = true), UI_Toggle]
		public bool IsEnabled;

		[KSPField(guiActive = true, guiActiveEditor = true, guiName = "Camera powered ")]
		private string _isPowered;

		[KSPField(guiActive = true, guiActiveEditor = true, guiName = "Bullets ")]
		private string aboutHits = "4/4";

		[KSPField(isPersistant = true)]
		private int _currentHits = -1;

		[KSPField]
		public int windowSize = 300;

		[KSPField]
		public int allowedScanDistance = 1000;

		[KSPField]
		public string resourceScanning = "ElectricCharge.50";

		private readonly string cameraName = "CamExt";
		private readonly string rotatorZ = "Case";
		private readonly string rotatorY = "Tube";
		private readonly string zoommer = "Lenz";
		private readonly string cap = "Cap";
		private readonly string bulletName = "Sphere";
		private readonly float stepper = 1000f;
		private GameObject capObject;
		private GameObject camObject;
		private PartCamera camera;
		private Vector3 initialUpVector;

		[KSPField(isPersistant = true)]
		private bool isOnboard;

		[KSPField(isPersistant = true)]
		private bool isLookAtMe;

		[KSPField(isPersistant = true)]
		private bool isLookAtMeAutoZoom;

		[KSPField(isPersistant = true)]
		private bool isFollowMe;

		[KSPField(isPersistant = true)]
		private bool isTargetCam;

		[KSPField(isPersistant = true)]
		private float _isFollowMeOffsetX;

		[KSPField(isPersistant = true)]
		private float _isFollowMeOffsetY;

		[KSPField(isPersistant = true)]
		private float _isFollowMeOffsetZ;

		[KSPField(isPersistant = true)]
		private float _targetOffset = 100f;

		public override void OnStart(PartModule.StartState state)
		{
            if (camera != null)
                return;
			camera = new PartCamera(base.part, resourceScanning, bulletName, _currentHits, rotatorZ, rotatorY, zoommer, stepper, cameraName, allowedScanDistance, windowSize, isOnboard, isLookAtMe, isLookAtMeAutoZoom, isFollowMe, isTargetCam, _isFollowMeOffsetX, _isFollowMeOffsetY, _isFollowMeOffsetZ, _targetOffset, "Camera");
			capObject = GameObjectExtension.GetChild(base.part.gameObject, cap);
			camObject = GameObjectExtension.GetChild(base.part.gameObject, cameraName);
			initialUpVector = camObject.transform.up;
			camera.InitialCamRotation = (camera.CurrentCamRotation = camObject.transform.rotation);
			camera.InitialCamPosition = (camera.CurrentCamPosition = camObject.transform.position);
			camera.InitialCamLocalRotation = (camera.CurrentCamLocalRotation = camObject.transform.localRotation);
			camera.InitialCamLocalPosition = (camera.CurrentCamLocalPosition = camObject.transform.localPosition);
		}

		public override string GetInfo()
		{
			return "External camera for various purposes. Provides 'Onboard', 'Look at Me', 'Follow Me' and 'Target Cam' modes. Can received commands from other vessels in a short distance";
		}

		public void Update()
		{
			GetElectricState();
		}

		public override void OnUpdate()
		{
            if (camera == null)
                return;
			if ( DockCamToolbarButton.FCS && base.part.vessel != FlightGlobals.ActiveVessel && IsEnabled)
			{
				float num = Vector3.Distance(FlightGlobals.ActiveVessel.transform.position, base.part.vessel.transform.position);
				float load = base.vessel.vesselRanges.orbit.load;
				if ( (double)num > (double)load * 0.99)
				{
					ScreenMessages.PostScreenMessage("FORCED SHUTDOWN", 3f, 0);
					camera.IsButtonOff = true;
				}
			}
			if ( _isPowered == "FALSE")
			{
				if (IsEnabled)
				{
					ScreenMessages.PostScreenMessage("ELECTRICITY HAS BEEN DEPLETED", 3f, 0);
				}
				camera.IsButtonOff = true;
			}
			if (camera.IsButtonOff)
			{
				IsEnabled = false;
				camera.IsButtonOff = false;
			}
			if (camera.IsToZero)
			{
				camera.IsToZero = false;
				base.StartCoroutine(camera.ReturnCamToZero());
			}
			if (camera.IsWaitForRay)
			{
				camera.IsWaitForRay = false;
				base.StartCoroutine(camera.WaitForRay());
			}
			isOnboard = camera.IsOnboard;
			isLookAtMe = camera.IsLookAtMe;
			isLookAtMeAutoZoom = camera.IsLookAtMeAutoZoom;
			isFollowMe = camera.IsFollowMe;
			isTargetCam = camera.IsTargetCam;

			if (isFollowMe)
			{
				_isFollowMeOffsetX = camera.IsFollowMeOffsetX;
				_isFollowMeOffsetY = camera.IsFollowMeOffsetY;
				_isFollowMeOffsetZ = camera.IsFollowMeOffsetZ;
			}
			if (isTargetCam)
			{
				_targetOffset = camera.TargetOffset;
			}
			if (isOnboard)
			{
				Onboard();
			}
			if (isLookAtMe)
			{
				LookAtMe();
			}
			if (isFollowMe)
			{
				FollowMe();
			}
			if (isTargetCam)
			{
				TargetCam();
			}
			_currentHits = camera.Hits;
			aboutHits = _currentHits + "/4";

			if (IsEnabled)
			{
				Activate();
			}
			else
			{
				Deactivate();
			}
			if (camera.IsAuxiliaryWindowButtonPres)
			{
				base.StartCoroutine(camera.ResizeWindow());
			}
			if (camera.IsActive)
			{
				camera.Update();
			}

		}

		private void SetCurrentMode(bool a, bool b, bool c, bool d)
		{
			camera.IsOnboardEnabled = a;
			camera.IsLookAtMeEnabled = b;
			camera.IsFollowEnabled = c;
			camera.IsTargetCamEnabled = d;
		}

		private void Onboard()
		{
			SetCurrentMode(true, false, false, false);
		}

		private void LookAtMe()
		{
			SetCurrentMode(false, true, false, false);

			if (camera.IsLookAtMeAutoZoom)
			{
				float num = Vector3.Distance(camObject.transform.position, FlightGlobals.ActiveVessel.vesselTransform.position);
				if ( num < 50f)
				{
					camera.CurrentZoom = camera.MaxZoom;
				}
				if ( num > 50f && num < 100f)
				{
					camera.CurrentZoom = 23f;
				}
				if ( num > 100f && num < 200f)
				{
					camera.CurrentZoom = 13f;
				}
				if ( num > 200f && num < 400f)
				{
					camera.CurrentZoom = 3f;
				}
				if ( num > 400f)
				{
					camera.ZoomMultiplier = true;
				}
				if (camera.ZoomMultiplier)
				{
					if ( num > 400f && num < 800f)
					{
						camera.CurrentZoom = 23f;
					}
					if ( num > 800f && num < 1600f)
					{
						camera.CurrentZoom = 13f;
					}
					if ( num > 1600f && num < 3200f)
					{
						camera.CurrentZoom = 3f;
					}
				}
			}
			camObject.transform.LookAt(FlightGlobals.ActiveVessel.CoM, initialUpVector);
		}

		private void FollowMe()
		{
			if ( !camera.IsFollowEnabled)
			{
				SetCurrentMode(false, false, true, false);
				camera.CurrentCamTarget = FlightGlobals.ActiveVessel.vesselTransform;
				camera.CurrentCam = camObject.transform;
			}
			Vector3 vector = camera.CurrentCamTarget.right * _isFollowMeOffsetX + camera.CurrentCamTarget.up * _isFollowMeOffsetY + camera.CurrentCamTarget.forward * _isFollowMeOffsetZ;
			camera.CurrentCam.position = (camera.CurrentCamTarget.position + vector);
			camera.CurrentCam.LookAt(FlightGlobals.ActiveVessel.CoM, camera.CurrentCamTarget.up);
		}

		private void TargetCam()
		{
			ITargetable target = TargetHelper.Target;
			if ( target == null)
			{
				SetCurrentMode(true, false, false, false);
			}
			else
			{
				Vessel vessel = target.GetVessel();
				SetCurrentMode(false, false, false, true);
				Vector3 vector = vessel.transform.position - FlightGlobals.ActiveVessel.transform.position;
				vector.Normalize();
				camObject.transform.position = (vessel.CoM - vector * _targetOffset);
				Vector3 normalized = base.vessel.vesselTransform.up.normalized;
				camObject.transform.LookAt(vessel.transform, normalized);
			}
		}

		private void GetElectricState()
		{
			double num = 0.0;
			int id = PartResourceLibrary.Instance.GetDefinition("ElectricCharge").id;

			if (HighLogic.LoadedSceneIsEditor)
			{
				List<Part> parts = EditorLogic.fetch.ship.parts;
				foreach (Part current in parts)
				{
					foreach (PartResource current2 in current.Resources)
					{
						if ( current2.info.id == id)
						{
							num += current2.amount;
						}
					}
				}
			}
			if (HighLogic.LoadedSceneIsFlight)
			{
				double num2;
				base.part.GetConnectedResourceTotals(id, out num, out num2, true);
			}
			if ( num > 0.0)
			{
				_isPowered = (IsEnabled ? "ONLINE" : "TRUE");
			}
			else
			{
				_isPowered = "FALSE";
			}
		}

		public void Activate()
		{
            if (!HighLogic.LoadedSceneIsFlight)
                return;
			if (!camera.IsActive)
			{
				camera.Activate();
				base.StartCoroutine("CapRotator");
			}
		}

		public void Deactivate()
		{
            if (!HighLogic.LoadedSceneIsFlight)
                return;
            if (!camera.IsActive)
                return;
			camera.Deactivate();
			if ( !IsEnabled)
			{
				base.StartCoroutine("CapRotator");
			}
		}

		private IEnumerator CapRotator()
		{
            int step = camera.IsActive ? 5 : -5;
            for (int i = 0; i < 54; i++)
            {
                capObject.transform.Rotate(new Vector3(0, 1f, 0), step);
                yield return new WaitForSeconds(1f / 270);
            }
        }
	}
}
