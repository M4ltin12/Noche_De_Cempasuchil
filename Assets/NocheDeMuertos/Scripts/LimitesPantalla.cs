using UnityEngine;

public static class LimitesPantalla
{
    // Espacio reservado para el papel picado (arriba) y el HUD (abajo).
    public const float MargenLateral = 0.4f;
    public const float MargenSuperior = 1.2f;
    public const float MargenInferior = 0.9f;

    public static Rect AreaJuego(float margenExtra = 0f)
    {
        Camera camara = Camera.main;

        float alto = camara != null ? camara.orthographicSize : 5f;
        float ancho = camara != null ? alto * camara.aspect : alto * 16f / 9f;

        float xMin = -ancho + MargenLateral + margenExtra;
        float xMax = ancho - MargenLateral - margenExtra;
        float yMin = -alto + MargenInferior + margenExtra;
        float yMax = alto - MargenSuperior - margenExtra;

        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }
}
