using System;
using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace OLDD_camera.Camera
{
    internal class PartCamera : BaseKspCamera
    {

        private static HashSet<int> _usedId = new HashSet<int>();
        private const int ButtonSize = 25;

        private LineRenderer scanningRay;
        private LineRenderer visibilityRay;
        private readonly GameObject rotatorZ;
        private readonly GameObject rotatorY;
        private readonly GameObject zoommer;
        private readonly GameObject camObject;

        private readonly float stepper;
        private readonly string bulletName;
        private float rotateZbuffer;
        private float rotateYbuffer;
        private float zoomBuffer;
        private float lastZoom;
        private float simplifiedRotateZBuffer;
        private float rotateStep;
        private int id;
        private readonly int allowedScanDistance;
        private readonly int resourceUsage;
        private readonly string resourceName;
        private bool isRayEnabled;
        private bool isUpsideDown;
        private bool isScienceActivate;
        private bool isVisibilityRay;

        public int Hits = 4;
        private string cameraMode;

        public bool IsWaitForRay;
        public bool IsToZero;
        internal bool IsOnboard;
        internal bool IsOnboardEnabled;
        internal bool IsLookAtMe;
        internal bool IsLookAtMeEnabled;
        internal bool IsLookAtMeAutoZoom;
        internal bool IsFollowMe;
        internal bool IsFollowEnabled;
        internal bool IsTargetCam;
        internal bool IsTargetCamEnabled;
        internal bool IsOutOfRange;

        internal float IsFollowMeOffsetX;
        internal float IsFollowMeOffsetY;
        internal float IsFollowMeOffsetZ;
        internal float TargetOffset;

        internal Transform CurrentCamTarget;
        internal Transform CurrentCam;

        internal Quaternion InitialCamRotation;
        internal Vector3 InitialCamPosition;
        internal Quaternion InitialCamLocalRotation;
        internal Vector3 InitialCamLocalPosition;

        internal Quaternion CurrentCamRotation;
        internal Vector3 CurrentCamPosition;
        internal Quaternion CurrentCamLocalRotation;
        internal Vector3 CurrentCamLocalPosition;

        public float RealZoom
        {
            get
            {
                return ZoomMultiplier ? (CurrentZoom / (float)MinZoomMultiplier) : CurrentZoom;
            }
            set
            {
                CurrentZoom = value;
            }
        }

        public PartCamera(Part thisPart, string resourceScanning, string bulletName, int hits, string rotatorZ, string rotatorY, string zoommer, float stepper, string cameraName, int allowedScanDistance, int windowSize, bool isOnboard, bool isLookAtMe, bool isLookAtMeAutoZoom, bool isFollowMe, bool isTargetCam, float isFollowMeOffsetX, float isFollowMeOffsetY, float isFollowMeOffsetZ, float targetOffset, string windowLabel = "Camera") : base(thisPart, (float)windowSize, windowLabel)
        {
            List<string> list = resourceScanning.Split(new char[]
            {
                '.'
            }).ToList<string>();
            resourceName = list[0];
            resourceUsage = int.Parse(list[1]);
            this.bulletName = bulletName;
            this.rotatorZ = GameObjectExtension.GetChild(partGameObject.gameObject, rotatorZ);
            this.rotatorY = GameObjectExtension.GetChild(partGameObject.gameObject, rotatorY);
            this.zoommer = GameObjectExtension.GetChild(partGameObject.gameObject, zoommer);
            camObject = GameObjectExtension.GetChild(partGameObject.gameObject, cameraName);
            this.stepper = stepper;
            this.allowedScanDistance = allowedScanDistance;
            lastZoom = CurrentZoom;
            IsOnboard = isOnboard;
            IsLookAtMe = isLookAtMe;
            IsFollowMe = isFollowMe;
            IsLookAtMeAutoZoom = isLookAtMeAutoZoom;
            IsTargetCam = isTargetCam;
            IsFollowMeOffsetX = isFollowMeOffsetX;
            IsFollowMeOffsetY = isFollowMeOffsetY;
            IsFollowMeOffsetZ = isFollowMeOffsetZ;
            TargetOffset = targetOffset;
            GameEvents.onGameSceneLoadRequested.Add(new EventData<GameScenes>.OnEvent(LevelWasLoaded));
            GetCurrentBullets(bulletName, hits);
        }

        private void LevelWasLoaded(GameScenes data)
        {
            PartCamera._usedId = new HashSet<int>();
        }

        ~PartCamera()
        {
            GameEvents.onGameSceneLoadRequested.Remove(new EventData<GameScenes>.OnEvent(LevelWasLoaded));
        }

        private void GetCurrentBullets(string bulletName, int hits)
        {
            if (hits == -1)
            {
                Hits = 0;
                while (true)
                {
                    GameObject child = GameObjectExtension.GetChild(partGameObject, string.Format("{0}{1:000}", bulletName, Hits + 1));
                    if (child == null)
                    {
                        break;
                    }
                    Hits++;
                }
            }
            else
            {
                Hits = hits;
                int num = Hits + 1;
                while (true)
                {
                    GameObject child2 = GameObjectExtension.GetChild(partGameObject, string.Format("{0}{1:000}", bulletName, num));
                    if (child2 == null)
                    {
                        break;
                    }
                    GameObject.Destroy(child2);
                    num++;
                }
            }
        }

        public override void Activate()
        {
            base.Activate();
            SetFreeId();
            windowPosition.x = (windowPosition.width * (float)(id - 1));
            windowPosition.y = (64f);
        }

        public override void Deactivate()
        {
            base.Deactivate();
            PartCamera._usedId.Remove(id);
        }

        private void SetFreeId()
        {
            for (int i = 1; i < 8; i++)
            {
                if (!PartCamera._usedId.Contains(i))
                {
                    id = i;
                    WindowLabel = SubWindowLabel + " " + id;
                    PartCamera._usedId.Add(i);
                    break;
                }
            }
        }

        protected override void ExtendedDrawWindowL1()
        {
            if (!IsOrbital)
            {
                SetRotationStep();
                float num = windowPosition.width - 90f;

                if (IsAuxiliaryWindowOpen)
                {
                    if (IsOnboard)
                    {
                        DrawButtonsBlock(num);
                    }
                    else
                    {
                        DrawModeDataBlock(num);
                    }
                    DrawModeSelector(num);
                    ZoomMultiplier = GUI.Toggle(new Rect(num, 112f, 80f, 20f), ZoomMultiplier, " x 24");

                    if (IsOnboard)
                    {
                        GUI.Label(new Rect(num, 148f, 80f, 20f), string.Format("rotateZ: {0:F0}°", simplifiedRotateZBuffer));
                        GUI.Label(new Rect(num, 164f, 80f, 20f), string.Format("rotateY: {0:F0}°", rotateYbuffer));
                    }
                    if (GUI.Button(new Rect(num, 186f, 80f, 25f), "PHOTO"))
                    {
                        renderTexture.SavePng(part.vessel.vesselName);
                    }
                }
                if ((IsOnboard || IsLookAtMe) && FlightGlobals.ActiveVessel == part.vessel)
                {
                    isVisibilityRay = GUI.Toggle(new Rect(num - 2f, 215f, 80f, 20f), isVisibilityRay, "Target Ray");
                }
                GUI.Label(new Rect(num, 312f, 80f, 20f), string.Format("Bullets: {0:F0}", Hits), Styles.guiStyleLabelWhiteBold);
                base.ExtendedDrawWindowL1();
            }
        }

        protected override void ExtendedDrawWindowL2()
        {
            if (IsTargetCam && IsOutOfRange)
            {
                if (Event.current.type.Equals(7))
                {
                    Graphics.DrawTexture(texturePosition, textureNoSignal[TextureNoSignalId]);
                }
                GUI.Label(new Rect(texturePosition.xMin + (float)(32 * WindowSizeCoef), texturePosition.yMin + (float)(32 * WindowSizeCoef), 160f, 160f), "TARGET \n IS \n OUT OF RANGE", Styles.guiStyleRedLabelBoldLarge);
            }
            base.ExtendedDrawWindowL2();
        }

        protected override void ExtendedDrawWindowL3()
        {
            string text = string.Concat(new object[]
            {
                "Mode: ",
                cameraMode,
                " ( x ",
                CalculatedZoom,
                " )"
            });
            GUI.Label(new Rect(texturePosition.xMin + (float)(44 * WindowSizeCoef), texturePosition.yMax - 12f, 160f, 20f), text, Styles.guiStyleGreenLabelSmall);
            base.ExtendedDrawWindowL3();
        }

        private void DrawButtonsBlock(float widthOffset)
        {
            if (GUI.Button(new Rect(widthOffset, 36f, 25f, 25f), "↻"))
            {
                camObject.transform.Rotate(new Vector3(0f, 0f, 180f));
                isUpsideDown = !isUpsideDown;
            }
            if (GUI.RepeatButton(new Rect(widthOffset + 25f, 36f, 25f, 25f), "↑"))
            {
                if (!isUpsideDown)
                {
                    if (rotateYbuffer < 180f)
                        RotateY += rotateStep;
                }
                else
                {
                    if (rotateYbuffer > 0)
                        RotateY -= rotateStep;
                }

            }
            if (GUI.Button(new Rect(widthOffset + 50f, 36f, 25f, 25f), "⦿"))
            {
                if (part.vessel.Equals(FlightGlobals.ActiveVessel))
                {
                    if (!TargetHelper.IsTargetSelect)
                    {
                        ScreenMessages.PostScreenMessage("No target for scanning", 3f, 0);
                    }
                    else
                    {
                        if (Hits <= 0)
                        {
                            ScreenMessages.PostScreenMessage("Bullets depleted", 3f, 0);
                        }
                        else
                        {
                            int id = PartResourceLibrary.Instance.GetDefinition(resourceName).id;
                            double num;
                            double num2;
                            part.GetConnectedResourceTotals(id, out num, out num2, true);
                            if (num > (double)resourceUsage)
                            {
                                part.RequestResource(id, (float)resourceUsage);
                                GameObject child = GameObjectExtension.GetChild(partGameObject, string.Format("{0}{1:000}", bulletName, Hits));
                                GameObject.Destroy(child);
                                Hits--;
                                isRayEnabled = true;
                                IsWaitForRay = true;
                                isScienceActivate = false;
                            }
                            else
                            {
                                ScreenMessages.PostScreenMessage("Not enough electricity for scan", 3f, 0);
                            }
                        }
                    }
                }
                else
                {
                    ScreenMessages.PostScreenMessage("Camera not on active vessel", 3f, 0);
                }
            }
            if (GUI.RepeatButton(new Rect(widthOffset, 61f, 25f, 25f), "←"))
            {
                if (!isUpsideDown)
                    RotateZ -= rotateStep;
                else
                    RotateZ += rotateStep;
            }
            if (GUI.Button(new Rect(widthOffset + 25f, 61f, 25f, 25f), "o"))
            {
                IsToZero = true;
            }
            if (GUI.RepeatButton(new Rect(widthOffset + 50f, 61f, 25f, 25f), "→"))
            {
                if (!isUpsideDown)
                    RotateZ += rotateStep;
                else
                    RotateZ -= rotateStep;
            }
            if (GUI.RepeatButton(new Rect(widthOffset, 86f, 25f, 25f), "-"))
            {
                CurrentZoom += 0.5f;
                if (CurrentZoom > MaxZoom)
                {
                    CurrentZoom = MaxZoom;
                }
            }
            if (GUI.RepeatButton(new Rect(widthOffset + 25f, 86f, 25f, 25f), "↓"))
            {
                if (!isUpsideDown)
                {
                    if (rotateYbuffer > 0f)
                        RotateY -= rotateStep;
                }
                else
                {
                    if (rotateYbuffer < 180f)
                        RotateY += rotateStep;
                }

            }
            if (GUI.RepeatButton(new Rect(widthOffset + 50f, 86f, 25f, 25f), "+"))
            {
                CurrentZoom -= 0.5f;
                if (CurrentZoom < MinZoom)
                {
                    CurrentZoom = MinZoom;
                }
            }
        }

        private void DrawModeDataBlock(float widthOffset)
        {
            if (IsLookAtMe)
            {
                GUI.Box(new Rect(widthOffset - 2f, 36f, 86f, 76f), "Look At Me");
                GUI.Label(new Rect(widthOffset + 2f, 55f, 84f, 44f), "Focus on:\n" + FlightGlobals.ActiveVessel.vesselName, Styles.guiStyleGreenLabelStandart);
                IsLookAtMeAutoZoom = GUI.Toggle(new Rect(widthOffset, 90f, 86f, 20f), IsLookAtMeAutoZoom, "AutoZoom");
            }
            if (IsFollowMe)
            {
                GUI.Box(new Rect(widthOffset - 2f, 36f, 86f, 76f), "Offset X,Y,Z");
                IsFollowMeOffsetX = GUI.HorizontalSlider(new Rect(widthOffset + 1f, 58f, 80f, 10f), IsFollowMeOffsetX, -80f, 80f);
                IsFollowMeOffsetY = GUI.HorizontalSlider(new Rect(widthOffset + 1f, 74f, 80f, 10f), IsFollowMeOffsetY, -80f, 80f);
                IsFollowMeOffsetZ = GUI.HorizontalSlider(new Rect(widthOffset + 1f, 90f, 80f, 10f), IsFollowMeOffsetZ, -80f, 80f);
            }
            if (IsTargetCam)
            {
                GUI.Box(new Rect(widthOffset - 2f, 36f, 86f, 76f), "Target Data");
                ITargetable target = TargetHelper.Target;
                if (target != null)
                {
                    GUI.Label(new Rect(widthOffset + 2f, 55f, 84f, 44f), "Focus on:\n" + target.GetVessel().GetName(), Styles.guiStyleGreenLabelStandart);
                    GUI.Label(new Rect(widthOffset + 4f, 152f, 80f, 22f), "Target Offset", Styles.guiStyleLabelWhiteBold);
                    TargetOffset = GUI.HorizontalSlider(new Rect(widthOffset + 2f, 170f, 80f, 25f), TargetOffset, 8f, 128f);
                }
                else
                {
                    GUI.Label(new Rect(widthOffset + 4f, 60f, 84f, 44f), "NO TARGET\nSELECTED", Styles.guiStyleRedLabelBold);
                }
            }
        }

        private void DrawModeSelector(float widthOffset)
        {
            GUI.Box(new Rect(widthOffset - 2f, 256f, 86f, 56f), "");
            if (part.vessel != FlightGlobals.ActiveVessel)
            {
                if (IsLookAtMe = GUI.Toggle(new Rect(widthOffset - 2f, 256f, 84f, 20f), IsLookAtMe, "Look at Me"))
                {
                    IsOnboard = (IsFollowMe = (IsTargetCam = false));
                    string cameraMode = this.cameraMode;
                    cameraMode = "Look at Me";
                    IsFollowMeOffsetX = (IsFollowMeOffsetY = (IsFollowMeOffsetZ = 0f));
                    if (!IsLookAtMeEnabled)
                    {
                        CameraPositioning(cameraMode);
                    }
                }
            }
            if (IsFollowMe = GUI.Toggle(new Rect(widthOffset - 2f, 274f, 84f, 20f), IsFollowMe, "Follow Me"))
            {
                IsOnboard = (IsLookAtMe = (IsTargetCam = false));
                string cameraMode2 = cameraMode;
                cameraMode = "Follow Me";
                if (!IsFollowEnabled)
                {
                    CameraPositioning(cameraMode2);
                }
            }
            if (part.vessel == FlightGlobals.ActiveVessel)
            {
                if (IsTargetCam = GUI.Toggle(new Rect(widthOffset - 2f, 292f, 84f, 20f), IsTargetCam, "Target Cam"))
                {
                    IsOnboard = (IsLookAtMe = (IsFollowMe = false));
                    string cameraMode3 = cameraMode;
                    cameraMode = "Target Cam";

                    if (IsTargetCamEnabled)
                    {
                        ITargetable target = TargetHelper.Target;
                        if (target != null)
                        {
                            Vessel vessel = target.GetVessel();
                            float num = Mathf.Round(Vector3.Distance(vessel.transform.position, FlightGlobals.ActiveVessel.transform.position));
                            if (num > FlightGlobals.ActiveVessel.vesselRanges.landed.load)
                            {
                                IsOutOfRange = true;
                            }
                            else
                            {
                                IsOutOfRange = false;
                            }
                        }
                    }
                    else
                    {
                        CameraPositioning(cameraMode3);
                        CurrentZoom = 32f;
                        ZoomMultiplier = false;
                    }
                }
            }
            if (!IsLookAtMe && !IsFollowMe && !IsTargetCam)
            {
                IsOnboard = true;
                cameraMode = "Onboard";
                IsFollowMeOffsetX = (IsFollowMeOffsetY = (IsFollowMeOffsetZ = 0f));
                if (!IsOnboardEnabled)
                {
                    camObject.transform.rotation = (CurrentCamRotation);
                    camObject.transform.position = (CurrentCamPosition);
                    camObject.transform.localRotation = (CurrentCamLocalRotation);
                    camObject.transform.localPosition = (CurrentCamLocalPosition);
                    CurrentZoom = 32f;
                    ZoomMultiplier = false;
                    IsOnboardEnabled = true;
                }
            }
        }

        private void CameraPositioning(string lastCameraMode)
        {
            if (lastCameraMode == "Onboard")
            {
                CurrentCamRotation = camObject.transform.rotation;
                CurrentCamPosition = camObject.transform.position;
                CurrentCamLocalRotation = camObject.transform.localRotation;
                CurrentCamLocalPosition = camObject.transform.localPosition;
            }
            camObject.transform.rotation = (InitialCamRotation);
            camObject.transform.position = (InitialCamPosition);
            camObject.transform.localRotation = (InitialCamLocalRotation);
            camObject.transform.localPosition = (InitialCamLocalPosition);
        }

        private void SetRotationStep()
        {
            simplifiedRotateZBuffer = rotateZbuffer;
            if (Mathf.Abs(simplifiedRotateZBuffer) > 360f)
            {
                simplifiedRotateZBuffer %= 360f;
            }
            rotateStep = 1f;
            if (CalculatedZoom >= 512)
            {
                rotateStep = 0.02f;
            }
            else
            {
                if (CalculatedZoom >= 256)
                {
                    rotateStep = 0.04f;
                }
                else
                {
                    if (CalculatedZoom >= 128)
                    {
                        rotateStep = 0.06f;
                    }
                    else
                    {
                        if (CalculatedZoom >= 64)
                        {
                            rotateStep = 0.07f;
                        }
                        else
                        {
                            if (CalculatedZoom >= 32)
                            {
                                rotateStep = 0.08f;
                            }
                            else
                            {
                                if (CalculatedZoom >= 16)
                                {
                                    rotateStep = 0.32f;
                                }
                                else
                                {
                                    if (CalculatedZoom >= 8)
                                    {
                                        rotateStep = 0.64f;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void Update()
        {
            if (IsOrbital || IsOutOfRange)
            {
                base.UpdateWhiteNoise();
            }
            DrawScanningRay();
            DrawVisibilityRay();
            allCamerasGameObject.Last<GameObject>().transform.position = (camObject.gameObject.transform.position);
            allCamerasGameObject.Last<GameObject>().transform.rotation = (camObject.gameObject.transform.rotation);
            float num = -(lastZoom - CurrentZoom) / stepper;
            lastZoom = CurrentZoom;
            zoommer.transform.Translate(new Vector3(num, 0f, 0f));
            rotatorZ.transform.Rotate(new Vector3(0f, 0f, 1f), RotateZ);
            rotatorY.transform.Rotate(new Vector3(0f, 1f, 0f), RotateY);
            rotateZbuffer += RotateZ;
            rotateYbuffer += RotateY;
            zoomBuffer += num;
            allCamerasGameObject[0].transform.rotation = (allCamerasGameObject.Last<GameObject>().transform.rotation);
            allCamerasGameObject[1].transform.rotation = (allCamerasGameObject.Last<GameObject>().transform.rotation);
            allCamerasGameObject[2].transform.rotation = (allCamerasGameObject.Last<GameObject>().transform.rotation);
            allCamerasGameObject[2].transform.position = (allCamerasGameObject.Last<GameObject>().transform.position);
            allCameras.ForEach(delegate (UnityEngine.Camera cam)
            {
                cam.fieldOfView = (RealZoom);
            });
            RotateZ = 0f;
            RotateY = 0f;
        }

        private void DrawScanningRay()
        {
            GameObject.Destroy(scanningRay);
            if (!isRayEnabled || !TargetHelper.IsTargetSelect)
                return;
            Vector3 vector;
            if (!IsInsight(out vector))
                return;
            scanningRay = new GameObject("scanningRay").AddComponent<LineRenderer>();
            scanningRay.material = (new Material(Shader.Find("Particles/Additive")));
            scanningRay.SetColors(Color.red, Color.red);
            scanningRay.SetVertexCount(2);
            scanningRay.SetWidth(0.02f, 0.02f);
            scanningRay.useWorldSpace = (true);
            scanningRay.SetPosition(0, part.transform.position);
            scanningRay.SetPosition(1, vector);
        }

        private void DrawVisibilityRay()
        {
            GameObject.Destroy(visibilityRay);
            if (!isVisibilityRay)
                return; ;

            if (!TargetHelper.IsTargetSelect || !IsTargetVisiable())
                return;
            visibilityRay = new GameObject("visibilityRay").AddComponent<LineRenderer>();
            Color white = Color.white;
            visibilityRay.material = (new Material(Shader.Find("Particles/Additive")));
            visibilityRay.SetColors(white, white);
            visibilityRay.SetVertexCount(2);
            visibilityRay.SetWidth(0.02f, 0.02f);
            visibilityRay.useWorldSpace = (true);
            visibilityRay.SetPosition(0, camObject.transform.position);
            visibilityRay.SetPosition(1, TargetHelper.Target.GetTransform().position);
        }

        private bool IsInsight(out Vector3 endPoint)
        {
            UnityEngine.Camera camera = allCameras.Last<UnityEngine.Camera>();
            endPoint = TargetHelper.Target.GetTransform().position;
            Vector3 vector = camera.WorldToViewportPoint(endPoint);
            float x = vector.x;
            float y = vector.y;
            float z = vector.z;
            return z > 0f && 0f <= x && x <= 1f && 0f <= y && y <= 1f;
        }

        private bool IsTargetVisiable()
        {
            bool result;
            foreach (CelestialBody current in FlightGlobals.Bodies)
            {
                double radius = current.Radius;
                Vector3d worldPos3D = part.vessel.GetWorldPos3D();
                Vector3 position = TargetHelper.Target.GetTransform().position;
                Vector3d vector3d = position - worldPos3D;
                double num = radius / (double)Vector3.Distance(worldPos3D, position);
                num *= 0.5;
                vector3d *= num;
                Vector3d vector3d2 = position - vector3d;
                float num2 = Vector3.Distance(current.position, vector3d2);
                if ((double)num2 < radius)
                {
                    result = false;
                    return result;
                }
            }
            result = true;
            return result;
        }

        public IEnumerator WaitForRay()
        {
            yield return new WaitForSeconds(1f);
            isRayEnabled = false;
            TargetHelper targetHelper = new TargetHelper(part);
            targetHelper.Update();
            Vector3 vector;
            if (targetHelper.Destination <= (float)allowedScanDistance && IsInsight(out vector) && IsTargetVisiable())
            {
                ScreenMessages.PostScreenMessage(FlightGlobals.activeTarget.vessel.vesselName + " HAS BEEN SCANNED", 3f, 0);
                if (!isScienceActivate)
                {
                    ModuleSpyExperiment moduleSpyExperiment = part.GetComponent<ModuleSpyExperiment>();
                    if (moduleSpyExperiment != null)
                    {
                        moduleSpyExperiment.DeployExperiment();
                    }
                    isScienceActivate = true;
                    moduleSpyExperiment = null;
                }
            }
            else
            {
                ScreenMessages.PostScreenMessage("NO DATA, TARGET " + FlightGlobals.activeTarget.vessel.vesselName + " IS OUT OF RANGE  OR VISIBILITY", 3f, 0);
            }
            yield break;
        }

        public IEnumerator ReturnCamToZero()
        {
            int num = 20;
            float num2 = -simplifiedRotateZBuffer / (float)num;
            float num3 = -rotateYbuffer / (float)num;
            float num4 = -zoomBuffer / (float)num;
            int num5;
            for (int i = 0; i < num; i = num5 + 1)
            {
                zoommer.transform.Translate(new Vector3(num4, 0f, 0f));
                rotatorZ.transform.Rotate(new Vector3(0f, 0f, 1f), num2);
                rotatorY.transform.Rotate(new Vector3(0f, 1f, 0f), num3);
                yield return new WaitForSeconds(0.05f);
                num5 = i;
            }
            rotateZbuffer = (rotateYbuffer = (zoomBuffer = 0f));
            CurrentZoom = MaxZoom;
            lastZoom = CurrentZoom;
            yield break;
        }
    }
}

