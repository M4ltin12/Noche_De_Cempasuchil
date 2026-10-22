using System.Collections.Generic;
using UnityEngine;

public enum TipoFantasma
{
    Pedro,
    Rosa,
    Tito,
    Luna
}

[RequireComponent(typeof(Rigidbody2D))]
public class Fantasma : MonoBehaviour
{
    public TipoFantasma tipo = TipoFantasma.Pedro;
    public float velocidad = 2.8f;
    public float velocidadAsustado = 2f;
    public float velocidadRegreso = 5f;
    public float tiempoAparicion = 0.6f;
    public float alfaFinal = 0.9f;
    public GameObject efectoDesvanecer;
    public Color colorAsustado = new Color(0.45f, 0.55f, 1f, 0.95f);

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform objetivo;
    private Catrina catrina;
    private ControlJuego control;
    private Laberinto laberinto;

    private Vector2Int celdaActual;
    private Vector2Int direccion = Vector2Int.left;
    private Vector2 destino;
    private Vector2Int celdaPatrulla;
    private float tiempoVivo;
    private bool regresando;
    private bool asustado;
    private bool configurado;
    private bool listo;
    private Color colorBase = Color.white;
    private TrajePixel traje;

    bool Activo => tiempoVivo >= tiempoAparicion;

    public bool EstaAsustado => asustado && !regresando;

    private float velocidadBase;

    void Awake()
    {
        velocidadBase = velocidad;
    }

    public void AjustarNoche(int noche)
    {
        float factor = 1f + noche * 0.18f;
        velocidad = velocidadBase * factor;
        velocidadAsustado = velocidadBase * 0.72f * factor;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        control = FindAnyObjectByType<ControlJuego>();
        catrina = FindAnyObjectByType<Catrina>();

        if (catrina != null)
            objetivo = catrina.transform;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.linearVelocity = Vector2.zero;

        AplicarColorTipo();
        CambiarAlfa(0f);
        traje = PixelArte.VestirFantasma(this);
        if (traje != null)
            traje.Teñir(new Color(colorBase.r, colorBase.g, colorBase.b, 0f));
        InicializarEnLaberinto();
    }

    void InicializarEnLaberinto()
    {
        laberinto = Laberinto.Instancia;
        if (laberinto == null)
            return;

        celdaPatrulla = ElegirEsquina();

        if (!configurado)
            rb.position = laberinto.PosicionNicho();

        celdaActual = laberinto.MundoACelda(rb.position);
        rb.position = laberinto.CeldaAMundo(celdaActual);
        direccion = ElegirDireccion(celdaActual);
        destino = direccion == Vector2Int.zero
            ? rb.position
            : laberinto.CeldaAMundo(celdaActual + direccion);
        listo = true;
    }

    void AplicarColorTipo()
    {
        switch (tipo)
        {
            case TipoFantasma.Pedro:
                colorBase = new Color(1f, 0.35f, 0.35f);
                break;
            case TipoFantasma.Rosa:
                colorBase = new Color(1f, 0.55f, 0.85f);
                break;
            case TipoFantasma.Tito:
                colorBase = new Color(1f, 0.75f, 0.3f);
                break;
            case TipoFantasma.Luna:
                colorBase = new Color(0.4f, 0.85f, 1f);
                break;
        }

        if (spriteRenderer != null)
            spriteRenderer.color = new Color(colorBase.r, colorBase.g, colorBase.b, spriteRenderer.color.a);
    }

    public void Configurar(TipoFantasma nuevoTipo, Vector3 posicion)
    {
        configurado = true;
        tipo = nuevoTipo;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        AplicarColorTipo();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (rb != null)
            rb.position = posicion;
    }

    void FixedUpdate()
    {
        tiempoVivo += Time.fixedDeltaTime;

        float alfaObjetivo = regresando ? 0.45f : alfaFinal;
        float tAlfa = Activo || regresando ? 1f : tiempoVivo / Mathf.Max(tiempoAparicion, 0.01f);
        CambiarAlfa(Mathf.Lerp(0f, alfaObjetivo, tAlfa));

        if (control != null && control.JuegoTerminado)
            return;

        if (!listo)
        {
            InicializarEnLaberinto();
            if (!listo)
                return;
        }

        if (!regresando && control != null)
        {
            if (control.PoderActivo && !asustado)
                EntrarAsustado();
            else if (!control.PoderActivo && asustado)
                SalirAsustado();

            if (asustado && control.TiempoPoderRestante < 2f)
            {
                bool visible = Mathf.Repeat(Time.time * 8f, 1f) > 0.4f;
                Color c = Color.white;
                c.a = visible ? alfaFinal : 0.25f;
                spriteRenderer.color = c;
            }
        }

        Mover();
    }

