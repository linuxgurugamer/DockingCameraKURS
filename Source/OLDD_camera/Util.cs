using OLDD_camera.Camera;
using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace OLDD_camera
{
    public enum ShaderType
    {
        OldTV,
        NightVisionNoise1,
        Noise,
        NoiseNightVision,
        NightVisionClear,
        Grayscale,
        None
    }

    public enum ShaderType1
    {
        OldTV,
        NightVisionNoise1
    }

    public enum ShaderType2
    {
        None,
        Grayscale,
        NightVisionClear
    }

    public enum ShaderType3
    {
        Noise,
        NoiseNightVision
    }

    /// <summary>
    /// Static class of utilities
    /// </summary>
    // UPDATE_112 START
    public delegate void UpdateGUIFunction();
    public class UpdateGUIObject : MonoBehaviour
    {
        public UpdateGUIFunction updateGUIFunction = null;
        //public UpdateGUIFunction awakeFunction;

        //void Awake()
        //{
        //    if (awakeFunction != null)
        //    {
        //        awakeFunction();
        //    }          
        //}
        void OnGUI()
        {
            if (updateGUIFunction != null)
            {
                updateGUIFunction();
            }
        }
    }
    // UPDATE_112 END

    public static class Util
	{
		private static string dataTexturePath = "DockingCam/";
		private static string PhotoDirectory = "Screenshots";

		public static Texture2D LoadTexture(string name)
		{
			return GameDatabase.Instance.GetTexture(Util.dataTexturePath + name, false);
		}

		public static Texture2D MonoColorRectTexture(Color color)
		{
			return Util.MonoColorTexture(color, 4, 4);
		}

		public static Texture2D MonoColorVerticalLineTexture(Color color, int size)
		{
			return Util.MonoColorTexture(color, 1, size);
		}

		public static Texture2D MonoColorHorizontalLineTexture(Color color, int size)
		{
			return Util.MonoColorTexture(color, size, 1);
		}

		public static Texture2D MonoColorTexture(Color color, int width, int height)
		{
			Texture2D texture2D = new Texture2D(width, height);
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					texture2D.SetPixel(i, j, color);
				}
			}
			texture2D.Apply();
			return texture2D;
		}

		public static Texture2D WhiteNoiseTexture(int width, int height, float alpha = 0.16f)
		{
			Color color = new Color(0f, 0f, 0f, alpha);
			Color color2 = new Color(1f, 1f, 1f, alpha);
			width *= 2;
			height *= 2;
			Texture2D texture2D = new Texture2D(width, height);
			Color[] array = new Color[width * height];
			for (int i = 0; i < width * height; i++)
			{
				array[i] = ((UnityEngine.Random.Range(0, 100) % 2 == 1) ? color : color2);
			}
			texture2D.SetPixels(array);
			texture2D.Apply();
			return texture2D;
		}

		public static void SavePng(this RenderTexture renderTexture, string photoFrom)
		{
			double time = Planetarium.fetch.time;
			string timeMark = Util.GetTimeMark(time);
			RenderTexture.active = (renderTexture);
			if ( Event.current.type.Equals(7))
			{
				Graphics.Blit(renderTexture, BaseKspCamera.CurrentShader);
			}
			Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height,  TextureFormat.RGB24, false);
			texture2D.ReadPixels(new Rect(0f, 0f, (float)renderTexture.width, (float)renderTexture.height), 0, 0);
			byte[] bytes = texture2D.EncodeToPNG();
			string text = string.Concat(new string[]
			{
				"Photo from ",
				photoFrom,
				" at UT ",
				timeMark,
				".png"
			});
			string text2 = Path.Combine(Util.PhotoDirectory, HighLogic.SaveFolder);
			Directory.CreateDirectory(text2);
			text = Path.Combine(text2, text);
			File.WriteAllBytes(text, bytes);
			ScreenMessages.PostScreenMessage("PHOTO HAS BEEN SAVED TO YOUR SCREENSHOTS FOLDER", 3f, 0);
		}

		public static string GetTimeMark(double universalTime)
		{
			double num = universalTime;
			StringBuilder stringBuilder = new StringBuilder();
			if ( num >= 9201600.0)
			{
				num = Util.Converter(num, stringBuilder, 9201600u, "y");
			}
			if ( num >= 21600.0)
			{
				num = Util.Converter(num, stringBuilder, 21600u, "d");
			}
			if ( num >= 3600.0)
			{
				num = Util.Converter(num, stringBuilder, 3600u, "h");
			}
			if ( num >= 60.0)
			{
				num = Util.Converter(num, stringBuilder, 60u, "m");
			}
			stringBuilder.Append(num.ToString("F0"));
			stringBuilder.Append("s");
			return stringBuilder.ToString();
		}

		private static double Converter(double time, StringBuilder timeMark, uint seconds, string suffix)
		{
			timeMark.Append(Math.Floor(time / seconds));
			timeMark.Append(suffix);
			return time % seconds;
		}

		public static Rect ConstrainToScreen(Rect r, int limit)
		{
			r.x = (Mathf.Clamp(r.x, (float)limit - r.width, (float)(Screen.width - limit)));
			r.y = (Mathf.Clamp(r.y, (float)limit - r.height, (float)(Screen.height - limit)));
			return r;
		}
	}
}
