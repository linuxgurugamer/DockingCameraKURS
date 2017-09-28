using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace OLDD_camera.Camera
{
	public abstract class BaseCamera
	{
		[CompilerGenerated]
		[Serializable]
		private sealed class <>c
		{
			public static readonly BaseCamera.<>c <>9 = new BaseCamera.<>c();

			public static Func<string, GameObject> <>9__48_0;

			public static Func<Camera, bool> <>9__56_0;

			public static Action<Camera> <>9__59_0;

			internal GameObject <InitCameras>b__48_0(string a)
			{
				return new GameObject();
			}

			internal bool <ExtendedDrawWindowL3>b__56_0(Camera x)
			{
				return x.get_name() == "Camera 00";
			}

			internal void <Render>b__59_0(Camera a)
			{
				a.Render();
			}
		}

		[CompilerGenerated]
		private sealed class <ResizeWindow>d__60 : IEnumerator<object>, IDisposable, IEnumerator
		{
			private int <>1__state;

			private object <>2__current;

			public BaseCamera <>4__this;

			object IEnumerator<object>.Current
			{
				[DebuggerHidden]
				get
				{
					return this.<>2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return this.<>2__current;
				}
			}

			[DebuggerHidden]
			public <ResizeWindow>d__60(int <>1__state)
			{
				this.<>1__state = <>1__state;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
			}

			bool IEnumerator.MoveNext()
			{
				int num = this.<>1__state;
				if (num != 0)
				{
					if (num != 1)
					{
						return false;
					}
					this.<>1__state = -1;
				}
				else
				{
					this.<>1__state = -1;
					this.<>4__this.IsAuxiliaryWindowButtonPres = false;
				}
				bool flag = this.<>4__this.IsAuxiliaryWindowOpen && this.<>4__this.WindowPosition.get_width() < this.<>4__this.WindowSize * (float)this.<>4__this.WindowSizeCoef + 92f;
				if (flag)
				{
					BaseCamera expr_85_cp_0 = this.<>4__this;
					expr_85_cp_0.WindowPosition.set_width(expr_85_cp_0.WindowPosition.get_width() + 4f);
					bool flag2 = this.<>4__this.WindowPosition.get_width() >= this.<>4__this.WindowSize * (float)this.<>4__this.WindowSizeCoef + 92f;
					if (flag2)
					{
						return false;
					}
				}
				else
				{
					bool flag3 = this.<>4__this.WindowPosition.get_width() > this.<>4__this.WindowSize * (float)this.<>4__this.WindowSizeCoef;
					if (!flag3)
					{
						return false;
					}
					BaseCamera expr_113_cp_0 = this.<>4__this;
					expr_113_cp_0.WindowPosition.set_width(expr_113_cp_0.WindowPosition.get_width() - 4f);
					bool flag4 = this.<>4__this.WindowPosition.get_width() <= this.<>4__this.WindowSize * (float)this.<>4__this.WindowSizeCoef;
					if (flag4)
					{
						return false;
					}
				}
				this.<>2__current = new WaitForSeconds(0.0108695654f);
				this.<>1__state = 1;
				return true;
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}
		}

		protected static int WindowCount;

		protected static double ElectricChargeAmount;

		public static Material CurrentShader;

		protected UpdateGUIObject UpdateGUIObject;

		internal Rect WindowPosition;

		internal Rect TexturePosition;

		protected string WindowLabel;

		protected string SubWindowLabel;

		protected GameObject PartGameObject;

		protected Part ThisPart;

		private Texture _textureBackGroundCamera;

		private Texture _textureSeparator;

		private Texture _textureTargetMark;

		internal Texture[] TextureNoSignal;

		internal int TextureNoSignalId;

		protected RenderTexture RenderTexture;

		private ShaderType _shaderType;

		private ShaderType1 _shaderType1;

		private ShaderType2 _shaderType2;

		private static string _currentShaderName;

		internal static int ShadersToUse = 0;

		protected float WindowSize;

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

		private bool _isTargetPoint;

		protected int WindowSizeCoef = 2;

		protected int WindowId = Random.Range(1000, 10000);

		internal bool IsActive;

		internal bool IsButtonOff;

		internal bool IsOrbital;

		internal bool IsAuxiliaryWindowOpen;

		internal bool IsAuxiliaryWindowButtonPres;

		protected List<Camera> AllCameras = new List<Camera>();

		protected List<GameObject> AllCamerasGameObject = new List<GameObject>();

		protected List<string> CameraNames = new List<string>
		{
			"GalaxyCamera",
			"Camera ScaledSpace",
			"Camera 01",
			"Camera 00"
		};

		protected BaseCamera(Part thisPart, float windowSizeInit, string windowLabel = "Camera")
		{
			this.WindowSize = windowSizeInit / 2f;
			this.ThisPart = thisPart;
			this.SubWindowLabel = windowLabel;
			this.WindowLabel = windowLabel;
			this.PartGameObject = thisPart.get_gameObject();
			this.InitWindow();
			this.InitTextures();
			GameEvents.OnFlightUIModeChanged.Add(new EventData<FlightUIMode>.OnEvent(this.FlightUIModeChanged));
			GameObject gameObject = new GameObject();
			this.UpdateGUIObject = gameObject.AddComponent<UpdateGUIObject>();
		}

		~BaseCamera()
		{
			GameEvents.OnFlightUIModeChanged.Remove(new EventData<FlightUIMode>.OnEvent(this.FlightUIModeChanged));
		}

		private void FlightUIModeChanged(FlightUIMode mode)
		{
			this.IsOrbital = (mode == 2);
		}

		protected virtual void InitWindow()
		{
			this.WindowPosition.set_width(this.WindowSize * (float)this.WindowSizeCoef);
			this.WindowPosition.set_height(this.WindowSize * (float)this.WindowSizeCoef + 34f);
		}

		protected virtual void InitTextures()
		{
			this.TexturePosition = new Rect(6f, 34f, this.WindowPosition.get_width() - 12f, this.WindowPosition.get_height() - 40f);
			this.RenderTexture = new RenderTexture((int)this.WindowSize * 4, (int)this.WindowSize * 4, 24, 4);
			RenderTexture.set_active(this.RenderTexture);
			this.RenderTexture.Create();
			this._textureBackGroundCamera = Util.MonoColorRectTexture(new Color(0.45f, 0.45f, 0.45f, 1f));
			this._textureSeparator = Util.MonoColorVerticalLineTexture(Color.get_white(), (int)this.TexturePosition.get_height());
			this._textureTargetMark = AssetLoader.texTargetPoint;
			this.TextureNoSignal = new Texture[8];
			for (int i = 0; i < this.TextureNoSignal.Length; i++)
			{
				this.TextureNoSignal[i] = Util.WhiteNoiseTexture((int)this.TexturePosition.get_width(), (int)this.TexturePosition.get_height(), 1f);
			}
		}

		protected virtual void InitCameras()
		{
			IEnumerable<string> arg_27_0 = this.CameraNames;
			Func<string, GameObject> arg_27_1;
			if ((arg_27_1 = BaseCamera.<>c.<>9__48_0) == null)
			{
				arg_27_1 = (BaseCamera.<>c.<>9__48_0 = new Func<string, GameObject>(BaseCamera.<>c.<>9.<InitCameras>b__48_0));
			}
			this.AllCamerasGameObject = arg_27_0.Select(arg_27_1).ToList<GameObject>();
			this.AllCameras = this.AllCamerasGameObject.Select(delegate(GameObject go, int i)
			{
				Camera camera = go.AddComponent<Camera>();
				Camera camera2 = Camera.get_allCameras().FirstOrDefault((Camera cam) => cam.get_name() == this.CameraNames[i]);
				bool flag = camera2 != null;
				if (flag)
				{
					camera.CopyFrom(camera2);
					camera.set_name(string.Format("{1} copy of {0}", this.CameraNames[i], BaseCamera.WindowCount));
					camera.set_targetTexture(this.RenderTexture);
				}
				return camera;
			}).ToList<Camera>();
		}

		protected virtual void DestroyCameras()
		{
			this.AllCameras.ForEach(new Action<Camera>(Object.Destroy));
			this.AllCameras.Clear();
		}

		public virtual void Activate()
		{
			bool isActive = this.IsActive;
			if (!isActive)
			{
				BaseCamera.WindowCount++;
				this.InitCameras();
				this.IsActive = true;
				this.UpdateGUIObject.UpdateGUIFunction += new Action(this.Begin);
			}
		}

		public virtual void Deactivate()
		{
			bool flag = !this.IsActive;
			if (!flag)
			{
				BaseCamera.WindowCount--;
				this.DestroyCameras();
				this.IsActive = false;
				this.UpdateGUIObject.UpdateGUIFunction -= new Action(this.Begin);
			}
		}

		private void Begin()
		{
			bool isActive = this.IsActive;
			if (isActive)
			{
				this.WindowPosition = GUI.Window(this.WindowId, KSPUtil.ClampRectToScreen(this.WindowPosition), new GUI.WindowFunction(this.DrawWindow), this.WindowLabel);
				int id = PartResourceLibrary.get_Instance().GetDefinition("ElectricCharge").get_id();
				double num;
				double num2;
				this.ThisPart.GetConnectedResourceTotals(id, ref num, ref num2, true);
				bool flag = HighLogic.LoadedSceneIsFlight && !FlightDriver.get_Pause();
				if (flag)
				{
					this.ThisPart.RequestResource(id, 0.02 * (double)TimeWarp.get_fixedDeltaTime());
				}
			}
		}

		private void DrawWindow(int id)
		{
			this.ExtendedDrawWindowL1();
			this.ExtendedDrawWindowL2();
			this.ExtendedDrawWindowL3();
			GUI.DragWindow();
		}

		protected virtual void ExtendedDrawWindowL1()
		{
			float num = this.WindowPosition.get_width() - 90f;
			int num2 = (int)(this.MaxZoom - this.CurrentZoom + this.MinZoom);
			this.CalculatedZoom = ((!this.ZoomMultiplier) ? num2 : (num2 * this.MinZoomMultiplier * 6));
			GUI.Label(new Rect(num, 128f, 80f, 20f), "zoom: " + this.CalculatedZoom, Styles.Label13B);
			bool flag = FlightGlobals.get_ActiveVessel() == this.ThisPart.vessel;
			if (flag)
			{
				this._isTargetPoint = GUI.Toggle(new Rect(num - 2f, 233f, 88f, 20f), this._isTargetPoint, "Target Mark");
			}
			GUI.DrawTexture(this.TexturePosition, this._textureBackGroundCamera);
			switch (BaseCamera.ShadersToUse)
			{
			case 0:
				BaseCamera.CurrentShader = CameraShaders.GetShader(this._shaderType);
				break;
			case 1:
				BaseCamera.CurrentShader = CameraShaders.GetShader1(this._shaderType1);
				break;
			case 2:
				BaseCamera.CurrentShader = CameraShaders.GetShader2(this._shaderType2);
				break;
			}
			BaseCamera._currentShaderName = ((BaseCamera.CurrentShader == null) ? "none" : BaseCamera.CurrentShader.get_name());
			bool flag2 = Event.get_current().get_type().Equals(7);
			if (flag2)
			{
				Graphics.DrawTexture(this.TexturePosition, this.Render(), BaseCamera.CurrentShader);
			}
		}

		protected virtual void ExtendedDrawWindowL2()
		{
			bool isTargetSelect = TargetHelper.IsTargetSelect;
			if (isTargetSelect)
			{
				Camera camera = this.AllCameras.Last<Camera>();
				Vessel vessel = TargetHelper.Target as Vessel;
				bool flag = vessel == null;
				if (flag)
				{
					ModuleDockingNode moduleDockingNode = TargetHelper.Target as ModuleDockingNode;
					vessel = moduleDockingNode.get_vessel();
				}
				Vector3 vector = camera.WorldToViewportPoint(vessel.get_transform().get_position());
				float x = vector.x;
				float y = vector.y;
				float z = vector.z;
				x = this.GetX(x, z);
				y = this.GetY(y, z);
				float num = this.TexturePosition.get_width() * x;
				float num2 = this.TexturePosition.get_height() * y;
				bool isTargetPoint = this._isTargetPoint;
				if (isTargetPoint)
				{
					GUI.DrawTexture(new Rect(this.TexturePosition.get_xMin() + num - 10f, this.TexturePosition.get_yMax() - num2 - 10f, 20f, 20f), this._textureTargetMark);
				}
			}
			bool isOrbital = this.IsOrbital;
			if (isOrbital)
			{
				GUI.DrawTexture(this.TexturePosition, this.TextureNoSignal[this.TextureNoSignalId]);
			}
		}

		protected virtual void ExtendedDrawWindowL3()
		{
			bool flag = !this.ThisPart.vessel.Equals(FlightGlobals.get_ActiveVessel());
			if (flag)
			{
				GUI.Label(new Rect(8f, 34f, 222f, 20f), "Broadcast from: " + this.ThisPart.vessel.vesselName, Styles.GreenLabel11);
			}
			bool isAuxiliaryWindowOpen = this.IsAuxiliaryWindowOpen;
			if (isAuxiliaryWindowOpen)
			{
				GUI.DrawTexture(new Rect(this.TexturePosition.get_width() + 8f, 34f, 1f, this.TexturePosition.get_height()), this._textureSeparator);
			}
			bool flag2 = GUI.Button(new Rect(this.WindowPosition.get_width() - 20f, 3f, 15f, 15f), " ");
			if (flag2)
			{
				this.IsButtonOff = true;
			}
			bool flag3 = GUI.Button(new Rect(this.WindowPosition.get_width() - 29f, 18f, 24f, 15f), this.IsAuxiliaryWindowOpen ? "◄" : "►");
			if (flag3)
			{
				this.IsAuxiliaryWindowOpen = !this.IsAuxiliaryWindowOpen;
				this.IsAuxiliaryWindowButtonPres = true;
			}
			GUIContent gUIContent = new GUIContent("☼", BaseCamera._currentShaderName);
			GUI.Box(new Rect(8f, this.TexturePosition.get_yMax() - 22f, 20f, 20f), gUIContent);
			GUI.Label(new Rect(64f, 128f, 200f, 40f), GUI.get_tooltip(), Styles.GreenLabel15B);
			bool flag4 = GUI.Button(new Rect(8f, this.TexturePosition.get_yMax() - 22f, 20f, 20f), "☼");
			if (flag4)
			{
				switch (BaseCamera.ShadersToUse)
				{
				case 0:
				{
					this._shaderType++;
					bool flag5 = !Enum.IsDefined(typeof(ShaderType), this._shaderType);
					if (flag5)
					{
						this._shaderType = ShaderType.OldTV;
					}
					break;
				}
				case 1:
				{
					this._shaderType1++;
					bool flag6 = !Enum.IsDefined(typeof(ShaderType1), this._shaderType1);
					if (flag6)
					{
						this._shaderType1 = ShaderType1.OldTV;
					}
					break;
				}
				case 2:
				{
					this._shaderType2++;
					bool flag7 = !Enum.IsDefined(typeof(ShaderType2), this._shaderType2);
					if (flag7)
					{
						this._shaderType2 = ShaderType2.None;
					}
					break;
				}
				}
			}
			bool arg_30D_0;
			if (GUI.RepeatButton(new Rect(this.TexturePosition.get_xMax() - 22f, this.TexturePosition.get_yMax() - 22f, 20f, 20f), "±"))
			{
				IEnumerable<Camera> arg_2FF_0 = Camera.get_allCameras();
				Func<Camera, bool> arg_2FF_1;
				if ((arg_2FF_1 = BaseCamera.<>c.<>9__56_0) == null)
				{
					arg_2FF_1 = (BaseCamera.<>c.<>9__56_0 = new Func<Camera, bool>(BaseCamera.<>c.<>9.<ExtendedDrawWindowL3>b__56_0));
				}
				arg_30D_0 = (arg_2FF_0.FirstOrDefault(arg_2FF_1) != null);
			}
			else
			{
				arg_30D_0 = false;
			}
			bool flag8 = arg_30D_0;
			if (flag8)
			{
				int windowSizeCoef = this.WindowSizeCoef;
				if (windowSizeCoef != 2)
				{
					if (windowSizeCoef == 3)
					{
						this.WindowSizeCoef = 2;
					}
				}
				else
				{
					this.WindowSizeCoef = 3;
				}
				this.Deactivate();
				this.InitWindow();
				this.InitTextures();
				this.Activate();
				this.IsAuxiliaryWindowOpen = false;
			}
			this.CurrentZoom = GUI.HorizontalSlider(new Rect(this.TexturePosition.get_width() / 2f - 80f, 20f, 160f, 10f), this.CurrentZoom, this.MaxZoom, this.MinZoom);
		}

		private float GetX(float x, float z)
		{
			bool flag = x < 0f && z > 0f && x <= 0f;
			float result;
			if (flag)
			{
				result = 0f;
			}
			else
			{
				bool flag2 = x > 0f && z < 0f;
				if (flag2)
				{
					result = 0f;
				}
				else
				{
					bool flag3 = x < 0f && z < 0f;
					if (flag3)
					{
						result = 1f;
					}
					else
					{
						bool flag4 = x > 0f && z > 0f && x >= 1f;
						if (flag4)
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
			bool flag = z > 0f;
			float result;
			if (flag)
			{
				bool flag2 = y <= 0f;
				if (flag2)
				{
					result = 0f;
					return result;
				}
				bool flag3 = y >= 1f;
				if (flag3)
				{
					result = 1f;
					return result;
				}
			}
			bool flag4 = z < 0f;
			if (flag4)
			{
				bool flag5 = y <= 0f;
				if (flag5)
				{
					result = 0f;
					return result;
				}
				bool flag6 = y >= 1f;
				if (flag6)
				{
					result = 1f;
					return result;
				}
			}
			result = y;
			return result;
		}

		protected virtual RenderTexture Render()
		{
			List<Camera> arg_26_0 = this.AllCameras;
			Action<Camera> arg_26_1;
			if ((arg_26_1 = BaseCamera.<>c.<>9__59_0) == null)
			{
				arg_26_1 = (BaseCamera.<>c.<>9__59_0 = new Action<Camera>(BaseCamera.<>c.<>9.<Render>b__59_0));
			}
			arg_26_0.ForEach(arg_26_1);
			return this.RenderTexture;
		}

		public IEnumerator ResizeWindow()
		{
			BaseCamera.<ResizeWindow>d__60 expr_06 = new BaseCamera.<ResizeWindow>d__60(0);
			expr_06.<>4__this = this;
			return expr_06;
		}

		protected void UpdateWhiteNoise()
		{
			this.TextureNoSignalId++;
			bool flag = this.TextureNoSignalId >= this.TextureNoSignal.Length;
			if (flag)
			{
				this.TextureNoSignalId = 0;
			}
		}

		public virtual void Update()
		{
			this.AllCamerasGameObject.Last<GameObject>().get_transform().set_position(this.PartGameObject.get_transform().get_position());
			this.AllCamerasGameObject.Last<GameObject>().get_transform().set_rotation(this.PartGameObject.get_transform().get_rotation());
			this.AllCamerasGameObject.Last<GameObject>().get_transform().Rotate(new Vector3(-1f, 0f, 0f), 90f);
			this.AllCamerasGameObject.Last<GameObject>().get_transform().Rotate(new Vector3(0f, 1f, 0f), this.RotateY);
			this.AllCamerasGameObject.Last<GameObject>().get_transform().Rotate(new Vector3(1f, 0f, 0f), this.RotateX);
			this.AllCamerasGameObject.Last<GameObject>().get_transform().Rotate(new Vector3(0f, 0f, 1f), this.RotateZ);
			this.AllCamerasGameObject[0].get_transform().set_rotation(this.AllCamerasGameObject.Last<GameObject>().get_transform().get_rotation());
			this.AllCamerasGameObject[1].get_transform().set_rotation(this.AllCamerasGameObject.Last<GameObject>().get_transform().get_rotation());
			this.AllCamerasGameObject[2].get_transform().set_rotation(this.AllCamerasGameObject.Last<GameObject>().get_transform().get_rotation());
			this.AllCamerasGameObject[2].get_transform().set_position(this.AllCamerasGameObject.Last<GameObject>().get_transform().get_position());
			this.AllCameras.ForEach(delegate(Camera cam)
			{
				cam.set_fieldOfView(this.CurrentZoom);
			});
		}
	}
}
