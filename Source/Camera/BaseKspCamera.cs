using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace OLDD_camera.Camera
{
    public abstract class BaseKspCamera
    {

        protected static int windowCount;
        protected static double ElectricChargeAmount;
        public static Material CurrentShader;
        protected UpdateGUIObject UpdateGUIObject;

        internal Rect windowPosition;
        internal Rect texturePosition;
        protected string WindowLabel;
        protected string SubWindowLabel;
        protected GameObject partGameObject;
        protected Part part;

        private Texture textureBackGroundCamera;
        private Texture textureSeparator;
        private Texture textureTargetMark;
        internal Texture[] textureNoSignal;
        internal int TextureNoSignalId;
        protected RenderTexture renderTexture;

        private ShaderType shaderType;
        private ShaderType1 shaderType1;
        private ShaderType2 shaderType2;
        private static string currentShaderName;
        internal static int ShadersToUse = 0;

        protected float windowSize;
        private const float WindowAddition = 92f;
        protected float RotateX = 0f;
        protected float RotateY = 0f;
        protected float RotateZ = 0f;

        protected int MinZoomMultiplier = 4;
        internal float MinZoom = 1f;
        internal float MaxZoom = 32f;
        internal float CurrentZoom = 32f;
        internal int CalculatedZoom;
        internal bool ZoomMultiplier = false;

        private bool isTargetPoint;

        protected int WindowSizeCoef = 2;
        protected int WindowId = UnityEngine.Random.Range(1000, 10000);

        internal bool IsActive;
        internal bool IsButtonOff;
        internal bool IsOrbital;
        internal bool IsAuxiliaryWindowOpen;
        internal bool IsAuxiliaryWindowButtonPres;

        protected List<UnityEngine.Camera> allCameras = new List<UnityEngine.Camera>();

        protected List<GameObject> allCamerasGameObject = new List<GameObject>();

        protected List<string> cameraNames = new List<string>
        {
            "GalaxyCamera",
            "Camera ScaledSpace",
            "Camera 01",
            "Camera 00"
        };

        protected BaseKspCamera(Part p, float windowSizeInit, string windowLabel = "Camera")
        {
            windowSize = windowSizeInit / 2f;
            part = p;
            SubWindowLabel = windowLabel;
            WindowLabel = windowLabel;
            partGameObject = p.gameObject;
            InitWindow();
            InitTextures();
            GameEvents.OnFlightUIModeChanged.Add(new EventData<FlightUIMode>.OnEvent(FlightUIModeChanged));
            GameObject gameObject = new GameObject();
            UpdateGUIObject = gameObject.AddComponent<UpdateGUIObject>();
        }

        ~BaseKspCamera()
        {
            GameEvents.OnFlightUIModeChanged.Remove(new EventData<FlightUIMode>.OnEvent(FlightUIModeChanged));
        }

        private void FlightUIModeChanged(FlightUIMode mode)
        {
            if (mode == FlightUIMode.ORBITAL)
                IsOrbital = true;
            else
                IsOrbital = false;
        }

        /// <summary>
        /// Initializes window
        /// </summary>
        protected virtual void InitWindow()
        {
            windowPosition.width = (windowSize * (float)WindowSizeCoef);
            windowPosition.height = (windowSize * (float)WindowSizeCoef + 34f);
        }

        /// <summary>
        /// Initializes texture
        /// </summary>
        protected virtual void InitTextures()
        {
            texturePosition = new Rect(6, 34, windowPosition.width - 12f, windowPosition.height - 40f); //42f);
            renderTexture = new RenderTexture((int)windowSize * 4, (int)windowSize * 4, 24, RenderTextureFormat.RGB565);
            RenderTexture.active = renderTexture;
            renderTexture.Create();
            textureBackGroundCamera = Util.MonoColorRectTexture(new Color(0.45f, 0.45f, 0.45f, 1));
            textureSeparator = Util.MonoColorVerticalLineTexture(Color.white, (int)texturePosition.height);
            textureTargetMark = AssetLoader.texTargetPoint;
            textureNoSignal = new Texture[8];
            for (int i = 0; i < textureNoSignal.Length; i++)
            {
                textureNoSignal[i] = Util.WhiteNoiseTexture((int)texturePosition.width, (int)texturePosition.height, 1f);
            }
        }

        /// <summary>
        /// Initializes camera
        /// </summary>
        protected virtual void InitCameras()
        {
            allCamerasGameObject = cameraNames.Select(a => new GameObject()).ToList();

            allCameras = allCamerasGameObject.Select((go, i) =>
            {
                var camera = go.AddComponent<UnityEngine.Camera>();
                var cameraExample = UnityEngine.Camera.allCameras.FirstOrDefault(cam => cam.name == cameraNames[i]);
                if (cameraExample != null)
                {
                    camera.CopyFrom(cameraExample);
                    camera.name = string.Format("{1} copy of {0}", cameraNames[i], windowCount);
                    camera.targetTexture = renderTexture;
                }
                return camera;
            }).ToList();
        }

        /// <summary>
        /// Destroy Cameras
        /// </summary>
        protected virtual void DestroyCameras()
        {
            if (allCameras != null)
            {
                allCameras.ForEach(UnityEngine.Camera.Destroy);
                allCameras.Clear();
            }
        }

        /// <summary>
        /// Create and activate cameras
        /// </summary>
        public virtual void Activate()
        {
            if (!IsActive)
            {
                BaseKspCamera.windowCount++;
                InitCameras();
                IsActive = true;
                UpdateGUIObject.updateGUIFunction += Begin;
            }
        }

        /// <summary>
        /// Destroy  cameras
        /// </summary>
        public virtual void Deactivate()
        {
            if (!IsActive)
                return;
            BaseKspCamera.windowCount--;
            DestroyCameras();
            IsActive = false;
            if (UpdateGUIObject.updateGUIFunction != null)
                UpdateGUIObject.updateGUIFunction -= Begin;
        }

        private void Begin()
        {
            if (IsActive)
            {
                windowPosition = GUI.Window(WindowId, KSPUtil.ClampRectToScreen(windowPosition), new GUI.WindowFunction(DrawWindow), WindowLabel);
                int id = PartResourceLibrary.Instance.GetDefinition("ElectricCharge").id;
                double num;
                double num2;
                part.GetConnectedResourceTotals(id, out num, out num2, true);
                if (HighLogic.LoadedSceneIsFlight && !FlightDriver.Pause)
                {
                    part.RequestResource(id, 0.02 * (double)TimeWarp.fixedDeltaTime);
                }
            }
        }

        #region DRAW LAYERS

        /// <summary>
        /// drawing method
        /// </summary>
        private void DrawWindow(int id)
        {
            ExtendedDrawWindowL1();
            ExtendedDrawWindowL2();
            ExtendedDrawWindowL3();
            GUI.DragWindow();
        }

        /// <summary>
        /// drawing method, first layer, for cameras
        /// </summary>
		protected virtual void ExtendedDrawWindowL1()
        {
            float num = windowPosition.width - 90f;
            int num2 = (int)(MaxZoom - CurrentZoom + MinZoom);
            CalculatedZoom = ((!ZoomMultiplier) ? num2 : (num2 * MinZoomMultiplier * 6));
            GUI.Label(new Rect(num, 128f, 80f, 20f), "zoom: " + CalculatedZoom, Styles.guiStyleLabelWhiteBold);
            if (FlightGlobals.ActiveVessel == part.vessel)
            {
                isTargetPoint = GUI.Toggle(new Rect(num - 2f, 233f, 88f, 20f), isTargetPoint, "Target Mark");
            }
            GUI.DrawTexture(texturePosition, textureBackGroundCamera);
            switch (BaseKspCamera.ShadersToUse)
            {
                case 0:
                    BaseKspCamera.CurrentShader = CameraShaders.GetShader(shaderType);
                    break;
                case 1:
                    BaseKspCamera.CurrentShader = CameraShaders.GetShader1(shaderType1);
                    break;
                case 2:
                    BaseKspCamera.CurrentShader = CameraShaders.GetShader2(shaderType2);
                    break;
            }
            BaseKspCamera.currentShaderName = ((BaseKspCamera.CurrentShader == null) ? "none" : BaseKspCamera.CurrentShader.name);
            if (Event.current.type == EventType.Repaint)
            {
                Graphics.DrawTexture(texturePosition, Render(), BaseKspCamera.CurrentShader);
            }
        }

        /// <summary>
        /// drawing method, second layer (draw any texture between cam and interface)
        /// </summary>
        protected virtual void ExtendedDrawWindowL2()
        {
            if (TargetHelper.IsTargetSelect)
            {
                UnityEngine.Camera camera = allCameras.Last<UnityEngine.Camera>();
                Vessel vessel = TargetHelper.Target as Vessel;
                if (vessel == null)
                {
                    ModuleDockingNode moduleDockingNode = TargetHelper.Target as ModuleDockingNode;
                    vessel = moduleDockingNode.vessel;
                }
                Vector3 vector = camera.WorldToViewportPoint(vessel.transform.position);
                float x = vector.x;
                float y = vector.y;
                float z = vector.z;
                x = GetX(x, z);
                y = GetY(y, z);
                float num = texturePosition.width * x;
                float num2 = texturePosition.height * y;

                if (isTargetPoint)
                {
                    GUI.DrawTexture(new Rect(texturePosition.xMin + num - 10f, texturePosition.yMax - num2 - 10f, 20f, 20f), textureTargetMark);
                }
            }

            if (IsOrbital)
            {
                GUI.DrawTexture(texturePosition, textureNoSignal[TextureNoSignalId]);
            }
        }

        /// <summary>
        /// drawing method, third layer, interface
        /// </summary>
        protected virtual void ExtendedDrawWindowL3()
        {
            if (!part.vessel.Equals(FlightGlobals.ActiveVessel))
            {
                GUI.Label(new Rect(8f, 34f, 222f, 20f), "Broadcast from: " + part.vessel.vesselName, Styles.guiStyleGreenLabelSmall);
            }

            if (IsAuxiliaryWindowOpen)
            {
                GUI.DrawTexture(new Rect(texturePosition.width + 8f, 34f, 1f, texturePosition.height), textureSeparator);
            }
            if (GUI.Button(new Rect(windowPosition.width - 20f, 3f, 15f, 15f), " "))
            {
                IsButtonOff = true;
            }
            if (GUI.Button(new Rect(windowPosition.width - 29f, 18f, 24f, 15f), IsAuxiliaryWindowOpen ? "◄" : "►"))
            {
                IsAuxiliaryWindowOpen = !IsAuxiliaryWindowOpen;
                IsAuxiliaryWindowButtonPres = true;
            }
            GUIContent gUIContent = new GUIContent("☼", BaseKspCamera.currentShaderName);
            GUI.Box(new Rect(8f, texturePosition.yMax - 22f, 20f, 20f), gUIContent);
            GUI.Label(new Rect(64f, 128f, 200f, 40f), GUI.tooltip, Styles.guiStyleGreenLabelBold);
            if (GUI.Button(new Rect(8f, texturePosition.yMax - 22f, 20f, 20f), "☼"))
            {
                switch (BaseKspCamera.ShadersToUse)
                {
                    case 0:
                        {
                            shaderType++;
                            if (!Enum.IsDefined(typeof(ShaderType), shaderType))
                            {
                                shaderType = ShaderType.OldTV;
                            }
                            break;
                        }
                    case 1:
                        {
                            shaderType1++;
                            if (!Enum.IsDefined(typeof(ShaderType1), shaderType1))
                            {
                                shaderType1 = ShaderType1.OldTV;
                            }
                            break;
                        }
                    case 2:
                        {
                            shaderType2++;
                            if (!Enum.IsDefined(typeof(ShaderType2), shaderType2))
                            {
                                shaderType2 = ShaderType2.None;
                            }
                            break;
                        }
                }
            }

            if (GUI.RepeatButton(new Rect(texturePosition.xMax - 22f, texturePosition.yMax - 22f, 20f, 20f), "±") &&
                UnityEngine.Camera.allCameras.FirstOrDefault(x => x.name == "Camera 00") != null) //Size of main window
            {
                int windowSizeCoef = WindowSizeCoef;
                if (windowSizeCoef != 2)
                {
                    if (windowSizeCoef == 3)
                    {
                        WindowSizeCoef = 2;
                    }
                }
                else
                {
                    WindowSizeCoef = 3;
                }
                Deactivate();
                InitWindow();
                InitTextures();
                Activate();
                IsAuxiliaryWindowOpen = false;
            }
            CurrentZoom = GUI.HorizontalSlider(new Rect(texturePosition.width / 2f - 80f, 20f, 160f, 10f), CurrentZoom, MaxZoom, MinZoom);
        }

        #endregion DRAW LAYERS

        private float GetX(float x, float z)
        {
            float result;

            if (x < 0f && z > 0f && x <= 0f)
            {
                result = 0f;
            }
            else
            {
                if (x > 0f && z < 0f)
                {
                    result = 0f;
                }
                else
                {
                    if (x < 0f && z < 0f)
                    {
                        result = 1f;
                    }
                    else
                    {
                        if (x > 0f && z > 0f && x >= 1f)
                        {
                            result = 1f;
                        }
                        else
                        {
                            result = x;
                        }
                    }
                }
            }
            return result;
        }

        private float GetY(float y, float z)
        {
            float result;

            if (z > 0f)
            {
                if (y <= 0f)
                {
                    result = 0f;
                    return result;
                }
                if (y >= 1f)
                {
                    result = 1f;
                    return result;
                }
            }
            if (z < 0f)
            {
                if (y <= 0f)
                {
                    result = 0f;
                    return result;
                }
                if (y >= 1f)
                {
                    result = 1f;
                    return result;
                }
            }
            result = y;
            return result;
        }

        /// <summary>
        /// render texture camera
        /// </summary>
        protected virtual RenderTexture Render()
        {
            Log.Info("RenderTexture.render  allCameras.Count: " + allCameras.Count.ToString());
            allCameras.ForEach(a => a.Render());
            return renderTexture;

        }

        public IEnumerator ResizeWindow()
        {
            IsAuxiliaryWindowButtonPres = false;
            while (true)
            {
                if (IsAuxiliaryWindowOpen && windowPosition.width < windowSize * (float)WindowSizeCoef + 92f)
                {
                    windowPosition.width = (windowPosition.width + 4f);
                    if (windowPosition.width >= windowSize * (float)WindowSizeCoef + 92f)
                    {
                        break;
                    }
                }
                else
                {
                    if (windowPosition.width <= windowSize * (float)WindowSizeCoef)
                        break;

                    windowPosition.width = (windowPosition.width - 4f);
                    if (windowPosition.width <= windowSize * (float)WindowSizeCoef)
                        break;
                }
                yield return new WaitForSeconds(0.0108695654f);
            }
            yield break;
        }

        protected void UpdateWhiteNoise()
        {
            TextureNoSignalId++;
            if (TextureNoSignalId >= textureNoSignal.Length)
            {
                TextureNoSignalId = 0;
            }
        }

        public virtual void Update()
        {
            allCamerasGameObject.Last<GameObject>().transform.position = partGameObject.transform.position;
            allCamerasGameObject.Last<GameObject>().transform.rotation = partGameObject.transform.rotation;
            allCamerasGameObject.Last<GameObject>().transform.Rotate(new Vector3(-1f, 0f, 0f), 90f);
            allCamerasGameObject.Last<GameObject>().transform.Rotate(new Vector3(0f, 1f, 0f), RotateY);
            allCamerasGameObject.Last<GameObject>().transform.Rotate(new Vector3(1f, 0f, 0f), RotateX);
            allCamerasGameObject.Last<GameObject>().transform.Rotate(new Vector3(0f, 0f, 1f), RotateZ);
            allCamerasGameObject[0].transform.rotation = (allCamerasGameObject.Last<GameObject>().transform.rotation);
            allCamerasGameObject[1].transform.rotation = (allCamerasGameObject.Last<GameObject>().transform.rotation);
            allCamerasGameObject[2].transform.rotation = (allCamerasGameObject.Last<GameObject>().transform.rotation);
            allCamerasGameObject[2].transform.position = (allCamerasGameObject.Last<GameObject>().transform.position);
            allCameras.ForEach(delegate (UnityEngine.Camera cam)
            {
                cam.fieldOfView = (CurrentZoom);
            });
        }
    }
}
