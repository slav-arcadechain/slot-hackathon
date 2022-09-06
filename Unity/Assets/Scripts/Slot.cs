using UnityEngine;

public class Slot : MonoBehaviour
{
    public const int GameFee = 10;
        
    bool _quitting = false;
 
    private void OnApplicationQuit()
    {
        _quitting = true;
    }
 
    private void OnDestroy()
    {
        if (!_quitting)
        {
            _instance = null;
            Init();
        }
    }
    #region Instance
 
    private static Slot _instance;
    private static Slot Instance
    {
        get
        {
            Init();
            return _instance;
        }
    }
 
    [RuntimeInitializeOnLoadMethod] // this enables eager loading
    private static void Init()
    {
        if (_instance == null || _instance.Equals(null))
        {
            var gameObject = new GameObject("Download");
            gameObject.hideFlags = HideFlags.HideAndDontSave; //hides from Unity editor
 
            _instance = gameObject.AddComponent<Slot>();
            DontDestroyOnLoad(gameObject); //prevents destroy on changing scene 
        }
    }
 
    #endregion
}