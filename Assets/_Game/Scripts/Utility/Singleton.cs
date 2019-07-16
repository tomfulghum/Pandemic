using UnityEngine;

// https://wiki.unity3d.com/index.php/Singleton
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    //**********************//
    //    Private Fields    //
    //**********************//

    private static bool destroyed = false;
    private static T instance;

    //******************//
    //    Properties    //
    //******************//

    public static T Instance
    {
        get {
            if (destroyed) {
                Debug.LogWarning("[Singleton] Instance '" + typeof(T) + "' already destroyed.");
                return null;
            }

            if (instance == null) {
                // Search for existing instance.
                instance = FindObjectOfType<T>();

                // Create new instance if one doesn't already exist.
                if (instance == null) {
                    // Need to create a new GameObject to attach the singleton to.
                    var singletonObject = new GameObject();
                    instance = singletonObject.AddComponent<T>();
                    singletonObject.name = typeof(T).ToString() + " (Singleton)";

                    // Make instance persistent.
                    DontDestroyOnLoad(singletonObject);
                }
            }

            return instance;
        }
    }

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void OnApplicationQuit()
    {
        destroyed = true;
    }

    private void OnDestroy()
    {
        destroyed = true;
    }
}