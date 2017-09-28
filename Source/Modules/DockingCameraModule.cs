using OLDD_camera.Camera;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace OLDD_camera.Modules
{
	internal class DockingCameraModule : PartModule, ICamPart
	{
		

		[KSPField(guiActive = true, guiActiveEditor = false, guiName = "Docking Camera", isPersistant = true), UI_Toggle]
		public bool IsEnabled;

		[KSPField(isPersistant = true)]
		public bool noise;

		[KSPField(isPersistant = true)]
		private bool _crossDPAI;

		[KSPField(isPersistant = true)]
		private bool _crossOLDD;

		private int _windowSize = 256;
		private DockingCamera camera;

		public override void OnStart(StartState state)
		{
            if (state == StartState.Editor || camera != null)
                return;
			if ( camera == null)
			{
				camera = new DockingCamera(base.part, noise, _crossDPAI, _crossOLDD, _windowSize, "DockCam");
			}
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
					camera.IsButtonOff = true;
				}
			}

			if (camera.IsButtonOff)
			{
				IsEnabled = false;
				camera.IsButtonOff = false;
			}

			if (IsEnabled)
			{
				Activate();
			}
			else
			{
				Deactivate();
			}
			noise = camera.Noise;
			_crossDPAI = camera.TargetCrossDPAI;
			_crossOLDD = camera.TargetCrossOLDD;

			if (camera.IsAuxiliaryWindowButtonPres)
			{
				base.StartCoroutine(camera.ResizeWindow());
			}

			if (camera.IsActive)
			{
				camera.Update();
			}
		}

		public void Activate()
		{
			if (!camera.IsActive)
			{
				camera.Activate();
				base.StartCoroutine("WhiteNoise");
			}
		}

		public void Deactivate()
		{
            if (!camera.IsActive)
                return;
			base.StopCoroutine("WhiteNoise");
			camera.Deactivate();
		}

		private IEnumerator WhiteNoise()
		{
            while (camera.IsActive)
            {
                camera.UpdateNoise();
                yield return new WaitForSeconds(.1f);
            }
        }
	}
}
