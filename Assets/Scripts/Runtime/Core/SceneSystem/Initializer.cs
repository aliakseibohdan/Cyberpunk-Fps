using UnityEngine;

public class Initializer : MonoBehaviour
{
    private static bool _isInitialized = false;
    private static GameObject _persistObjectsPrefab;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Execute()
    {
        if (_isInitialized) return;

        _isInitialized = true;

        if (_persistObjectsPrefab == null)
        {
            _persistObjectsPrefab = Resources.Load<GameObject>("PERSISTOBJECTS");
        }

        if (_persistObjectsPrefab != null)
        {
            GameObject persistInstance = Instantiate(_persistObjectsPrefab);
            DontDestroyOnLoad(persistInstance);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("Loaded by the Persist Objects from the Initializer script");
#endif
        }
    }
}