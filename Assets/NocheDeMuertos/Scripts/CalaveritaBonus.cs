using UnityEngine;

public class CalaveritaBonus : MonoBehaviour
{
    public int puntos = 50;
    public float multiplicadorVelocidad = 1.25f;
    public float duracionBonus = 6f;
    public GameObject efecto;
    public AudioClip sonidoBonus;

    private float inicio;
    private Vector3 escalaBase;

    void Start()
    {
        inicio = Time.time;
        escalaBase = transform.localScale;
    }

    void Update()
    {
        float vivo = Time.time - inicio;
        float latido = 1f + Mathf.Sin(vivo * 5f) * 0.06f;
        transform.localScale = escalaBase * latido;
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        Catrina catrina = otro.GetComponent<Catrina>();

        if (catrina == null)
            return;

        ControlJuego control = FindAnyObjectByType<ControlJuego>();

        if (control == null || control.JuegoTerminado)
            return;

        if (sonidoBonus != null)
            AudioSource.PlayClipAtPoint(sonidoBonus, Camera.main.transform.position, 0.8f);

        if (efecto != null)
            Instantiate(efecto, transform.position, Quaternion.identity);

        catrina.AplicarBonusVelocidad(multiplicadorVelocidad, duracionBonus);
        control.SumarPuntos(puntos);
        control.ActivarPoder();

        Destroy(gameObject);
    }
}
