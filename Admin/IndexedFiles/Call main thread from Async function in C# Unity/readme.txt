Create an empty GameObject, call it UnityMainThreadDispatcher.
Download the UnityMainThreadDispatcher.cs script and add it to your GameObject.
You can now dispatch objects to the main thread in Unity.

Usage example:
It must be in async funcion:
UnityMainThreadDispatcher.Instance().Enqueue(() => {Debug.Log ("This is executed from the main thread");});