using System;
using UnityEngine;

namespace OLDD_camera
{
    /// <summary>
    /// Extended information about the selected target
    /// </summary>
    public class TargetHelper
	{
        /// <summary>
        /// Object of comparison
        /// </summary>
        private readonly GameObject self;
		private readonly Part selfPart;
		public float DX;
		public float DY;
		public float DZ;
		public float SpeedX;
		public float SpeedY;
		public float SpeedZ;
		public float AngleX;
		public float AngleY;
		public float AngleZ;
		public float Destination;
		public bool IsMoveToTarget;
		public float SecondsToDock;
		public bool LookForward;
		public float TargetMoveHelpX;
		public float TargetMoveHelpY;

		public static ITargetable Target
		{
			get
			{
				return FlightGlobals.fetch.VesselTarget;
			}
		}

		private Transform targetTransform
		{
			get
			{
				return Target.GetTransform();
			}
		}

		public static bool IsTargetSelect
		{
			get
			{
				return Target != null && (Target as ModuleDockingNode != null || Target as Vessel != null);
			}
		}

		public bool IsDockPort
		{
			get
			{
				return Target is ModuleDockingNode;
			}
		}

		public TargetHelper(Part from)
		{
			selfPart = from;
			self = selfPart.gameObject;
		}

		public void Update()
		{
			UpdatePosition();
			Vector3 velocity = UpdateSpeed();
			Destination = (float)Math.Sqrt(Math.Pow((double)DX, 2.0) + Math.Pow((double)DY, 2.0) + Math.Pow((double)DZ, 2.0));
			UpdateAngles();
			UpdateIsMoveToTarget(velocity);
			UpdateTargetMoveHelp();
		}

		private void UpdatePosition()
		{
			DX = targetTransform.position.x - self.transform.position.x;
			DY = targetTransform.position.y - self.transform.position.y;
			DZ = targetTransform.position.z - self.transform.position.z;
		}

		private void UpdateIsMoveToTarget(Vector3 velocity)
		{
            bool flag = false;

			try
			{
				SecondsToDock = Destination / velocity.magnitude;
				flag = true;
			}
			catch (DivideByZeroException)
			{
				IsMoveToTarget = false;
			}
			if ( flag)
			{
                float num;

                if ( SpeedX == 0f && Mathf.Abs(DX) < 0.5f)
				{
					num = SecondsToDock;
				}
				else
				{
					num = ((Mathf.Abs(DX) < 0.5f) ? SecondsToDock : (-DX / SpeedX));
				}
                float num2;
                if ( SpeedY == 0f && Mathf.Abs(DY) < 0.5f)
				{
					num2 = SecondsToDock;
				}
				else
				{
					num2 = ((Mathf.Abs(DY) < 0.5f) ? SecondsToDock : (-DY / SpeedY));
				}
				IsMoveToTarget = (Mathf.Abs(SecondsToDock - num) < 1f && Mathf.Abs(SecondsToDock - num2) < 1f && DZ * SpeedZ < 0f);
			}
		}

		private void UpdateAngles()
		{
			AngleX = SignedAngleAroundVector(-targetTransform.forward, self.transform.up, -self.transform.forward);
			AngleY = SignedAngleAroundVector(-targetTransform.forward, self.transform.up, self.transform.right);
			AngleZ = SignedAngleAroundVector(targetTransform.up, -self.transform.forward, -self.transform.up);
		}

		private Vector3 UpdateSpeed()
		{
			Vector3 vector = Target.GetObtVelocity() - selfPart.vessel.GetObtVelocity();
			SpeedX = vector.x;
			SpeedY = vector.y;
			SpeedZ = vector.z;
			return vector;
		}

		private void UpdateTargetMoveHelp()
		{
			Vector3 a = self.transform.position - targetTransform.position;
			Vector2 vector = new Vector2(SignedAngleAroundVector(a, targetTransform.forward.normalized, self.transform.forward), SignedAngleAroundVector(a, targetTransform.forward.normalized, -self.transform.right));
			LookForward = (Math.Abs(vector.x) < 90f);
			float value = (float)(LookForward ? 1 : -1) * (vector.x / 90f % 2f);
			float value2 = (float)(LookForward ? 1 : -1) * (vector.y / 90f % 2f);
			float exponent = 0.75f;
			if ( Destination <= 5f)
			{
				exponent = 1f;
			}
			else
			{
				if ( Destination < 15f)
				{
					float num = Destination - 5f;
					float num2 = 10f;
					float num3 = num / num2;
					float num4 = 0.25f;
					exponent = 1f - num4 * num3;
				}
			}
			TargetMoveHelpX = (ScaleExponentially(value, exponent) + 1f) / 2f;
			TargetMoveHelpY = (ScaleExponentially(value2, exponent) + 1f) / 2f;
		}

		private static float SignedAngleAroundVector(Vector3 a, Vector3 b, Vector3 c)
		{
			Vector3 vector = Vector3.Cross(c, a);
			Vector3 vector2 = Vector3.Cross(c, b);
            float result;
            if ( Vector3.Dot(Vector3.Cross(vector, vector2), c) < 0f)
			{
				result = -Vector3.Angle(vector, vector2);
			}
			else
			{
				result = Vector3.Angle(vector, vector2);
			}
			return result;
		}

		private static float ScaleExponentially(float value, float exponent)
		{
			return (float)Math.Pow((double)Math.Abs(value), (double)exponent) * (float)Math.Sign(value);
		}
	}
}
