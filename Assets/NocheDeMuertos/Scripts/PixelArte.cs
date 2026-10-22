using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sprites de 16 px, filtro de punto y trajes de la Catrina y los fantasmas.
/// </summary>
public static class PixelArte
{
    public const float PixelesPorUnidad = 24f;
    public const float Celda = 16f / PixelesPorUnidad;

    static readonly Color Hueso = new Color(0.96f, 0.91f, 0.78f, 1f);
    static readonly Color Negro = new Color(0.07f, 0.03f, 0.08f, 1f);
    static readonly Color Magenta = new Color(0.90f, 0.16f, 0.42f, 1f);
    static readonly Color MagentaOscuro = new Color(0.55f, 0.08f, 0.28f, 1f);
    static readonly Color Oro = new Color(0.98f, 0.62f, 0.08f, 1f);
    static readonly Color OroClaro = new Color(1f, 0.86f, 0.28f, 1f);
    static readonly Color Morado = new Color(0.28f, 0.08f, 0.48f, 1f);
    static readonly Color MoradoOscuro = new Color(0.12f, 0.03f, 0.22f, 1f);
    static readonly Color MoradoSombrero = new Color(0.42f, 0.10f, 0.62f, 1f);
    static readonly Color AzulAsustado = new Color(0.25f, 0.45f, 0.95f, 1f);
    static readonly Color Turquesa = new Color(0.1f, 0.82f, 0.78f, 1f);
    static readonly Color Blanco = Color.white;

    static readonly Dictionary<int, Sprite> muros = new Dictionary<int, Sprite>();
    static bool listo;

    public static Sprite Petalo { get; private set; }
    public static Sprite Rastro { get; private set; }
    public static Sprite Calavera { get; private set; }
    public static Sprite CatrinaAbierta { get; private set; }
    public static Sprite CatrinaCerrada { get; private set; }
    public static Sprite FantasmaCuerpo { get; private set; }
    public static Sprite FantasmaOjos { get; private set; }
    public static Sprite FantasmaAsustado { get; private set; }

    public static void Asegurar()
    {
        if (listo)
            return;

        Petalo = DesdeFilas(new[]
        {
            "........",
            "...tt...",
            "..twwt..",
            "..twwt..",
            "...tt...",
            "........",
            "........",
            "........"
        }, "Petalo");

        Rastro = DesdeFilas(new[]
        {
            "........",
            "..yyyy..",
            ".yyooyy.",
            ".yyooyy.",
            "..yyyy..",
            "........",
            "........",
            "........"
        }, "Rastro");

        Calavera = DesdeFilas(new[]
        {
            "................",
            ".....hhhhhh.....",
            "...hhhhhhhhhh...",
            "..hhhkkhhkkhhh..",
            "..hhkkkkkkkkhh..",
            "..hhkkwwkkwwkh..",
            "..hhhkkhhkkhhh..",
            "..hhhhhhhhhhhh..",
            "..hhhhnnnnhhhh..",
            "...hhhnnhnhhh...",
            "....hhhhhhhh....",
            ".....hhoohh.....",
            "......yyyy......",
            "................",
            "................",
            "................"
        }, "Calavera");

        CatrinaCerrada = DesdeFilas(new[]
        {
            "................",
            "....oyy..yyo....",
            "...mmmmmmmmmm...",
            "..mmmppppppmmm..",
            "..mmhhhhhhhhmm..",
            "..hhkkhhhhkkhh..",
            "..hhwwhhhhwwhh..",
            "...hhhhhhhhhh...",
            "....hhhnnnhh....",
            "...mmmmmmmmmm...",
            "..mmmoooooommm..",
            "..mmmmmmmmmmmm..",
            "...mmmmmmmmmm...",
            "....m..mm..m....",
            "................",
            "................"
        }, "CatrinaQuieta");

        CatrinaAbierta = DesdeFilas(new[]
        {
            "................",
            ".....oyy.yyo....",
            "....mmmmmmmmm...",
            "...mmmppppppmm..",
            "...mmhhhhhhhhmm.",
            "..hhkkhhhhkkhh..",
            "..hhwwhhhhwwhh..",
            "...hhhhhhhhhh...",
            "....hhhnnnhh....",
            "..mmmmmmmmmmm...",
            "...mmmoooooomm..",
            "..mmmmmmmmmmmm..",
            "...mmmmmmmmmm...",
            ".....m.mm.m.....",
            "................",
            "................"
        }, "CatrinaPaso");

        FantasmaCuerpo = DesdeFilas(new[]
        {
            "................",
            "................",
            ".....bbbbbb.....",
            "...bbbbbbbbbb...",
            "..bbbbbbbbbbbb..",
            "..bb..bbbb..bb..",
            "..bb..bbbb..bb..",
            "..bbbbbbbbbbbb..",
            "..bbbbbbbbbbbb..",
            "..bbbbbbbbbbbb..",
            "..bbbbbbbbbbbb..",
            "..bbbbbbbbbbbb..",
            "..b.bb.bb.bb.b..",
            "...b..b..b..b...",
            "................",
            "................"
        }, "FantasmaCuerpo");

        FantasmaOjos = DesdeFilas(new[]
        {
            "................",
            "................",
            "................",
            "................",
            "................",
            "....kk....kk....",
            "....ww....ww....",
            "................",
            "................",
            "................",
            "................",
            "................",
            "................",
            "................",
            "................",
            "................"
        }, "FantasmaOjos");

        FantasmaAsustado = DesdeFilas(new[]
        {
            "................",
            "................",
            ".....aaaaaa.....",
            "...aaaaaaaaaaa..",
            "..aaaaaaaaaaaa..",
            "..aawwaaaawwaa..",
            "..aakkaaaakkaa..",
            "..aaaaaaaaaaaa..",
            "..aaaaaaaaaaaa..",
            "..aammaammaaaa..",
            "..ammaammaamma..",
            "..aaaaaaaaaaaa..",
            "..a.aa.aa.aa.a..",
            "...a..a..a..a...",
            "................",
            "................"
        }, "FantasmaAsustado");

        listo = true;
    }

