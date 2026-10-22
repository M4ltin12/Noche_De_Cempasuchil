using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    void Awake()
    {
        Time.timeScale = 1f;
    }

    public void Jugar()
    {
        SceneManager.LoadScene("Juego");
    }

    public void Salir()
    {
        Application.Quit();

        Debug.Log("Salir del juego");
    }
}
