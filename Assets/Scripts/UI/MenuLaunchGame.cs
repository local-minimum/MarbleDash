using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuLaunchGame : MonoBehaviour {

    public string lvlScene = "level";

    public void Launch()
    {
        SceneManager.LoadScene(lvlScene, LoadSceneMode.Single);
    }
}