    public static Sprite Muro(bool arriba, bool abajo, bool izquierda, bool derecha)
    {
        Asegurar();

        int clave = (arriba ? 1 : 0) | (abajo ? 2 : 0) | (izquierda ? 4 : 0) | (derecha ? 8 : 0);
        if (muros.TryGetValue(clave, out Sprite existente))
            return existente;

        const int n = 16;
        Color[] pixeles = new Color[n * n];

        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                Color color = Morado;

                if ((x + y) % 8 == 0)
                    color = MoradoOscuro;

                if (arriba && y >= 14)
                    color = y == 15 ? OroClaro : Oro;
                if (abajo && y <= 1)
                    color = y == 0 ? MagentaOscuro : Magenta;
                if (izquierda && x <= 1)
                    color = x == 0 ? MagentaOscuro : Magenta;
                if (derecha && x >= 14)
                    color = x == 15 ? OroClaro : Oro;

                pixeles[y * n + x] = color;
            }
        }

        Sprite sprite = Crear(pixeles, n, n, "Muro" + clave);
        muros[clave] = sprite;
        return sprite;
    }

    public static void PrepararCamara()
    {
        Asegurar();

        Camera camara = Camera.main;
        if (camara == null)
            return;

        camara.orthographic = true;
        camara.backgroundColor = new Color(0.035f, 0.012f, 0.06f);

        SpriteRenderer[] dibujos = Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Exclude);
        for (int i = 0; i < dibujos.Length; i++)
        {
            Sprite sprite = dibujos[i].sprite;
            if (sprite != null && sprite.texture != null)
                sprite.texture.filterMode = FilterMode.Point;
        }
    }

    public static TrajePixel VestirJugador(Catrina catrina)
    {
        Asegurar();

        catrina.transform.localScale = Vector3.one;

        CircleCollider2D colision = catrina.GetComponent<CircleCollider2D>();
        if (colision != null)
        {
            colision.isTrigger = true;
            colision.radius = 0.2f;
        }

        SpriteRenderer original = catrina.GetComponent<SpriteRenderer>();
        if (original != null)
            original.enabled = false;

        Transform existente = catrina.transform.Find("Pixel");
        GameObject pixel = existente != null ? existente.gameObject : new GameObject("Pixel");
        pixel.transform.SetParent(catrina.transform, false);
        pixel.transform.localPosition = Vector3.zero;
        pixel.transform.localScale = Vector3.one * 0.9f;

        SpriteRenderer cuerpo = pixel.GetComponent<SpriteRenderer>();
        if (cuerpo == null)
            cuerpo = pixel.AddComponent<SpriteRenderer>();

        cuerpo.sprite = CatrinaCerrada;
        cuerpo.sortingOrder = 10;
        cuerpo.color = Color.white;

        TrajePixel traje = pixel.GetComponent<TrajePixel>();
        if (traje == null)
            traje = pixel.AddComponent<TrajePixel>();

        traje.ConfigurarJugador(cuerpo, CatrinaAbierta, CatrinaCerrada);
        ApagarLucesSuaves(catrina.gameObject);
        return traje;
    }

    public static TrajePixel VestirFantasma(Fantasma fantasma)
    {
        Asegurar();

        fantasma.transform.localScale = Vector3.one * 0.9f;

        CircleCollider2D colision = fantasma.GetComponent<CircleCollider2D>();
        if (colision != null)
        {
            colision.isTrigger = true;
            colision.radius = 0.2f;
        }

        SpriteRenderer cuerpo = fantasma.GetComponent<SpriteRenderer>();
        cuerpo.sprite = FantasmaCuerpo;
        cuerpo.sortingOrder = 12;

        Transform ojosT = fantasma.transform.Find("Ojos");
        GameObject ojosGo = ojosT != null ? ojosT.gameObject : new GameObject("Ojos");
        ojosGo.transform.SetParent(fantasma.transform, false);
        ojosGo.transform.localPosition = Vector3.zero;
        ojosGo.transform.localScale = Vector3.one;

        SpriteRenderer ojos = ojosGo.GetComponent<SpriteRenderer>();
        if (ojos == null)
            ojos = ojosGo.AddComponent<SpriteRenderer>();

        ojos.sprite = FantasmaOjos;
        ojos.sortingOrder = 13;
        ojos.color = new Color(1f, 1f, 1f, 0f);

        TrajePixel traje = fantasma.GetComponent<TrajePixel>();
        if (traje == null)
            traje = fantasma.gameObject.AddComponent<TrajePixel>();

        traje.ConfigurarFantasma(cuerpo, ojos, FantasmaCuerpo, FantasmaAsustado);
        return traje;
    }

    public static void VestirFlor(GameObject flor)
    {
        Asegurar();

        flor.transform.localScale = Vector3.one * 0.72f;
        flor.transform.rotation = Quaternion.identity;

        SpriteRenderer renderer = flor.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sprite = Petalo;
            renderer.color = Color.white;
            renderer.sortingOrder = 4;
        }

        Cempasuchil cempasuchil = flor.GetComponent<Cempasuchil>();
        if (cempasuchil != null)
        {
            cempasuchil.velocidadGiro = 0f;
            cempasuchil.pulso = 0.05f;
        }

        ApagarLucesSuaves(flor);
    }

    public static void VestirCalavera(GameObject calavera)
    {
        Asegurar();

        calavera.transform.localScale = Vector3.one * 0.95f;
        calavera.transform.rotation = Quaternion.identity;

        SpriteRenderer renderer = calavera.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sprite = Calavera;
            renderer.color = Color.white;
            renderer.sortingOrder = 6;
        }
    }

    static void ApagarLucesSuaves(GameObject objeto)
    {
        Behaviour[] comportamientos = objeto.GetComponentsInChildren<Behaviour>(true);
        for (int i = 0; i < comportamientos.Length; i++)
        {
            Behaviour comportamiento = comportamientos[i];
            if (comportamiento != null && comportamiento.GetType().Name == "Light2D")
                comportamiento.enabled = false;
        }
    }

    static Sprite DesdeFilas(string[] filas, string nombre)
    {
        int alto = filas.Length;
        int ancho = filas[0].Length;
        Color[] pixeles = new Color[ancho * alto];

        for (int fila = 0; fila < alto; fila++)
        {
        int y = alto - 1 - fila;
        string linea = filas[fila];

        for (int x = 0; x < ancho; x++)
        {
            char letra = x < linea.Length ? linea[x] : '.';
            pixeles[y * ancho + x] = ColorDe(letra);
        }
        }

        return Crear(pixeles, ancho, alto, nombre);
    }

    static Color ColorDe(char letra)
    {
        switch (letra)
        {
            case 'h': return Hueso;
            case 'k': return Negro;
            case 'w': return Blanco;
            case 'm': return Magenta;
            case 'p': return MoradoSombrero;
            case 'o': return Oro;
            case 'y': return OroClaro;
            case 't': return Turquesa;
            case 'b': return Blanco;
            case 'a': return AzulAsustado;
            case 'n': return Negro;
            default: return Color.clear;
        }
    }

    static Sprite Crear(Color[] pixeles, int ancho, int alto, string nombre)
    {
        Texture2D textura = new Texture2D(ancho, alto, TextureFormat.RGBA32, false);
        textura.filterMode = FilterMode.Point;
        textura.wrapMode = TextureWrapMode.Clamp;
        textura.SetPixels(pixeles);
        textura.Apply(false, false);
        textura.name = nombre;

        return Sprite.Create(textura, new Rect(0f, 0f, ancho, alto), new Vector2(0.5f, 0.5f), PixelesPorUnidad);
    }
}

