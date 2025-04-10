using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnZone : MonoBehaviour
{
    public float fallThreshold = -10f; // Batas jatuh

    private void Update()
    {
        if (transform.position.y < fallThreshold)
        {
            // Restart level
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
    }
}
