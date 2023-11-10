using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Cephei.DebugPack
{
	[Serializable]
	public class Graph
	{
		public string Label;
		
		[Header("Control")] 
		public bool Remove;

		[Header("View")] 
		public AnimationCurve Curve;
		[Space]
		public float MinValue;
		public float MidleValue;
		public float MaxValue;

		private List<TimeValue> _timeValues;
		public FileInfo File { get; }

		public bool IsChangeName => Label != File.GetNameWithoutExtension();

		public Graph(string label, TimeValue firstValue, FileInfo file)
		{
			Label = label;
			File = file;
			
			InitFromFirstValue(firstValue);
			AppendFile(firstValue);
		}

		public Graph(FileInfo fileInfo)
		{
			File = fileInfo;
			Label = fileInfo.GetNameWithoutExtension();
			
			RecoverFromFile(fileInfo);
		}

		private void RecoverFromFile(FileInfo fileInfo)
		{
			string[] allLines = System.IO.File.ReadAllLines(fileInfo.FullName);

			TimeValue firstValue = GetTimeValueFromLines(allLines, 0);
			InitFromFirstValue(firstValue);

			for (int i = 2; i < allLines.Length; i += 2)
			{
				TimeValue value = GetTimeValueFromLines(allLines, i);
				AddValue(value, false);
			}
		}

		private void InitFromFirstValue(TimeValue firstValue)
		{
			_timeValues = new List<TimeValue> { firstValue };
			Curve = new AnimationCurve(new Keyframe(firstValue.Time, firstValue.Value, 0, 0, 0, 0));

			MinValue = firstValue.Value;
			MaxValue = firstValue.Value;
			MidleValue = firstValue.Value;
		}

		private TimeValue GetTimeValueFromLines(string[] allLines, int fromIndex)
		{
			bool isTimeParse = float.TryParse(allLines[fromIndex], out float time);
			bool isValueParse = float.TryParse(allLines[fromIndex + 1], out float value);
			
			if((isTimeParse && isValueParse) == false)
				Debug.LogError("File is don't correctly");

			return new TimeValue(time, value);
		}

		public void AddValue(TimeValue timeValue, bool withAddedToFile = true)
		{
			_timeValues.Add(timeValue);
			
			if(withAddedToFile)
				AppendFile(timeValue);
			
			UpdateView(timeValue);
		}

		private void UpdateView(TimeValue timeValue)
		{
			Curve.AddKey(new Keyframe(timeValue.Time, timeValue.Value, 0, 0, 0, 0));

			MinValue = Mathf.Min(MinValue, timeValue.Value);
			MaxValue = Mathf.Max(MaxValue, timeValue.Value);

			MidleValue = CalculateMidleValue();
		}

		private float CalculateMidleValue() => 
			_timeValues.Sum(x => x.Value) / _timeValues.Count;

		private void AppendFile(TimeValue firstValue) => 
			System.IO.File.AppendAllText(File.FullName, $"{firstValue.Time}\n{firstValue.Value}\n");
	}
}