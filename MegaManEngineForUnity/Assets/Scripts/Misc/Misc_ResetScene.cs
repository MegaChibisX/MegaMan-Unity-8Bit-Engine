using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Resets the scene after some time.
/// </summary>
[AddComponentMenu("MegaMan/Misc/Reset Scene")]
public class Misc_ResetScene : MonoBehaviour {


    public float timeToReset = 5.0f;


    private void Update()
    {
        timeToReset -= Time.deltaTime;
        if (GameManager.preventDeathReset)
            timeToReset = 0.0001f;
        if (timeToReset <= 0.0f)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }


}
