using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plus: al caminar, la Catrina deja un camino de cempasúchil.
/// Los fantasmas van más lentos sobre él y, con la calaverita, evitan cruzarlo.
/// </summary>
public class RastroCempasuchil : MonoBehaviour
{
    public static RastroCempasuchil Instancia { get; private set; }

    class Marca
    {
        public Vector2Int celda;
        public float hasta;
        public float vida;
        public SpriteRenderer renderer;
    }

    readonly List<Marca> marcas = new List<Marca>();
    Vector2Int ultima = new Vector2Int(int.MinValue, 0);

    void Awake()
    {
        Instancia = this;
        PixelArte.Asegurar();
    }

    void OnDestroy()
    {
        if (Instancia == this)
            Instancia = null;
    }

    public void Limpiar()
    {
        for (int i = marcas.Count - 1; i >= 0; i--)
        {
            if (marcas[i].renderer != null)
                Destroy(marcas[i].renderer.gameObject);
        }

        marcas.Clear();
        ultima = new Vector2Int(int.MinValue, 0);
    }

    public void Sembrar(Vector2Int celda, bool poder)
    {
        if (celda == ultima || Laberinto.Instancia == null)
            return;

        ultima = celda;
        float vida = poder ? 4.2f : 2.1f;

        for (int i = 0; i < marcas.Count; i++)
        {
            if (marcas[i].celda != celda)
                continue;

            marcas[i].hasta = Time.time + vida;
            marcas[i].vida = vida;
            return;
        }

        GameObject objeto = new GameObject("PetaloCamino");
        objeto.transform.SetParent(transform, false);
        objeto.transform.position = Laberinto.Instancia.CeldaAMundo(celda);
        objeto.transform.localScale = Vector3.one * 0.85f;

        SpriteRenderer renderer = objeto.AddComponent<SpriteRenderer>();
        renderer.sprite = PixelArte.Rastro;
        renderer.sortingOrder = 2;
        renderer.color = poder
            ? new Color(1f, 0.92f, 0.35f, 0.95f)
            : new Color(0.98f, 0.55f, 0.08f, 0.85f);

        marcas.Add(new Marca
        {
            celda = celda,
            hasta = Time.time + vida,
            vida = vida,
            renderer = renderer
        });
    }

    void Update()
    {
        float ahora = Time.time;

        for (int i = marcas.Count - 1; i >= 0; i--)
        {
            Marca marca = marcas[i];
            float queda = marca.hasta - ahora;

            if (queda <= 0f || marca.renderer == null)
            {
                if (marca.renderer != null)
                    Destroy(marca.renderer.gameObject);

                marcas.RemoveAt(i);
                continue;
            }

            Color color = marca.renderer.color;
            color.a = Mathf.Clamp01(queda / marca.vida) * 0.9f;
            marca.renderer.color = color;
        }
    }

    public static bool HayEn(Vector2Int celda)
    {
        if (Instancia == null)
            return false;

        float ahora = Time.time;

        for (int i = 0; i < Instancia.marcas.Count; i++)
        {
            Marca marca = Instancia.marcas[i];
            if (marca.celda == celda && marca.hasta - ahora > 0.12f)
                return true;
        }

        return false;
    }
}
