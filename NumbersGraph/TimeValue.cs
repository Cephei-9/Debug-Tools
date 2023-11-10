using System;

namespace Cephei.DebugPack
{
	[Serializable]
	public class TimeValue
	{
		public float Time;
		public float Value;

		public TimeValue(float time, float value)
		{
			Time = time;
			Value = value;
		}
	}
}