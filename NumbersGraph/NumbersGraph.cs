using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;

namespace Cephei.DebugPack
{
	[ExecuteAlways]
	public class NumbersGraph : MonoBehaviour
	{
		private static readonly string DirectoryPath = "/CepheiDebugPack/Tools/NumbersGraph/GraphSaves";
		private static string FullPath => FileExtensions.AssetPath + DirectoryPath;

		public bool RemoveAll;
		public List<Graph> Graphs = new List<Graph>();

        private static NumbersGraph Instance => _instance ??= new GameObject("NumbersGraph");

		private static NumbersGraph _instance;

        public void WatchNumber(string label, float value) =>
			WatchNumber(label, Time.time, value);

		public static void WatchNumber(string label, float time, float value)
		{
			TimeValue timeValue = new TimeValue(time, value);
			Graph graph = Instance.Graphs.FirstOrDefault(x => x.Label == label);

			if (graph != null)
				graph.AddValue(timeValue);
			else
				CreateNewGraph(label, timeValue);
		}

		private void Reboot()
		{
			Graphs.Clear();
			DeleteAllMeta();

			FileInfo[] allFiles = GetAllFiles("*.txt");
			foreach (var fileInfo in allFiles)
			{
				Graphs.Add(new Graph(fileInfo));
			}
		}

		private static FileInfo[] GetAllFiles(string extension)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(FullPath);
			return directoryInfo.GetFiles(extension);
		}

		private static void CreateNewGraph(string label, TimeValue timeValue)
		{
			FileInfo file = new FileInfo(FullPath + "/" + label + ".txt");
			Instance.Graphs.Add(new Graph(label, timeValue, file));
		}

		private void OnValidate()
		{
			if (Graphs.Count == 0)
				return;

			if (HandleRename()) ;
			else if (HandleAllRemove()) ;
			else if(HandleRemoveSomeGraph());

			Reboot();
		}

		private bool HandleRename()
		{
			if (Graphs[0].File == null)
				return false;
			
			foreach (Graph graph in Graphs)
			{
				if (graph.IsChangeName)
				{
					TryRenameGraph(graph);
					return true;
				}
			}
			
			return false;
		}

		private void TryRenameGraph(Graph graph)
		{
			if (GetAllFiles("*.txt").All(x => x.GetNameWithoutExtension() != graph.Label))
			{
				string newPath = GetFilePath(graph.Label);
				graph.File.MoveTo(newPath);
			}
		}

		private static string GetFilePath(string label) => 
			FullPath + "/" + label + ".txt";

		private bool HandleAllRemove()
		{
			if (RemoveAll)
			{
				RemoveAll = false;

				Graphs.Clear();
				DeleteAllFiles();
				
				return true;
			}

			return false;
		}

		private static void DeleteAllFiles()
		{
			FileInfo[] allFiles = GetAllFiles("*.txt");

			foreach (FileInfo fileInfo in allFiles)
			{
				fileInfo.Delete();
			}
		}

		private void DeleteAllMeta()
		{
			FileInfo[] allFiles = GetAllFiles("*.meta");

			foreach (FileInfo fileInfo in allFiles)
			{
				fileInfo.Delete();
			}
		}

		private bool HandleRemoveSomeGraph()
		{
			for (var index = 0; index < Graphs.Count; index++)
			{
				Graph graph = Graphs[index];
				
				if (graph.Remove)
				{
					graph.File.Delete();
					RemoveElementByIndex(Graphs, index);
					
					return true;
				}
			}

			return false;
		}

		private void RemoveElementByIndex<T>(List<T> ts, int index)
		{
			int lastIndex = ts.Count - 1;
			T lastElement = ts[lastIndex];

			ts[lastIndex] = ts[index];
			ts[index] = lastElement;

			ts.RemoveAt(lastIndex);
		}
	}
}