    void Mover()
    {
        float vel = regresando ? velocidadRegreso : (asustado ? velocidadAsustado : velocidad);
        if (RastroCempasuchil.HayEn(celdaActual))
            vel *= control != null && control.PoderActivo ? 0.35f : 0.55f;
        float paso = vel * Time.fixedDeltaTime;

        if (direccion == Vector2Int.zero)
        {
            direccion = ElegirDireccion(celdaActual);
            if (direccion != Vector2Int.zero)
                destino = laberinto.CeldaAMundo(celdaActual + direccion);
            return;
        }

        Vector2 pos = rb.position;
        Vector2 hacia = destino - pos;
        float dist = hacia.magnitude;

        if (dist <= paso)
        {
            rb.position = destino;
            celdaActual += direccion;

            if (regresando && celdaActual == laberinto.CeldaNicho)
            {
                LlegarAlNicho();
                return;
            }

            direccion = ElegirDireccion(celdaActual);
            destino = direccion == Vector2Int.zero
                ? rb.position
                : laberinto.CeldaAMundo(celdaActual + direccion);
        }
        else
        {
            rb.MovePosition(pos + hacia / dist * paso);
        }

        if (direccion.x != 0 && traje != null)
            traje.SincronizarOjos(direccion.x > 0);
        else if (traje != null)
            traje.SincronizarOjos(spriteRenderer.flipX);
    }

    Vector2Int ElegirDireccion(Vector2Int celda)
    {
        Vector2Int atras = -direccion;
        List<Vector2Int> opciones = laberinto.DireccionesLibres(celda, atras);

        if (opciones.Count == 0)
        {
            if (laberinto.PuedeAvanzar(celda, atras))
                return atras;
            return Vector2Int.zero;
        }

        if (control != null && control.PoderActivo && !regresando)
            opciones = EvitarRastro(celda, opciones);

        if (regresando)
            return DireccionHacia(celda, laberinto.CeldaNicho, opciones);

        if (asustado)
            return DireccionHuyendo(celda, opciones);

        return DireccionHacia(celda, CalcularObjetivo(celda), opciones);
    }

    Vector2Int CalcularObjetivo(Vector2Int celdaActualLocal)
    {
        if (objetivo == null || catrina == null)
            return celdaPatrulla;

        Vector2Int jugador = laberinto.MundoACelda(objetivo.position);
        Vector2Int dirJugador = catrina.DireccionActual;

        switch (tipo)
        {
            case TipoFantasma.Pedro:
                return jugador;
            case TipoFantasma.Rosa:
                return jugador + dirJugador * 4;
            case TipoFantasma.Tito:
                if (Vector2Int.Distance(celdaActualLocal, jugador) > 6)
                    return jugador;
                return celdaPatrulla;
            default:
                if (Vector2Int.Distance(celdaActualLocal, jugador) < 5)
                    return jugador;
                return celdaPatrulla;
        }
    }

    Vector2Int DireccionHuyendo(Vector2Int celda, List<Vector2Int> opciones)
    {
        if (objetivo == null)
            return opciones[Random.Range(0, opciones.Count)];

        Vector2Int jugador = laberinto.MundoACelda(objetivo.position);
        Vector2Int mejor = opciones[0];
        float mejorDist = -1f;

        for (int i = 0; i < opciones.Count; i++)
        {
            float d = Vector2Int.Distance(celda + opciones[i], jugador);
            if (d > mejorDist)
            {
                mejorDist = d;
                mejor = opciones[i];
            }
        }

        if (tipo == TipoFantasma.Tito && Random.value < 0.35f)
            return opciones[Random.Range(0, opciones.Count)];

        return mejor;
    }

    Vector2Int DireccionHacia(Vector2Int celda, Vector2Int destinoCelda, List<Vector2Int> opciones)
    {
        Vector2Int mejor = opciones[0];
        float mejorDist = float.MaxValue;

        for (int i = 0; i < opciones.Count; i++)
        {
            float d = Vector2Int.Distance(celda + opciones[i], destinoCelda);
            if (d < mejorDist)
            {
                mejorDist = d;
                mejor = opciones[i];
            }
        }

        if (tipo == TipoFantasma.Tito && !asustado && Random.value < 0.25f)
            return opciones[Random.Range(0, opciones.Count)];

        return mejor;
    }

