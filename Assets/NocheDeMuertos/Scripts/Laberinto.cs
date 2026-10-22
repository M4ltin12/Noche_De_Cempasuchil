using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Laberinto tipo Pac-Man del panteón: muros, pétalos fijos y puntos de spawn.
/// </summary>
public class Laberinto : MonoBehaviour
{
    public static Laberinto Instancia { get; private set; }

    public float tamanoCelda = PixelArte.Celda;
    public Vector2 origen = new Vector2(0f, -0.15f);
    public GameObject prefabFlor;
    public GameObject prefabCalaverita;
    public Sprite spriteMuro;

    public int Columnas { get; private set; }
    public int Filas { get; private set; }
    public Vector2Int CeldaJugador { get; private set; }
    public Vector2Int CeldaNicho { get; private set; }
    public int FloresTotales { get; private set; }

    public int Noche { get; private set; }
    public int TotalNoches => Mapas.Length;
    public bool HayOtraNoche => Noche + 1 < Mapas.Length;

    // # muro  . punto  o calaverita  - pasillo  P jugador  G nicho
    static readonly string[][] Mapas =
    {
        new[]
        {
            "#########################",
            "#...........#...........#",
            "#.###.###.#.#.#.###.###.#",
            "#o###.###.#.#.#.###.###o#",
            "#.......................#",
            "#.###.#.#########.#.###.#",
            "#.....#.....#.....#.....#",
            "#####.###.#---#.###.#####",
            "#.........#GGG#.........#",
            "#.###.###.##.##.###.###.#",
            "#o###...........###.###o#",
            "#.....###...P...###.....#",
            "#########################"
        },
        new[]
        {
            "#########################",
            "#o.........#...........o#",
            "#.###.###.#.#.#.###.###.#",
            "#.#.......#.#.#.......#.#",
            "#.#.###.#.#.#.#.#.###.#.#",
            "#...#...#...#...#...#...#",
            "#####.###.#---#.###.#####",
            "#.........#GGG#.........#",
            "#.###.###.##.##.###.###.#",
            "#.#...................#.#",
            "#o#.###.........###.#o#.#",
            "#...#.....#P#.....#...#.#",
            "#########################"
        },
        new[]
        {
            "#########################",
            "#o.....................o#",
            "#.#####.#########.#####.#",
            "#.#...................#.#",
            "#.#.#####.#.#.#####.#.#.#",
            "#.#.....#.#.#.#.....#.#.#",
            "#####.###.#---#.###.#####",
            "#.........#GGG#.........#",
            "#.###.#.#########.#.###.#",
            "#.....#.....#.....#.....#",
            "#o...................o#.#",
            "#.....###...P...###.....#",
            "#########################"
        }
    };

    bool[,] paredes;

    void Awake()
    {
        Instancia = this;
        Construir();
    }

    void OnDestroy()
    {
        if (Instancia == this)
            Instancia = null;
    }

    public void CargarSiguiente()
    {
        if (!HayOtraNoche)
            return;

        Noche++;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject hijo = transform.GetChild(i).gameObject;
            hijo.SetActive(false);
            Destroy(hijo);
        }

