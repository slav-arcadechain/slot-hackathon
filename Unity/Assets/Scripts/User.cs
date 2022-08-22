using System.Collections;
using UnityEngine;


public class User : MonoBehaviour
{
    #region Internal Methods

    public int walletTokenBalance;
    public int approvedTokenBalance;
    bool quitting = false;

    private void Awake()
    {
        Debug.Log("Loading planets...");
        StartCoroutine(LoadInternal());
    }
 
    private IEnumerator LoadInternal()
    {
        var path = Application.streamingAssetsPath + "/planets.json";
        // using (var www = new WWW(path))
        // {
        //     yield return new WaitForSeconds(5); // Pretend the network is slow
        //     yield return www;
        //     planets = JsonUtility.FromJson<Planets>(www.text).planets;
        // }
        yield break;
    }
 
 
    private void OnApplicationQuit()
    {
        quitting = true;
    }
 
    private void OnDestroy()
    {
        if (!quitting)
        {
            instance = null;
            Init();
        }
    }
 
    #endregion
 
    #region Instance
 
    private static User instance;
    private static User Instance
    {
        get
        {
            Init();
            return instance;
        }
    }
 
    [RuntimeInitializeOnLoadMethod] // this enables eager loading
    private static void Init()
    {
        if (instance == null || instance.Equals(null))
        {
            var gameObject = new GameObject("User");
            gameObject.hideFlags = HideFlags.HideAndDontSave; //hides from Unity editor
 
            instance = gameObject.AddComponent<User>();
            DontDestroyOnLoad(gameObject); //prevents destroy on changing scene 
        }
    }
 
    #endregion
}