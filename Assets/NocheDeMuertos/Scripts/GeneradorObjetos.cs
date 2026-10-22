using UnityEngine;

/// <summary>
/// Crea el laberinto (si falta) y spawnea a Pedro, Rosa, Tito y Luna en el nicho.
/// </summary>
public class GeneradorObjetos : MonoBehaviour
{
    public GameObject prefabFlor;
    public GameObject prefabFantasma;
    public GameObject prefabCalaverita;

    // Campos viejos: Unity los ignora si ya no se usan; se mantienen por compatibilidad de escena.
    public int floresIniciales = 3;
    public int maxFlores = 4;
    public float intervaloFlor = 1.2f;
    public int fantasmasIniciales = 2;
    public int maxFantasmas = 9;
    public float intervaloFantasma = 7f;
    public float intervaloFantasmaMinimo = 3f;
    public float reduccionIntervalo = 0.4f;
    public float intervaloCalaverita = 12f;
    public float distanciaMinimaJugador = 2.5f;
    public float margen = 0.4f;

    static readonly TipoFantasma[] Tipos =
    {
        TipoFantasma.Pedro,
        TipoFantasma.Rosa,
        TipoFantasma.Tito,
        TipoFantasma.Luna
    };

    void Awake()
    {
        PixelArte.PrepararCamara();
        LimpiarDecoracionLibre();

        if (GetComponent<RastroCempasuchil>() == null)
            gameObject.AddComponent<RastroCempasuchil>();

        // FindAnyObjectByType ve el componente aunque su Awake aún no haya corrido.
        if (FindAnyObjectByType<Laberinto>() == null)
            CrearLaberintoRuntime();
    }

    static void LimpiarDecoracionLibre()
    {
        GameObject panteon = GameObject.Find("Panteon");
        if (panteon == null)
            return;

        Transform raiz = panteon.transform;
        for (int i = raiz.childCount - 1; i >= 0; i--)
        {
            Transform hijo = raiz.GetChild(i);
            string nombre = hijo.name;
            if (nombre.Contains("Papel") || nombre.Contains("Luz") || nombre.Contains("Global"))
                continue;

            hijo.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        if (prefabFantasma == null)
            return;

        Laberinto lab = Laberinto.Instancia;
        Vector3 nicho = lab != null ? lab.PosicionNicho() : Vector3.zero;

        for (int i = 0; i < Tipos.Length; i++)
        {
            Vector3 offset = new Vector3((i - 1.5f) * 0.35f, 0f, 0f);
            GameObject go = Instantiate(prefabFantasma, nicho + offset, Quaternion.identity);
            Fantasma fantasma = go.GetComponent<Fantasma>();

            if (fantasma != null)
                fantasma.Configurar(Tipos[i], nicho + offset);
        }
    }

    void CrearLaberintoRuntime()
    {
        GameObject objeto = new GameObject("Laberinto");
        objeto.SetActive(false);

        Laberinto lab = objeto.AddComponent<Laberinto>();
        lab.prefabFlor = prefabFlor;
        lab.prefabCalaverita = prefabCalaverita;
        lab.spriteMuro = BuscarSpriteMuro();
        lab.tamanoCelda = PixelArte.Celda;

        objeto.SetActive(true); // Awake construye el mapa con los prefabs ya asignados.
    }

    static Sprite BuscarSpriteMuro()
    {
        SpriteRenderer[] renderers = FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Exclude);

        for (int i = 0; i < renderers.Length; i++)
        {
            Sprite sprite = renderers[i].sprite;
            if (sprite != null && sprite.name.Contains("Lapida"))
                return sprite;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            Sprite sprite = renderers[i].sprite;
            if (sprite != null && sprite.name.Contains("Cruz"))
                return sprite;
        }

        return null;
    }
}
