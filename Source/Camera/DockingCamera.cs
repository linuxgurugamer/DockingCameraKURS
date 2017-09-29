using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OLDD_camera.Camera
{
	internal class DockingCamera : BaseKspCamera
	{
		private static HashSet<int> usedId = new HashSet<int>();
		private static List<Texture2D>[] textureWhiteNoise;

		private int id;
		private int idTextureNoise;

		private Texture2D textureVLineOLDD;
		private Texture2D textureHLineOLDD;
		private Texture2D textureVLine;
		private Texture2D textureHLine;
		private Texture2D textureVLineBack;
		private Texture2D textureHLineBack;

		private readonly GameObject moduleDockingNodeGameObject;
		private TargetHelper target;

		internal bool Noise;
		internal bool TargetCrossOLDD;
		internal bool TargetCrossDPAI;
		private bool cameraData = true;
		private bool rotatorState = true;
		private readonly float maxSpeed = 2f;

		private Color targetCrossColorOLDD = new Color(0f, 0f, 0.9f, 1f);
		private Color targetCrossColorDPAI = new Color(0.5f, 0f, 0f, 1f);
		private Color targetCrossColorBack = new Color(0.9f, 0f, 0f, 1f);

		private string lastVesselName;
		private string windowLabelSuffix;

		public Color TargetCrossColorOLDD
		{
			get
			{
				return targetCrossColorOLDD;
			}
			set
			{
				targetCrossColorOLDD = value;
				textureVLineOLDD = Util.MonoColorVerticalLineTexture(targetCrossColorOLDD, (int)windowSize * WindowSizeCoef);
				textureHLineOLDD = Util.MonoColorHorizontalLineTexture(targetCrossColorOLDD, (int)windowSize * WindowSizeCoef);
			}
		}

		public Color TargetCrossColorDPAI
		{
			get
			{
				return targetCrossColorDPAI;
			}
			set
			{
				targetCrossColorDPAI = value;
				textureVLine = Util.MonoColorVerticalLineTexture(TargetCrossColorDPAI, (int)windowSize * WindowSizeCoef);
				textureHLine = Util.MonoColorHorizontalLineTexture(TargetCrossColorDPAI, (int)windowSize * WindowSizeCoef);
			}
		}

		public Color TargetCrossColorBack
		{
			get
			{
				return targetCrossColorBack;
			}
			set
			{
				targetCrossColorBack = value;
				textureVLineBack = Util.MonoColorVerticalLineTexture(TargetCrossColorBack, (int)windowSize * WindowSizeCoef);
				textureHLineBack = Util.MonoColorHorizontalLineTexture(TargetCrossColorBack, (int)windowSize * WindowSizeCoef);
			}
		}

		public DockingCamera(Part thisPart, bool noise, bool crossDPAI, bool crossOLDD, int windowSize, string windowLabel = "DockCam") : base(thisPart, (float)windowSize, windowLabel)
		{
			GameEvents.onGameSceneLoadRequested.Add(new EventData<GameScenes>.OnEvent(LevelWasLoaded));
			Noise = noise;
			TargetCrossDPAI = crossDPAI;
			TargetCrossOLDD = crossOLDD;
			target = new TargetHelper(thisPart);
			moduleDockingNodeGameObject = (GameObjectExtension.GetChild(partGameObject, "dockingNode") ?? partGameObject);
            if (DockingCamera.textureWhiteNoise != null || !Noise)
                return;
			DockingCamera.textureWhiteNoise = new List<Texture2D>[3];
			for (int i = 0; i < 3; i++)
			{
				DockingCamera.textureWhiteNoise[i] = new List<Texture2D>();
				for (int j = 0; j < 4; j++)
				{
					DockingCamera.textureWhiteNoise[i].Add(Util.WhiteNoiseTexture((int)texturePosition.width, (int)texturePosition.height, 0.16f));
				}
			}
		}

		private void LevelWasLoaded(GameScenes data)
		{
			DockingCamera.usedId = new HashSet<int>();
		}

		~DockingCamera()
		{
			GameEvents.onGameSceneLoadRequested.Remove(new EventData<GameScenes>.OnEvent(LevelWasLoaded));
		}

		protected override void InitTextures()
		{
			base.InitTextures();
			textureVLineOLDD = Util.MonoColorVerticalLineTexture(TargetCrossColorOLDD, (int)windowSize * WindowSizeCoef);
			textureHLineOLDD = Util.MonoColorHorizontalLineTexture(TargetCrossColorOLDD, (int)windowSize * WindowSizeCoef);
			textureVLine = Util.MonoColorVerticalLineTexture(TargetCrossColorDPAI, (int)windowSize * WindowSizeCoef);
			textureHLine = Util.MonoColorHorizontalLineTexture(TargetCrossColorDPAI, (int)windowSize * WindowSizeCoef);
			textureVLineBack = Util.MonoColorVerticalLineTexture(targetCrossColorBack, (int)windowSize * WindowSizeCoef);
			textureHLineBack = Util.MonoColorHorizontalLineTexture(targetCrossColorBack, (int)windowSize * WindowSizeCoef);
		}

		protected override void ExtendedDrawWindowL1()
		{
			float num = windowPosition.width - 92f;

			if (IsAuxiliaryWindowOpen)
			{
				if ( part.vessel.Equals(FlightGlobals.ActiveVessel) && TargetHelper.IsTargetSelect)
				{
					cameraData = GUI.Toggle(new Rect(num, 34f, 88f, 20f), cameraData, "Flight data");
					rotatorState = GUI.Toggle(new Rect(num, 54f, 88f, 20f), rotatorState, "Rotator");
					if ( target != null && target.IsDockPort)
					{
						TargetCrossDPAI = GUI.Toggle(new Rect(num, 74f, 88f, 20f), TargetCrossDPAI, "Cross DPAI");
						TargetCrossOLDD = GUI.Toggle(new Rect(num, 94f, 88f, 20f), TargetCrossOLDD, "Cross OLDD");
					}
					else
					{
						GUI.Label(new Rect(num, 76f, 88f, 60f), " Select\n docking\n port", Styles.guiStyleRedLabelBold);
					}
				}
				Noise = GUI.Toggle(new Rect(num, 253f, 88f, 20f), Noise, "Noise");
			}
			base.ExtendedDrawWindowL1();
		}

		protected override void ExtendedDrawWindowL2()
		{
			GUI.DrawTexture(texturePosition, AssetLoader.texDockingCam);

			if (Noise)
			{
				GUI.DrawTexture(texturePosition, DockingCamera.textureWhiteNoise[WindowSizeCoef - 2][idTextureNoise]);
			}
			base.ExtendedDrawWindowL2();
		}

		protected override void ExtendedDrawWindowL3()
		{
			if (target.IsMoveToTarget)
			{
				GUI.DrawTexture(new Rect(texturePosition.xMin + 20f, texturePosition.yMax - 20f, 20f, 20f), AssetLoader.texLampOn);
				GUI.Label(new Rect(texturePosition.xMin + 40f, texturePosition.yMax - 20f, 140f, 20f), string.Format("Time to dock:{0:f0}s", target.SecondsToDock));
			}
			else
			{
				GUI.DrawTexture(new Rect(texturePosition.xMin + 20f, texturePosition.yMax - 20f, 20f, 20f), AssetLoader.texLampOff);
			}
			GetWindowLabel();
			GetFlightData();
			GetCross();
			if ( rotatorState && part.vessel.Equals(FlightGlobals.ActiveVessel) && TargetHelper.IsTargetSelect)
			{
				float num = texturePosition.width / 7f;
				float num2 = texturePosition.xMin + texturePosition.width / 2f - num / 2f;
				Rect rect = new Rect(num2, texturePosition.yMax - num, num, num);
				GUI.DrawTexture(rect, AssetLoader.texTargetRot);
				Matrix4x4 matrix = GUI.matrix;
				GUIUtility.RotateAroundPivot(target.AngleZ, rect.center);
				GUI.DrawTexture(new Rect(num2, texturePosition.yMax - num, num, num), AssetLoader.texSelfRot);
				GUI.matrix =(matrix);
				float num3 = texturePosition.width / 8f;
				float num4 = texturePosition.xMin + texturePosition.width / 2f - num3 / 2f - 34f;
				Rect rect2 = new Rect(num4, texturePosition.yMax - num3, num3, num3);
				GUI.DrawTexture(rect2, AssetLoader.texTargetRot);
				Matrix4x4 matrix2 = GUI.matrix;
				GUIUtility.RotateAroundPivot(target.AngleX, rect2.center);
				GUI.DrawTexture(new Rect(num4, texturePosition.yMax - num3, num3, num3), AssetLoader.texSelfRot);
				GUI.matrix =(matrix2);
				float num5 = texturePosition.width / 8f;
				float num6 = texturePosition.xMin + texturePosition.width / 2f - num5 / 2f + 34f;
				Rect rect3 = new Rect(num6, texturePosition.yMax - num5, num5, num5);
				GUI.DrawTexture(rect3, AssetLoader.texTargetRot);
				Matrix4x4 matrix3 = GUI.matrix;
				GUIUtility.RotateAroundPivot(target.AngleY, rect3.center);
				GUI.DrawTexture(new Rect(num6, texturePosition.yMax - num5, num5, num5), AssetLoader.texSelfRot);
				GUI.matrix =(matrix3);
			}
			base.ExtendedDrawWindowL3();
		}

		private void GetWindowLabel()
		{
			if ( part.vessel.Equals(FlightGlobals.ActiveVessel))
			{
				if (TargetHelper.IsTargetSelect)
				{
					lastVesselName = TargetHelper.Target.GetName();
					windowLabelSuffix = " to " + lastVesselName;
					WindowLabel = string.Concat(new object[]
					{
						SubWindowLabel,
						" ",
						id,
						windowLabelSuffix
					});
				}
				else
				{
					if ( part.vessel.Equals(FlightGlobals.ActiveVessel))
					{
						WindowLabel = SubWindowLabel + " " + id;
						lastVesselName = "";
						windowLabelSuffix = lastVesselName;
					}
				}
			}
			else
			{
				WindowLabel = string.Concat(new object[]
				{
					SubWindowLabel,
					" ",
					id,
					windowLabelSuffix
				});
			}
		}

		private void GetCross()
		{
			if ( TargetCrossDPAI && target.IsDockPort)
			{
				Texture2D texture2D = target.LookForward ? textureVLine : textureVLineBack;
				Texture2D texture2D2 = target.LookForward ? textureHLine : textureHLineBack;
				float num = target.TargetMoveHelpX;
				float num2 = target.TargetMoveHelpY;
				if ( !target.LookForward)
				{
					num = 1f - num;
					num2 = 1f - num2;
				}
				GUI.DrawTexture(new Rect(texturePosition.xMin + Math.Abs(num * texturePosition.width) % texturePosition.width, texturePosition.yMin, 1f, texturePosition.height), texture2D);
				GUI.DrawTexture(new Rect(texturePosition.xMin, texturePosition.yMin + Math.Abs(num2 * texturePosition.height) % texturePosition.height, texturePosition.width, 1f), texture2D2);
			}
			if ( TargetCrossOLDD && target.IsDockPort)
			{
				float num3 = texturePosition.width / 2f;
				float num4 = texturePosition.height / 2f;
				if ( Mathf.Abs(target.AngleX) > 20f)
				{
					num3 += (float)((target.AngleX > 0f) ? -1 : 1) * (texturePosition.width / 2f - 1f);
				}
				else
				{
					num3 += texturePosition.width / 40f * -target.AngleX;
				}
				if ( Mathf.Abs(target.AngleY) > 20f)
				{
					num4 += (float)((target.AngleY > 0f) ? -1 : 1) * (texturePosition.height / 2f - 1f);
				}
				else
				{
					num4 += texturePosition.height / 40f * -target.AngleY;
				}
				GUI.DrawTexture(new Rect(texturePosition.xMin + num3, texturePosition.yMin, 1f, texturePosition.height), textureVLineOLDD);
				GUI.DrawTexture(new Rect(texturePosition.xMin, texturePosition.yMin + num4, texturePosition.width, 1f), textureHLineOLDD);
			}
		}

#if false
        private void GetFlightData()
		{
			if (cameraData)
			{
				if ( TargetHelper.IsTargetSelect && part.vessel.Equals(FlightGlobals.ActiveVessel))
				{
					float num = 0f;
					target.Update();
					if ( !target.IsDockPort)
					{
						GUI.Label(new Rect(texturePosition.xMin + 2f, 34f, 100f, 40f), "Target is not\n a DockPort");
						if ( target.Destination < 200f)
						{
							GUI.Label(new Rect(texturePosition.xMin + 2f, 68f, 100f, 40f), "DockPort is\n available", Styles.guiStyleGreenLabelSmall);
						}
					}
					else
					{
						GUI.Label(new Rect(texturePosition.xMin + 2f, 34f, 100f, 40f), "DockPort captured", Styles.guiStyleGreenLabelStandart);
					}
					string str = (Math.Abs(target.Destination) < 1000f) ? "{0:f2}" : "{0:f0}";
					int num2 = 16;
					float arg_16B_0 = texturePosition.xMax - 70f;
					float arg_160_0 = 34f;
					float expr_154 = num;
					num = expr_154 + 1f;
					GUI.Label(new Rect(arg_16B_0, arg_160_0 + expr_154 * (float)num2, 70f, 20f), string.Format("Dist:" + str, target.Destination), Styles.guiStyleLabelWhiteBold);
					num += 0.2f;
					float arg_1D1_0 = texturePosition.xMax - 70f;
					float arg_1C6_0 = 34f;
					float expr_1BA = num;
					num = expr_1BA + 1f;
					GUI.Label(new Rect(arg_1D1_0, arg_1C6_0 + expr_1BA * (float)num2, 70f, 20f), string.Format("dX:" + str, target.DX));
					float arg_22A_0 = texturePosition.xMax - 70f;
					float arg_21F_0 = 34f;
					float expr_213 = num;
					num = expr_213 + 1f;
					GUI.Label(new Rect(arg_22A_0, arg_21F_0 + expr_213 * (float)num2, 70f, 20f), string.Format("dY:" + str, target.DY));
					float arg_283_0 = texturePosition.xMax - 70f;
					float arg_278_0 = 34f;
					float expr_26C = num;
					num = expr_26C + 1f;
					GUI.Label(new Rect(arg_283_0, arg_278_0 + expr_26C * (float)num2, 70f, 20f), string.Format("dZ:" + str, target.DZ));
					num += 0.2f;
					if ( Math.Abs(target.SpeedX) > maxSpeed && Math.Abs(target.Destination) < 200f)
					{
						float arg_31C_0 = texturePosition.xMax - 70f;
						float arg_311_0 = 38f;
						float expr_305 = num;
						num = expr_305 + 1f;
						GUI.Label(new Rect(arg_31C_0, arg_311_0 + expr_305 * (float)num2, 70f, 20f), string.Format("vX:{0:f2}", target.SpeedX), Styles.guiStyleRedLabelNormal);
					}
					else
					{
						float arg_376_0 = texturePosition.xMax - 70f;
						float arg_36B_0 = 38f;
						float expr_35F = num;
						num = expr_35F + 1f;
						GUI.Label(new Rect(arg_376_0, arg_36B_0 + expr_35F * (float)num2, 70f, 20f), string.Format("vX:{0:f2}", target.SpeedX), Styles.guiStyleLabelWhiteNormal);
					}
					if ( Math.Abs(target.SpeedY) > maxSpeed && Math.Abs(target.Destination) < 200f)
					{
						float arg_406_0 = texturePosition.xMax - 70f;
						float arg_3FB_0 = 38f;
						float expr_3EF = num;
						num = expr_3EF + 1f;
						GUI.Label(new Rect(arg_406_0, arg_3FB_0 + expr_3EF * (float)num2, 70f, 20f), string.Format("vY:{0:f2}", target.SpeedY), Styles.guiStyleRedLabelNormal);
					}
					else
					{
						float arg_460_0 = texturePosition.xMax - 70f;
						float arg_455_0 = 38f;
						float expr_449 = num;
						num = expr_449 + 1f;
						GUI.Label(new Rect(arg_460_0, arg_455_0 + expr_449 * (float)num2, 70f, 20f), string.Format("vY:{0:f2}", target.SpeedY), Styles.guiStyleLabelWhiteNormal);
					}
					if ( Math.Abs(target.SpeedZ) > maxSpeed && Math.Abs(target.Destination) < 200f)
					{
						float arg_4F0_0 = texturePosition.xMax - 70f;
						float arg_4E5_0 = 38f;
						float expr_4D9 = num;
						num = expr_4D9 + 1f;
						GUI.Label(new Rect(arg_4F0_0, arg_4E5_0 + expr_4D9 * (float)num2, 70f, 20f), string.Format("vZ:{0:f2}", target.SpeedZ), Styles.guiStyleRedLabelNormal);
					}
					else
					{
						float arg_54A_0 = texturePosition.xMax - 70f;
						float arg_53F_0 = 38f;
						float expr_533 = num;
						num = expr_533 + 1f;
						GUI.Label(new Rect(arg_54A_0, arg_53F_0 + expr_533 * (float)num2, 70f, 20f), string.Format("vZ:{0:f2}", target.SpeedZ), Styles.guiStyleLabelWhiteNormal);
					}
					num += 0.2f;
					float arg_5AA_0 = texturePosition.xMax - 70f;
					float arg_59F_0 = 40f;
					float expr_593 = num;
					num = expr_593 + 1f;
					GUI.Label(new Rect(arg_5AA_0, arg_59F_0 + expr_593 * (float)num2, 70f, 20f), string.Format("Yaw:{0:f0}°", target.AngleX));
					float arg_5FD_0 = texturePosition.xMax - 70f;
					float arg_5F2_0 = 40f;
					float expr_5E6 = num;
					num = expr_5E6 + 1f;
					GUI.Label(new Rect(arg_5FD_0, arg_5F2_0 + expr_5E6 * (float)num2, 70f, 20f), string.Format("Pitch:{0:f0}°", target.AngleY));
					GUI.Label(new Rect(texturePosition.xMax - 70f, 40f + num * (float)num2, 70f, 20f), string.Format("Roll:{0:f0}°", target.AngleZ));
				}
			}
		}
#else
        private void GetFlightData()
        {
            if (cameraData)
            {
                if (TargetHelper.IsTargetSelect && part.vessel.Equals(FlightGlobals.ActiveVessel))
                {
                    /// DATA block
                    /// <summary>
                    float i = 0f;
                    target.Update();
                    if (!target.IsDockPort)
                    {
                        GUI.Label(new Rect(texturePosition.xMin + 2f, 34f, 100f, 40f), "Target is not\n a DockPort");
                        if (target.Destination < 200f)
                        {
                            GUI.Label(new Rect(texturePosition.xMin + 2f, 68f, 100f, 40f), "DockPort is\n available", Styles.guiStyleGreenLabelSmall);
                        }
                    }
                    else
                    {
                        GUI.Label(new Rect(texturePosition.xMin + 2f, 34f, 100f, 40f), "DockPort captured", Styles.guiStyleGreenLabelStandart);
                    }

                    string str = (Math.Abs(target.Destination) < 1000f) ? "{0:f2}" : "{0:f0}";

                    GUI.Label(new Rect(texturePosition.xMax - 70f, 34f + i++ * 16f, 70f, 20f),
                        string.Format("Dist:" + str, target.Destination), Styles.guiStyleLabelWhiteBold);
                    i += 0.2f;

                    GUI.Label(new Rect(texturePosition.xMax - 70f, 34f + i++ * 16f, 70f, 20f),
                        string.Format("dX:" + str, target.DX));
                    GUI.Label(new Rect(texturePosition.xMax - 70f, 34f + i++ * 16f, 70f, 20f),
                        string.Format("dY:" + str, target.DY));
                    GUI.Label(new Rect(texturePosition.xMax - 70f, 34f + i++ * 16f, 70f, 20f),
                        string.Format("dZ:" + str, target.DZ));

                    i += 0.2f;
                    if (Math.Abs(target.SpeedX) > maxSpeed && Math.Abs(target.Destination) < 200f)
                    {
                        GUI.Label(new Rect(texturePosition.xMax - 70f, 38f + i++ * 16f, 70f, 20f),
                            string.Format("vX:{0:f2}", target.SpeedX), Styles.guiStyleRedLabelNormal);
                    }
                    else
                    {
                        GUI.Label(new Rect(texturePosition.xMax - 70f, 38f + i++ * 16f, 70f, 20f),
                            string.Format("vX:{0:f2}", target.SpeedX), Styles.guiStyleLabelWhiteNormal);
                    }

                    if (Math.Abs(target.SpeedY) > maxSpeed && Math.Abs(target.Destination) < 200f)
                    {
                        GUI.Label(new Rect(texturePosition.xMax - 70f, 38f + i++ * 16f, 70f, 20f),
                            string.Format("vY:{0:f2}", target.SpeedY), Styles.guiStyleRedLabelNormal);
                    }
                    else
                    {
                        GUI.Label(new Rect(texturePosition.xMax - 70f, 38f + i++ * 16f, 70f, 20f),
                            string.Format("vY:{0:f2}", target.SpeedY), Styles.guiStyleLabelWhiteNormal);
                    }
                    if (Math.Abs(target.SpeedZ) > maxSpeed && Math.Abs(target.Destination) < 200f)
                    {
                        GUI.Label(new Rect(texturePosition.xMax - 70f, 38f + i++ * 16f, 70f, 20f),
                            string.Format("vZ:{0:f2}", target.SpeedZ), Styles.guiStyleRedLabelNormal);
                    }
                    else
                    {
                        GUI.Label(new Rect(texturePosition.xMax - 70f, 38f + i++ * 16f, 70f, 20f),
                            string.Format("vZ:{0:f2}", target.SpeedZ), Styles.guiStyleLabelWhiteNormal);
                    }
                    i += 0.2f;

                    GUI.Label(new Rect(texturePosition.xMax - 70f, 40f + i++ * 16f, 70f, 20f),
                        string.Format("Yaw:{0:f0}°", target.AngleX));
                    GUI.Label(new Rect(texturePosition.xMax - 70f, 40f + i++ * 16f, 70f, 20f),
                        string.Format("Pitch:{0:f0}°", target.AngleY));
                    GUI.Label(new Rect(texturePosition.xMax - 70f, 40f + i * 16f, 70f, 20f),
                        string.Format("Roll:{0:f0}°", target.AngleZ));
                }
            }
        }
#endif

        public override void Activate()
		{
			if (!IsActive)
			{
				SetFreeId();
				windowPosition.x = (windowPosition.width * (float)(id - 1));
				windowPosition.y = (400f);
				base.Activate();
			}
		}

		public override void Deactivate()
		{
            if (!IsActive)
                return;
            if (DockingCamera.usedId != null)
			    DockingCamera.usedId.Remove(id);
			base.Deactivate();
		}

		private void SetFreeId()
		{
			for (int i = 1; i < 2147483647; i++)
			{
				if ( !DockingCamera.usedId.Contains(i))
				{
					id = i;
					DockingCamera.usedId.Add(i);
					break;
				}
			}
		}

		public void UpdateNoise() //whitenoise
        {
			idTextureNoise++;
			if ( idTextureNoise >= 4)
			{
				idTextureNoise = 0;
			}
		}

		public override void Update()
		{
			base.UpdateWhiteNoise();
			allCamerasGameObject.Last<GameObject>().transform.position = (moduleDockingNodeGameObject.transform.position);
			allCamerasGameObject.Last<GameObject>().transform.rotation = (moduleDockingNodeGameObject.transform.rotation);

			allCamerasGameObject[0].transform.rotation = (allCamerasGameObject.Last<GameObject>().transform.rotation);
			allCamerasGameObject[1].transform.rotation = (allCamerasGameObject.Last<GameObject>().transform.rotation);
			allCamerasGameObject[2].transform.rotation = (allCamerasGameObject.Last<GameObject>().transform.rotation);
			allCamerasGameObject[2].transform.position = (allCamerasGameObject.Last<GameObject>().transform.position);
			allCameras.ForEach(delegate(UnityEngine.Camera cam)
			{
				cam.fieldOfView = (CurrentZoom);
			});
		}
	}
}
