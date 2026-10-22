using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Catrina : MonoBehaviour
{
    public float velocidad = 4.5f;
    public float tiempoInvulnerable = 2f;
    public float velocidadParpadeo = 0.12f;
    public AudioClip sonidoDano;
    public Color colorBonus = new Color(1f, 0.85f, 0.4f);

    private Rigidbody2D rb;
    private AudioSource audioSource;
    private ControlJuego control;
    private Laberinto laberinto;

    private Vector2Int celdaActual;
    private Vector2Int direccion = Vector2Int.zero;
    private Vector2Int direccionDeseada = Vector2Int.left;
    private Vector2 destino;
    private float multiplicadorVelocidad = 1f;
    private bool invulnerable = false;
    private bool listo;
    private Coroutine rutinaBonus;
    private TrajePixel traje;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        control = FindAnyObjectByType<ControlJuego>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.linearVelocity = Vector2.zero;

        traje = PixelArte.VestirJugador(this);
        InicializarEnLaberinto();
    }

    void LateUpdate()
    {
        if (traje == null)
            return;

        Vector2Int mirada = direccion != Vector2Int.zero ? direccion : direccionDeseada;
        traje.AnimarJugador(mirada);
    }

    void InicializarEnLaberinto()
    {
        laberinto = Laberinto.Instancia;
        if (laberinto == null)
            return;

        celdaActual = laberinto.CeldaJugador;
        rb.position = laberinto.CeldaAMundo(celdaActual);
        destino = rb.position;
        direccion = Vector2Int.zero;
        listo = true;
    }

    public void Reubicar()
    {
        listo = false;
        InicializarEnLaberinto();
    }

    void Update()
    {
        if (control != null && control.JuegoTerminado)
            return;

        LeerInput();
    }

    void LeerInput()
    {
        Keyboard teclado = Keyboard.current;
        if (teclado != null)
        {
            if (teclado.aKey.isPressed || teclado.leftArrowKey.isPressed)
            {
                direccionDeseada = Vector2Int.left;
                return;
            }

            if (teclado.dKey.isPressed || teclado.rightArrowKey.isPressed)
            {
                direccionDeseada = Vector2Int.right;
                return;
            }

            if (teclado.wKey.isPressed || teclado.upArrowKey.isPressed)
            {
                direccionDeseada = Vector2Int.down; // fila menor = arriba en pantalla
                return;
            }

            if (teclado.sKey.isPressed || teclado.downArrowKey.isPressed)
                direccionDeseada = Vector2Int.up;
        }
    }

    void FixedUpdate()
    {
        if (control != null && control.JuegoTerminado)
            return;

        if (!listo)
        {
            InicializarEnLaberinto();
            if (!listo)
                return;
        }

        float paso = velocidad * multiplicadorVelocidad * Time.fixedDeltaTime;

        // Sin dirección: intentar arrancar desde la celda actual.
        if (direccion == Vector2Int.zero)
        {
            if (laberinto.PuedeAvanzar(celdaActual, direccionDeseada))
            {
                direccion = direccionDeseada;
                destino = laberinto.CeldaAMundo(celdaActual + direccion);
            }

            rb.rotation = 0f;
            return;
        }

        Vector2 pos = rb.position;
        Vector2 hacia = destino - pos;
        float dist = hacia.magnitude;

        if (dist <= paso)
        {
            rb.position = destino;
            celdaActual += direccion;

            // Giro o seguir; si choca, detenemos en el centro.
            if (laberinto.PuedeAvanzar(celdaActual, direccionDeseada))
                direccion = direccionDeseada;
            else if (!laberinto.PuedeAvanzar(celdaActual, direccion))
                direccion = Vector2Int.zero;

            if (direccion != Vector2Int.zero)
                destino = laberinto.CeldaAMundo(celdaActual + direccion);

            if (RastroCempasuchil.Instancia != null)
                RastroCempasuchil.Instancia.Sembrar(celdaActual, control != null && control.PoderActivo);
        }
        else
        {
            rb.MovePosition(pos + hacia / dist * paso);
        }

        rb.rotation = 0f;
    }

    public Vector2Int DireccionActual => direccion;

    public bool RecibirDano()
    {
        if (invulnerable || control == null || control.JuegoTerminado)
            return false;

        if (control.PoderActivo)
            return false;

        control.PerderVida();

        if (sonidoDano != null)
            audioSource.PlayOneShot(sonidoDano);

        if (control.JuegoTerminado)
        {
            direccion = Vector2Int.zero;
            if (traje != null)
                traje.Teñir(new Color(0.6f, 0.6f, 0.7f, 0.5f));
        }
        else
        {
            if (laberinto != null)
            {
                celdaActual = laberinto.CeldaJugador;
                rb.position = laberinto.CeldaAMundo(celdaActual);
                destino = rb.position;
                direccion = Vector2Int.zero;
            }

            StartCoroutine(Invulnerabilidad());
        }

        return true;
    }

    IEnumerator Invulnerabilidad()
    {
        invulnerable = true;
        float tiempo = 0f;
        bool cuerpoVisible = true;

        while (tiempo < tiempoInvulnerable)
        {
            if (traje != null)
                traje.Visible(!cuerpoVisible);
            cuerpoVisible = !cuerpoVisible;
            yield return new WaitForSeconds(velocidadParpadeo);
            tiempo += velocidadParpadeo;
        }

        if (traje != null)
            traje.Visible(true);
        invulnerable = false;
    }

    public void AplicarBonusVelocidad(float multiplicador, float duracion)
    {
        if (rutinaBonus != null)
            StopCoroutine(rutinaBonus);

        rutinaBonus = StartCoroutine(BonusVelocidad(multiplicador, duracion));
    }

    IEnumerator BonusVelocidad(float multiplicador, float duracion)
    {
        multiplicadorVelocidad = multiplicador;
        if (traje != null)
            traje.Teñir(colorBonus);
        yield return new WaitForSeconds(duracion);
        multiplicadorVelocidad = 1f;
        if (traje != null)
            traje.Teñir(Color.white);
        rutinaBonus = null;
    }
}
