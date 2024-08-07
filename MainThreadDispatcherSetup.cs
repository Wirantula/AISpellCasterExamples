using UnityEngine;

public class MainThreadDispatcherSetup : MonoBehaviour
{
	private void Awake()
	{
		if( FindObjectOfType<UnityMainThreadDispatcher>() == null )
		{
			var dispatcherObject = new GameObject( "UnityMainThreadDispatcher" );
			dispatcherObject.AddComponent<UnityMainThreadDispatcher>();
			DontDestroyOnLoad( dispatcherObject );
		}
	}
}
