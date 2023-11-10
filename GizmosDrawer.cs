using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cephei.DebugPack
{
	public class GizmosDrawer : MonoBehaviour
	{
		private class ActionGizmos
		{
			private float _startTime;
			private float _timeAlive;

			private Action _action;

			public ActionGizmos(float startTime, float timeAlive, Action action)
			{
				_startTime = startTime;
				_timeAlive = timeAlive;
				_action = action;
			}

			public bool Invoke(float time)
			{
				float timeDelta = time - _startTime;

				if (timeDelta < _timeAlive)
					_action.Invoke();

				return timeDelta < _timeAlive;
			}
		}

		private List<ActionGizmos> _actions = new List<ActionGizmos>();

		private static GizmosDrawer Instance => _instance ??= new GameObject("GizmosDrawer");

		private static GizmosDrawer _instance;

		public static void AddAction(Action action, float timeAlive)
		{
			ActionGizmos actionGizmos = new ActionGizmos(Time.time, timeAlive, action);
			Instance.AddAction(actionGizmos);
		}

		public static void AddAction(Action action) => AddAction(action, Mathf.Infinity);

		private void AddAction(ActionGizmos action) => _actions.Add(action);

		private void OnDisable() => _actions.Clear();

		private void OnDrawGizmos()
		{
			for (var i = 0; i < _actions.Count; i++)
			{
				if (_actions[i].Invoke(Time.time) == false)
				{
					RemoveElementByIndex(i);
					i--;
				}
			}
		}

		private void RemoveElementByIndex(int index)
		{
			int lastIndex = _actions.Count - 1;
			ActionGizmos last = _actions[lastIndex];

			_actions[lastIndex] = _actions[index];
			_actions[index] = last;

			_actions.RemoveAt(lastIndex);
		}
	}
}

