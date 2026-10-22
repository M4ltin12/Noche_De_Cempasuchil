using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuPausa : MonoBehaviour
{
    public GameObject panelPausa;

    private bool pausado = false;
    private ControlJuego control;

    void Start()
    {
        control = FindAnyObjectByType<ControlJuego>();
        panelPausa.SetActive(false);
    }

    void Update()
    {
        if (control != null && control.JuegoTerminado)
            return;

        Keyboard teclado = Keyboard.current;

        if (teclado != null &&
            (teclado.escapeKey.wasPressedThisFrame || teclado.pKey.wasPressedThisFrame))
        {
            if (pausado)
                Continuar();
            else
                Pausar();
        }
    }

    public void Pausar()
    {
        panelPausa.SetActive(true);

        Time.timeScale = 0f;

        pausado = true;
    }

    public void Continuar()
    {
        panelPausa.SetActive(false);

        Time.timeScale = 1f;

        pausado = false;
    }

    public void IrMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("MainMenu");
    }
}
