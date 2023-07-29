using UnityEngine;
using UnityEngine.SceneManagement;

public class Preloader : MonoBehaviour
{
    void Start() {
        Application.targetFrameRate = 60;
        SceneManager.LoadScene("MenuScene");
    }
}
