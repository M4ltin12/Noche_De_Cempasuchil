using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlJuego : MonoBehaviour
{
    public int vidas = 3;
    public int puntuacion = 0;
    public int floresRecogidas = 0;
    public int floresParaGanar = 20;
    public float duracionPoder = 6f;

    public TMP_Text textoOfrenda;
    public TMP_Text textoVidas;
    public TMP_Text textoPuntos;

    public GameObject panelGameOver;
    public TMP_Text textoFinalGameOver;
    public GameObject panelVictoria;
    public TMP_Text textoFinalVictoria;

    public bool JuegoTerminado { get; private set; }
    public bool PoderActivo => Time.time < finPoder;
    public float TiempoPoderRestante => Mathf.Max(0f, finPoder - Time.time);

    private float tiempoInicio;
    private float finPoder;
    private int comboFantasmas;
    private int puntosFantasmaBase = 200;

    void Awake()
    {
        Time.timeScale = 1f;
    }

    void Start()
    {
        tiempoInicio = Time.time;

        if (panelGameOver != null)
            panelGameOver.SetActive(false);
        if (panelVictoria != null)
            panelVictoria.SetActive(false);

        // Generador puede crear el laberinto en Awake; por si acaso reintentamos.
        Laberinto lab = Laberinto.Instancia;
        if (lab != null && lab.FloresTotales > 0)
            floresParaGanar = lab.FloresTotales;

        ActualizarUI();
    }

    bool ofrendaSincronizada;

    void LateUpdate()
    {
        if (ofrendaSincronizada || JuegoTerminado)
            return;

        Laberinto lab = Laberinto.Instancia;
        if (lab != null && lab.FloresTotales > 0)
        {
            floresParaGanar = lab.FloresTotales;
            ofrendaSincronizada = true;
            ActualizarUI();
        }
    }

    public void SumarFlor(int puntos = 1)
    {
        if (JuegoTerminado)
            return;

        floresRecogidas++;
        puntuacion += puntos;
        ActualizarUI();

        if (floresRecogidas >= floresParaGanar)
        {
            Laberinto laberinto = Laberinto.Instancia;
            if (laberinto != null && laberinto.HayOtraNoche)
                PasarNoche();
            else
                Victoria();
        }
    }

    public void SumarPuntos(int puntos)
    {
        if (JuegoTerminado)
            return;

        puntuacion += puntos;
        ActualizarUI();
    }

    public void ActivarPoder()
    {
        if (JuegoTerminado)
            return;

        finPoder = Time.time + duracionPoder;
        comboFantasmas = 0;

        Fantasma[] fantasmas = FindObjectsByType<Fantasma>(FindObjectsInactive.Exclude);
        for (int i = 0; i < fantasmas.Length; i++)
            fantasmas[i].EntrarAsustado();
    }

    public void SumarPuntosFantasma()
    {
        comboFantasmas++;
        int puntos = puntosFantasmaBase * (1 << Mathf.Min(comboFantasmas - 1, 3));
        puntuacion += puntos;
        ActualizarUI();
    }

    public void PerderVida()
    {
        if (JuegoTerminado)
            return;

        vidas--;
        finPoder = 0f;
        comboFantasmas = 0;
        ActualizarUI();

        if (vidas <= 0)
            GameOver();
        else
            ResetearPosiciones();
    }

    void ResetearPosiciones()
    {
        Laberinto lab = Laberinto.Instancia;
        Catrina catrina = FindAnyObjectByType<Catrina>();

        if (catrina != null && lab != null)
            catrina.GetComponent<Rigidbody2D>().position = lab.PosicionJugadorInicio();

        Fantasma[] fantasmas = FindObjectsByType<Fantasma>(FindObjectsInactive.Exclude);
        Vector3 nicho = lab != null ? lab.PosicionNicho() : Vector3.zero;

        for (int i = 0; i < fantasmas.Length; i++)
            fantasmas[i].ReiniciarEnNicho(nicho + new Vector3((i - 1.5f) * 0.35f, 0f, 0f));
    }

    void ActualizarUI()
    {
        if (textoPuntos != null)
        {
            if (textoOfrenda != null)
                textoOfrenda.text = TextoNoche() + "OFRENDA: " + floresRecogidas + " / " + floresParaGanar;
            textoPuntos.text = "PUNTOS: " + puntuacion;
        }
        else if (textoOfrenda != null)
        {
            textoOfrenda.text = TextoNoche() + "OFRENDA: " + floresRecogidas + " / " + floresParaGanar +
                "   ·   " + puntuacion + " pts";
        }

        if (textoVidas != null)
            textoVidas.text = "VIDAS: " + Mathf.Max(vidas, 0);
    }

    void Update()
    {
        if (textoPuntos != null && PoderActivo)
            textoPuntos.color = Color.Lerp(
                new Color(1f, 0.85f, 0.4f),
                new Color(0.5f, 0.7f, 1f),
                Mathf.PingPong(Time.time * 3f, 1f));
        else if (textoPuntos != null)
            textoPuntos.color = Color.white;
    }

    string TextoNoche()
    {
        Laberinto laberinto = Laberinto.Instancia;
        if (laberinto == null)
            return "";

        return "NOCHE " + (laberinto.Noche + 1) + "/" + laberinto.TotalNoches + "   ";
    }

    void PasarNoche()
    {
        Laberinto laberinto = Laberinto.Instancia;
        if (laberinto == null)
            return;

        laberinto.CargarSiguiente();
        floresRecogidas = 0;
        floresParaGanar = Mathf.Max(1, laberinto.FloresTotales);
        ofrendaSincronizada = true;
        finPoder = 0f;
        comboFantasmas = 0;

        if (RastroCempasuchil.Instancia != null)
            RastroCempasuchil.Instancia.Limpiar();

        Catrina catrina = FindAnyObjectByType<Catrina>();
        if (catrina != null)
            catrina.Reubicar();

        Fantasma[] fantasmas = FindObjectsByType<Fantasma>(FindObjectsInactive.Exclude);
        Vector3 nicho = laberinto.PosicionNicho();

        for (int i = 0; i < fantasmas.Length; i++)
        {
            fantasmas[i].AjustarNoche(laberinto.Noche);
            fantasmas[i].ReiniciarEnNicho(nicho + new Vector3((i - 1.5f) * 0.35f, 0f, 0f));
        }

        ActualizarUI();
    }

    void Victoria()
    {
        JuegoTerminado = true;

        int segundos = Mathf.RoundToInt(Time.time - tiempoInicio);

        panelVictoria.SetActive(true);
        textoFinalVictoria.text =
            "¡Completaste las " + (Laberinto.Instancia != null ? Laberinto.Instancia.TotalNoches : 3) + " noches en " + segundos + " segundos!\n" +
            "Puntos: " + puntuacion + "\n" +
            "Las almas pueden volver a casa esta noche.";

        Time.timeScale = 0f;
    }

    void GameOver()
    {
        JuegoTerminado = true;

        panelGameOver.SetActive(true);
        textoFinalGameOver.text =
            "Los fantasmas te alcanzaron.\n" +
            "OFRENDA: " + floresRecogidas + " / " + floresParaGanar + "\n" +
            "Puntos: " + puntuacion;

        Time.timeScale = 0f;
    }

    public void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void IrMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