    Vector2Int ElegirEsquina()
    {
        Vector2Int[] esquinas =
        {
            new Vector2Int(1, 1),
            new Vector2Int(laberinto.Columnas - 2, 1),
            new Vector2Int(1, laberinto.Filas - 2),
            new Vector2Int(laberinto.Columnas - 2, laberinto.Filas - 2)
        };

        return esquinas[(int)tipo % esquinas.Length];
    }

    public void EntrarAsustado()
    {
        if (regresando)
            return;

        if (!asustado)
            direccion = -direccion;

        asustado = true;
        if (traje != null)
            traje.PonerAsustado(true);
        spriteRenderer.color = new Color(1f, 1f, 1f, alfaFinal);

        if (direccion != Vector2Int.zero && laberinto != null)
            destino = laberinto.CeldaAMundo(celdaActual + direccion);
    }

    public void SalirAsustado()
    {
        asustado = false;
        if (traje != null)
            traje.PonerAsustado(false);
        if (!regresando)
            spriteRenderer.color = new Color(colorBase.r, colorBase.g, colorBase.b, alfaFinal);
    }

    public void ReiniciarEnNicho(Vector3 posicion)
    {
        regresando = false;
        asustado = false;
        tiempoVivo = 0f;
        rb.position = posicion;

        if (laberinto != null)
        {
            celdaActual = laberinto.MundoACelda(posicion);
            rb.position = laberinto.CeldaAMundo(celdaActual);
            direccion = ElegirDireccion(celdaActual);
            destino = direccion == Vector2Int.zero
                ? rb.position
                : laberinto.CeldaAMundo(celdaActual + direccion);
        }
        else
        {
            direccion = Vector2Int.left;
            destino = posicion;
        }

        spriteRenderer.color = new Color(colorBase.r, colorBase.g, colorBase.b, 0f);
        if (traje != null)
            traje.PonerAsustado(false);
    }

    void LlegarAlNicho()
    {
        regresando = false;
        asustado = false;
        tiempoVivo = 0f;
        direccion = ElegirDireccion(celdaActual);
        destino = direccion == Vector2Int.zero
            ? rb.position
            : laberinto.CeldaAMundo(celdaActual + direccion);
        spriteRenderer.color = new Color(colorBase.r, colorBase.g, colorBase.b, 0f);
        if (traje != null)
            traje.PonerAsustado(false);
    }

    public void SerAhuyentado()
    {
        if (regresando)
            return;

        regresando = true;
        asustado = false;
        if (traje != null)
            traje.PonerAsustado(false);
        spriteRenderer.color = new Color(colorBase.r, colorBase.g, colorBase.b, 0.4f);

        if (efectoDesvanecer != null)
            Instantiate(efectoDesvanecer, transform.position, Quaternion.identity);

        if (control != null)
            control.SumarPuntosFantasma();

        direccion = ElegirDireccion(celdaActual);
        if (direccion != Vector2Int.zero)
            destino = laberinto.CeldaAMundo(celdaActual + direccion);
    }

    void CambiarAlfa(float alfa)
    {
        Color color = spriteRenderer.color;
        color.a = Mathf.Clamp01(alfa);
        spriteRenderer.color = color;
        if (traje != null)
            traje.SincronizarOjos(spriteRenderer.flipX);
    }

    List<Vector2Int> EvitarRastro(Vector2Int celda, List<Vector2Int> opciones)
    {
        List<Vector2Int> limpias = new List<Vector2Int>();

        for (int i = 0; i < opciones.Count; i++)
        {
            if (!RastroCempasuchil.HayEn(celda + opciones[i]))
                limpias.Add(opciones[i]);
        }

        return limpias.Count > 0 ? limpias : opciones;
    }

    void OnTriggerStay2D(Collider2D otro)
    {
        if (!Activo && !regresando)
            return;

        Catrina jugadora = otro.GetComponent<Catrina>();
        if (jugadora == null || regresando)
            return;

        if (EstaAsustado)
        {
            SerAhuyentado();
            return;
        }

        jugadora.RecibirDano();
    }
}
