using System;
using UnityEngine;

namespace OLDD_camera
{
	internal class CameraShaders
	{
		private static Material oldtv = null;
		private static Material grayscale = null;
		private static Material nightvisionclear = null;
		private static Material nightvisionnoise1 = null;
		private static Material noise = null;
		private static Material noisenightvision = null;

		public static Material OldTV
		{
			get
			{
				if ( oldtv == null)
				{
					oldtv = AssetLoader.matOldTV;
				}
				return oldtv;
			}
		}

		public static Material Grayscale
		{
			get
			{
				if ( grayscale == null)
				{
					grayscale = AssetLoader.matGrayscale;
				}
				return grayscale;
			}
		}

		public static Material NightVisionClear
		{
			get
			{
				if ( nightvisionclear == null)
				{
					nightvisionclear = AssetLoader.matNightVisionClear;
				}
				return nightvisionclear;
			}
		}

		public static Material NightVisionNoise1
		{
			get
			{
				if ( nightvisionnoise1 == null)
				{
					nightvisionnoise1 = AssetLoader.matNightVisionNoise1;
				}
				return nightvisionnoise1;
			}
		}

		public static Material Noise
		{
			get
			{
				if ( noise == null)
				{
					noise = AssetLoader.matNoise;
				}
				return noise;
			}
		}

		public static Material NoiseNightVision
		{
			get
			{
				if ( noisenightvision == null)
				{
					noisenightvision = AssetLoader.matNoiseNightVision;
				}
				return noisenightvision;
			}
		}

		public static Material GetShader(ShaderType type)
		{
			Material result;
			switch (type)
			{
			case ShaderType.OldTV:
				result = OldTV;
				break;
			case ShaderType.NightVisionNoise1:
				result = NightVisionNoise1;
				break;
			case ShaderType.Noise:
				result = Noise;
				break;
			case ShaderType.NoiseNightVision:
				result = NoiseNightVision;
				break;
			case ShaderType.NightVisionClear:
				result = NightVisionClear;
				break;
			case ShaderType.Grayscale:
				result = Grayscale;
				break;
			case ShaderType.None:
				result = null;
				break;
			default:
				result = null;
				break;
			}
			return result;
		}

		public static Material GetShader1(ShaderType1 type)
		{
			Material result;
			if (type != ShaderType1.OldTV)
			{
				if (type != ShaderType1.NightVisionNoise1)
				{
					result = null;
				}
				else
				{
					result = NightVisionNoise1;
				}
			}
			else
			{
				result = OldTV;
			}
			return result;
		}

		public static Material GetShader2(ShaderType2 type)
		{
			Material result;
			switch (type)
			{
			case ShaderType2.None:
				result = null;
				break;
			case ShaderType2.Grayscale:
				result = Grayscale;
				break;
			case ShaderType2.NightVisionClear:
				result = NightVisionClear;
				break;
			default:
				result = null;
				break;
			}
			return result;
		}
	}
}
