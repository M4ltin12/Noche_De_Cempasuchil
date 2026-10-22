using UnityEngine;

public class Cempasuchil : MonoBehaviour
{
    public int puntos = 10;
    public float pulso = 0.08f;
    public float velocidadGiro = 20f;
    public GameObject efectoPetalos;
    public AudioClip sonidoRecoger;

    private Vector3 escalaBase;
    private float fase;

    void Start()
    {
        escalaBase = transform.localScale;
        fase = Random.value * 6f;
    }

    void Update()
    {
        float latido = 1f + Mathf.Sin(Time.time * 4f + fase) * pulso;
        transform.localScale = escalaBase * latido;
        transform.Rotate(0f, 0f, velocidadGiro * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (otro.GetComponent<Catrina>() == null)
            return;

        ControlJuego control = FindAnyObjectByType<ControlJuego>();

        if (control == null || control.JuegoTerminado)
            return;

        if (sonidoRecoger != null)
            AudioSource.PlayClipAtPoint(sonidoRecoger, Camera.main.transform.position, 0.55f);

        if (efectoPetalos != null)
            Instantiate(efectoPetalos, transform.position, Quaternion.identity);

        control.SumarFlor(puntos);
        Destroy(gameObject);
    }
}
