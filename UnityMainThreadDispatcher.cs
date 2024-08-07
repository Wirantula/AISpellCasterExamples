using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
	private static readonly Queue<Action> executionQueue = new Queue<Action>();

	private void Update()
	{
		lock( executionQueue )
		{
			while( executionQueue.Count > 0 )
			{
				executionQueue.Dequeue().Invoke();
			}
		}
	}

	public static void Enqueue( Action action )
	{
		lock( executionQueue )
		{
			executionQueue.Enqueue( action );
		}
	}
}
