using UnityEngine;
using UnityEngine.SceneManagement;

public class Preloader : MonoBehaviour
{
    void Start() {
        SceneManager.LoadScene("MenuScene");
    }
}
