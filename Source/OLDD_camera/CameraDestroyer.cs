using OLDD_camera.Modules;
using System;
using UnityEngine;

namespace OLDD_camera
{
    /// <summary>
    /// Destroyer cameras
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Flight, true)]
    internal class CameraDestroyer : MonoBehaviour
	{
        /// <summary>
        /// Subscription events
        /// </summary>
        protected void Awake()
		{
			GameEvents.onPartDestroyed.Add(new EventData<Part>.OnEvent(PartCameraDeactivate));
			GameEvents.onVesselDestroy.Add(new EventData<Vessel>.OnEvent(VesselDestroy));
			GameEvents.onVesselChange.Add(new EventData<Vessel>.OnEvent(RemoveLines));
		}

		protected void OnDestroy()
		{
			GameEvents.onPartDestroyed.Remove(new EventData<Part>.OnEvent(PartCameraDeactivate));
			GameEvents.onVesselDestroy.Remove(new EventData<Vessel>.OnEvent(VesselDestroy));
			GameEvents.onVesselChange.Remove(new EventData<Vessel>.OnEvent(RemoveLines));
		}


        /// <summary>
        /// Destroys cameras on the ship
        /// </summary>
        private void VesselDestroy(Vessel vessel)
		{
			foreach (Part current in vessel.parts)
			{
				PartCameraDeactivate(current);
			}
		}

        /// <summary>
        /// Destroy camera on the part
        /// </summary>
        private void PartCameraDeactivate(Part part)
		{
			foreach (PartModule current in part.Modules)
			{
				ICamPart camPart = current as ICamPart;
				if ( camPart != null)
				{
					camPart.Deactivate();
				}
			}
		}

		private void RemoveLines(Vessel data)
		{
            Destroy(GameObject.Find("scanningRay"));
            Destroy(GameObject.Find("visibilityRay"));
        }
	}
}