/// <summary>
/// Boca de la Catrina y ojos de los fantasmas, separados del cuerpo para poder teñirlos.
/// </summary>
public class TrajePixel : MonoBehaviour
{
    SpriteRenderer cuerpo;
    SpriteRenderer ojos;
    Sprite abierta;
    Sprite cerrada;
    Sprite cuerpoNormal;
    Sprite cuerpoAsustado;
    bool jugador;
    bool asustado;
    float reloj;

    public void ConfigurarJugador(SpriteRenderer cuerpoNuevo, Sprite spriteAbierta, Sprite spriteCerrada)
    {
        cuerpo = cuerpoNuevo;
        abierta = spriteAbierta;
        cerrada = spriteCerrada;
        jugador = true;
    }

    public void ConfigurarFantasma(SpriteRenderer cuerpoNuevo, SpriteRenderer ojosNuevos, Sprite normal, Sprite asustadoSprite)
    {
        cuerpo = cuerpoNuevo;
        ojos = ojosNuevos;
        cuerpoNormal = normal;
        cuerpoAsustado = asustadoSprite;
        jugador = false;
    }

    public void AnimarJugador(Vector2Int direccion)
    {
        if (!jugador || cuerpo == null)
            return;

        bool camina = direccion != Vector2Int.zero;
        if (camina)
            reloj += Time.deltaTime;

        cuerpo.sprite = camina && Mathf.Repeat(reloj, 0.28f) < 0.14f ? abierta : cerrada;
        transform.localRotation = Quaternion.identity;

        if (direccion.x != 0)
            cuerpo.flipX = direccion.x < 0;
    }

    public void PonerAsustado(bool activo)
    {
        asustado = activo;

        if (cuerpo != null && cuerpoNormal != null)
            cuerpo.sprite = activo ? cuerpoAsustado : cuerpoNormal;

        if (ojos != null)
            ojos.enabled = !activo;
    }

    public void SincronizarOjos(bool voltearX)
    {
        if (ojos == null || cuerpo == null || !ojos.enabled)
            return;

        Color color = ojos.color;
        color.a = cuerpo.color.a;
        ojos.color = color;
        ojos.flipX = voltearX;
        cuerpo.flipX = voltearX;
    }

    public void Visible(bool visible)
    {
        if (cuerpo != null)
            cuerpo.enabled = visible;

        if (ojos != null)
            ojos.enabled = visible && !asustado;
    }

    public void Teñir(Color color)
    {
        if (cuerpo != null)
            cuerpo.color = color;
    }
}