        Construir();
    }

    void Construir()
    {
        string[] mapa = Mapas[Mathf.Clamp(Noche, 0, Mapas.Length - 1)];
        Filas = mapa.Length;
        Columnas = mapa[0].Length;
        paredes = new bool[Columnas, Filas];

        Transform muros = new GameObject("Muros").transform;
        muros.SetParent(transform, false);

        Transform coleccionables = new GameObject("Coleccionables").transform;
        coleccionables.SetParent(transform, false);

        FloresTotales = 0;
        CeldaJugador = new Vector2Int(Columnas / 2, Filas - 2);
        CeldaNicho = new Vector2Int(Columnas / 2, 8);
        int sumaNicho = 0;
        int cuentaNicho = 0;
        int filaNicho = CeldaNicho.y;
        PixelArte.Asegurar();

        for (int f = 0; f < Filas; f++)
        {
            string fila = mapa[f];

            if (fila.Length != Columnas)
            {
                Debug.LogError("Laberinto: fila " + f + " tiene " + fila.Length + " chars, se esperaban " + Columnas);
                continue;
            }

            for (int c = 0; c < Columnas; c++)
                paredes[c, f] = fila[c] == '#';
        }

        for (int f = 0; f < Filas; f++)
        {
            string fila = mapa[f];

            for (int c = 0; c < Columnas && c < fila.Length; c++)
            {
                char ch = fila[c];
                Vector2Int celda = new Vector2Int(c, f);
                Vector3 mundo = CeldaAMundo(celda);

                if (ch == '#')
                {
                    CrearMuro(celda, mundo, muros);
                    continue;
                }

                if (ch == 'P')
                    CeldaJugador = celda;
                else if (ch == 'G')
                {
                    sumaNicho += c;
                    cuentaNicho++;
                    filaNicho = f;
                }

                if (ch == '.')
                {
                    if (prefabFlor != null)
                    {
                        GameObject flor = Instantiate(prefabFlor, mundo, Quaternion.identity, coleccionables);
                        PixelArte.VestirFlor(flor);
                        FloresTotales++;
                    }
                }
                else if (ch == 'o' && prefabCalaverita != null)
                {
                    GameObject cal = Instantiate(prefabCalaverita, mundo, Quaternion.identity, coleccionables);
                    PixelArte.VestirCalavera(cal);
                }
            }
        }

        if (cuentaNicho > 0)
            CeldaNicho = new Vector2Int(sumaNicho / cuentaNicho, filaNicho);
    }

    void CrearMuro(Vector2Int celda, Vector3 posicion, Transform padre)
    {
        GameObject muro = new GameObject("Muro");
        muro.transform.SetParent(padre, false);
        muro.transform.position = posicion;
        muro.transform.localScale = Vector3.one;

        bool bordeArriba = !EsPared(new Vector2Int(celda.x, celda.y - 1));
        bool bordeAbajo = !EsPared(new Vector2Int(celda.x, celda.y + 1));
        bool bordeIzquierda = !EsPared(new Vector2Int(celda.x - 1, celda.y));
        bool bordeDerecha = !EsPared(new Vector2Int(celda.x + 1, celda.y));

        SpriteRenderer renderer = muro.AddComponent<SpriteRenderer>();
        renderer.sprite = PixelArte.Muro(bordeArriba, bordeAbajo, bordeIzquierda, bordeDerecha);
        renderer.sortingOrder = -5;
        renderer.color = Color.white;
    }

    public Vector3 CeldaAMundo(Vector2Int celda)
    {
        float x = origen.x + (celda.x - (Columnas - 1) * 0.5f) * tamanoCelda;
        float y = origen.y + ((Filas - 1) * 0.5f - celda.y) * tamanoCelda;
        return new Vector3(x, y, 0f);
    }

    public Vector2Int MundoACelda(Vector2 mundo)
    {
        float cx = (mundo.x - origen.x) / tamanoCelda + (Columnas - 1) * 0.5f;
        float cy = (Filas - 1) * 0.5f - (mundo.y - origen.y) / tamanoCelda;
        return new Vector2Int(
            Mathf.Clamp(Mathf.RoundToInt(cx), 0, Columnas - 1),
            Mathf.Clamp(Mathf.RoundToInt(cy), 0, Filas - 1)
        );
    }

    public bool EsPared(Vector2Int celda)
    {
        if (celda.x < 0 || celda.y < 0 || celda.x >= Columnas || celda.y >= Filas)
            return true;

        return paredes[celda.x, celda.y];
    }

    public bool EsCaminable(Vector2Int celda) => !EsPared(celda);

    public bool PuedeAvanzar(Vector2Int desde, Vector2Int direccion)
    {
        if (direccion == Vector2Int.zero)
            return false;

        return EsCaminable(desde + direccion);
    }

    public List<Vector2Int> DireccionesLibres(Vector2Int celda, Vector2Int evitar = default)
    {
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        List<Vector2Int> libres = new List<Vector2Int>(4);

        for (int i = 0; i < dirs.Length; i++)
        {
            if (dirs[i] == evitar)
                continue;

            if (EsCaminable(celda + dirs[i]))
                libres.Add(dirs[i]);
        }

        // Si solo quedaba ir hacia atrás, permitir.
        if (libres.Count == 0 && evitar != Vector2Int.zero && EsCaminable(celda + evitar))
            libres.Add(evitar);

        return libres;
    }

    public bool CercaDelCentro(Vector2 posicion, float umbral = 0.15f)
    {
        Vector2Int celda = MundoACelda(posicion);
        Vector3 centro = CeldaAMundo(celda);
        return Vector2.Distance(posicion, centro) <= umbral;
    }

    public Vector3 PosicionJugadorInicio() => CeldaAMundo(CeldaJugador);

    public Vector3 PosicionNicho() => CeldaAMundo(CeldaNicho);
}
