using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NocheDeMuertosEditor
{
    public static class GeneradorSprites
    {
        public const string RutaSprites = "Assets/NocheDeMuertos/Sprites";

        static readonly Color Hueso = new Color(0.98f, 0.95f, 0.88f);
        static readonly Color Contorno = new Color(0.35f, 0.3f, 0.4f);
        static readonly Color Negro = new Color(0.08f, 0.05f, 0.1f);
        static readonly Color NaranjaOscuro = new Color(0.85f, 0.33f, 0.02f);
        static readonly Color Naranja = new Color(1f, 0.55f, 0.05f);
        static readonly Color NaranjaClaro = new Color(1f, 0.68f, 0.1f);
        static readonly Color Amarillo = new Color(1f, 0.82f, 0.2f);
        static readonly Color Rosa = new Color(1f, 0.35f, 0.65f);
        static readonly Color Magenta = new Color(0.75f, 0.1f, 0.45f);
        static readonly Color Turquesa = new Color(0.2f, 0.8f, 0.85f);
        static readonly Color Morado = new Color(0.45f, 0.15f, 0.6f);
        static readonly Color MoradoOscuro = new Color(0.3f, 0.08f, 0.42f);
        static readonly Color Verde = new Color(0.25f, 0.55f, 0.2f);

        [MenuItem("Noche de Muertos/Generar sprites")]
        public static void GenerarTodos()
        {
            Directory.CreateDirectory(RutaSprites);

            Guardar("Catrina", DibujarCatrina(), 128);
            Guardar("Fantasma", DibujarFantasma(), 128);
            Guardar("Cempasuchil", DibujarCempasuchil(), 128);
            Guardar("Calaverita", DibujarCalaverita(), 128);
            Guardar("Lapida", DibujarLapida(), 128);
            Guardar("Cruz", DibujarCruz(), 128);
            Guardar("Calabaza", DibujarCalabaza(), 128);
            Guardar("Vela", DibujarVela(), 128);
            Guardar("PapelPicado", DibujarPapelPicado(), 128);
            Guardar("Particula", DibujarParticula(), 32);

            AssetDatabase.Refresh();
        }

        static void Guardar(string nombre, Lienzo lienzo, float pixelesPorUnidad)
        {
            string ruta = RutaSprites + "/" + nombre + ".png";
            File.WriteAllBytes(ruta, lienzo.CodificarPNG());
            AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceSynchronousImport);

            TextureImporter importador = (TextureImporter)AssetImporter.GetAtPath(ruta);
            importador.textureType = TextureImporterType.Sprite;
            importador.spriteImportMode = SpriteImportMode.Single;
            importador.spritePixelsPerUnit = pixelesPorUnidad;
            importador.mipmapEnabled = false;
            importador.alphaIsTransparency = true;
            importador.textureCompression = TextureImporterCompression.Uncompressed;
            importador.filterMode = FilterMode.Bilinear;
            importador.wrapMode = TextureWrapMode.Clamp;
            importador.SaveAndReimport();
        }

        static Lienzo DibujarCatrina()
        {
            Lienzo l = new Lienzo(128, 128);

            l.Elipse(64, 14, 38, 17, Contorno);
            l.Elipse(64, 14, 36, 15, Magenta);

            l.Circulo(64, 56, 33, Contorno);
            l.Circulo(64, 56, 31, Hueso);

            l.Anillo(50, 58, 9, 12.5f, Naranja);
            l.Anillo(78, 58, 9, 12.5f, Naranja);
            l.Circulo(50, 58, 9, Negro);
            l.Circulo(78, 58, 9, Negro);
            l.Circulo(53, 61, 2.5f, Color.white);
            l.Circulo(81, 61, 2.5f, Color.white);

            l.Circulo(61, 46, 3, Negro);
            l.Circulo(67, 46, 3, Negro);
            l.Triangulo(58, 46, 70, 46, 64, 39, Negro);

            l.Rect(46, 30, 82, 32.5f, Negro);
            for (int x = 50; x <= 78; x += 5)
                l.Rect(x, 26, x + 1.5f, 36, Negro);

            l.Circulo(38, 44, 4, Rosa);
            l.Circulo(90, 44, 4, Rosa);

            for (int i = -2; i <= 2; i++)
                l.Circulo(64 + i * 8, 75 - Mathf.Abs(i) * 2, 2.2f, Turquesa);

            l.Elipse(64, 104, 30, 20, Contorno);
            l.Elipse(64, 104, 28, 18, MoradoOscuro);
            l.Rect(38, 92, 90, 98, Rosa);
            l.Elipse(64, 88, 62, 11, Contorno);
            l.Elipse(64, 88, 60, 9.5f, Morado);

            l.Flor(34, 94, 6, 5, 7, 0, Naranja);
            l.Circulo(34, 94, 3.5f, Amarillo);
            l.Flor(94, 94, 6, 5, 7, 0, Naranja);
            l.Circulo(94, 94, 3.5f, Amarillo);
            l.Flor(64, 100, 7, 6, 8, 0, NaranjaOscuro);
            l.Circulo(64, 100, 4, Amarillo);

            return l;
        }

        static float CuerpoFantasma(float x, float y)
        {
            float cabeza = Mathf.Sqrt((x - 64) * (x - 64) + (y - 76) * (y - 76)) - 40f;
            float baseOndulada = 24f + 7f * Mathf.Sin(x * 0.24f);
            float cuerpo = Mathf.Max(Mathf.Max(24f - x, x - 104f), Mathf.Max(baseOndulada - y, y - 76f));
            return Mathf.Min(cabeza, cuerpo);
        }

        static Lienzo DibujarFantasma()
        {
            Lienzo l = new Lienzo(128, 128);
            Color contornoFantasma = new Color(0.55f, 0.62f, 0.8f);

            l.Forma((x, y) => CuerpoFantasma(x, y) - 2.5f, contornoFantasma);
            l.Forma(CuerpoFantasma, new Color(0.93f, 0.96f, 1f));

            l.Elipse(50, 82, 7, 11, Negro);
            l.Elipse(78, 82, 7, 11, Negro);
            l.Circulo(52, 86, 2, Color.white);
            l.Circulo(80, 86, 2, Color.white);
            l.Elipse(64, 58, 7, 10, Negro);
            l.Circulo(38, 66, 5, new Color(1f, 0.6f, 0.75f, 0.6f));
            l.Circulo(90, 66, 5, new Color(1f, 0.6f, 0.75f, 0.6f));

            return l;
        }

        static Lienzo DibujarCempasuchil()
        {
            Lienzo l = new Lienzo(128, 128);

            l.Flor(64, 64, 38, 17, 12, 0, NaranjaOscuro);
            l.Flor(64, 64, 36, 15, 12, 0, Naranja);
            l.Flor(64, 64, 22, 15, 10, 0.3f, NaranjaOscuro);
            l.Flor(64, 64, 20, 13, 10, 0.3f, NaranjaClaro);
            l.Flor(64, 64, 9, 10, 8, 0.1f, Amarillo);
            l.Circulo(64, 64, 7, new Color(0.9f, 0.45f, 0.02f));

            return l;
        }

        static Lienzo DibujarCalaverita()
        {
            Lienzo l = new Lienzo(128, 128);

            l.Circulo(64, 72, 43, Contorno);
            l.Elipse(64, 36, 29, 21, Contorno);
            l.Circulo(64, 72, 40, Hueso);
            l.Elipse(64, 36, 26, 18, Hueso);

            l.Circulo(48, 72, 15, Turquesa);
            l.Circulo(80, 72, 15, Rosa);
            l.Circulo(48, 72, 8.5f, Negro);
            l.Circulo(80, 72, 8.5f, Negro);
            l.Circulo(51, 75, 2.5f, Color.white);
            l.Circulo(83, 75, 2.5f, Color.white);

            l.Triangulo(59, 52, 69, 52, 64, 60, Negro);

            l.Rect(46, 34, 82, 37, Negro);
            for (int x = 50; x <= 78; x += 6)
                l.Rect(x, 28, x + 1.5f, 42, Negro);

            l.Flor(64, 100, 8, 5.5f, 6, 0, Naranja);
            l.Circulo(64, 100, 4, Amarillo);
            l.Circulo(36, 50, 3, Rosa);
            l.Circulo(92, 50, 3, Turquesa);
            l.Circulo(30, 70, 2.5f, Amarillo);
            l.Circulo(98, 70, 2.5f, Amarillo);

            return l;
        }

        static float FormaLapida(float x, float y)
        {
            float rect = Mathf.Max(Mathf.Max(26f - x, x - 102f), Mathf.Max(6f - y, y - 80f));
            float arco = Mathf.Sqrt((x - 64) * (x - 64) + (y - 80) * (y - 80)) - 38f;
            return Mathf.Min(rect, arco);
        }

        static Lienzo DibujarLapida()
        {
            Lienzo l = new Lienzo(128, 128);
            Color grabado = new Color(0.3f, 0.3f, 0.36f);

            l.Forma((x, y) => FormaLapida(x, y) - 3f, new Color(0.2f, 0.2f, 0.26f));
            l.Forma(FormaLapida, new Color(0.5f, 0.5f, 0.57f));
            l.Rect(84, 10, 100, 80, new Color(0.42f, 0.42f, 0.49f));
            l.Rect(60, 30, 68, 96, grabado);
            l.Rect(46, 70, 82, 78, grabado);

            for (int x = 20; x <= 108; x += 8)
                l.Triangulo(x - 5, 2, x + 5, 2, x + 1, 16, Verde);

            return l;
        }

        static Lienzo DibujarCruz()
        {
            Lienzo l = new Lienzo(128, 128);
            Color madera = new Color(0.45f, 0.28f, 0.14f);
            Color maderaOscura = new Color(0.22f, 0.13f, 0.06f);

            l.Rect(53, 2, 75, 124, maderaOscura);
            l.Rect(25, 77, 103, 99, maderaOscura);
            l.Rect(56, 4, 72, 122, madera);
            l.Rect(28, 80, 100, 96, madera);
            l.Flor(64, 88, 6, 4.5f, 7, 0, Naranja);
            l.Circulo(64, 88, 3, Amarillo);

            return l;
        }

        static Lienzo DibujarCalabaza()
        {
            Lienzo l = new Lienzo(128, 128);
            Color luzInterior = new Color(1f, 0.88f, 0.3f);

            l.Rect(57, 84, 71, 112, new Color(0.2f, 0.35f, 0.12f));
            l.Elipse(40, 52, 28, 40, NaranjaOscuro);
            l.Elipse(88, 52, 28, 40, NaranjaOscuro);
            l.Elipse(64, 52, 32, 44, Naranja);
            l.Elipse(64, 52, 14, 42, NaranjaClaro);

            l.Triangulo(34, 62, 56, 62, 45, 80, luzInterior);
            l.Triangulo(72, 62, 94, 62, 83, 80, luzInterior);
            l.Triangulo(57, 48, 71, 48, 64, 58, luzInterior);
            l.Rect(34, 26, 94, 40, luzInterior);
            l.Rect(50, 33, 58, 40, Naranja);
            l.Rect(70, 26, 78, 33, Naranja);

            return l;
        }

        static Lienzo DibujarVela()
        {
            Lienzo l = new Lienzo(128, 128);

            l.Circulo(64, 88, 28, new Color(1f, 0.8f, 0.3f, 0.18f));
            l.Rect(48, 4, 80, 72, new Color(0.7f, 0.62f, 0.5f));
            l.Rect(50, 6, 78, 70, new Color(0.96f, 0.9f, 0.76f));
            l.Circulo(55, 64, 4, new Color(0.96f, 0.9f, 0.76f));
            l.Rect(51, 52, 59, 66, new Color(0.96f, 0.9f, 0.76f));
            l.Rect(63, 70, 65, 78, Negro);
            l.Elipse(64, 90, 8, 16, Naranja);
            l.Elipse(64, 86, 4, 8, Amarillo);

            return l;
        }

        static Lienzo DibujarPapelPicado()
        {
            Lienzo l = new Lienzo(128, 96);

            l.Rect(6, 14, 122, 92, Color.white);
            float ancho = 116f / 8f;
            for (int i = 0; i < 8; i++)
                l.Triangulo(6 + i * ancho, 14.5f, 6 + (i + 1) * ancho, 14.5f, 6 + (i + 0.5f) * ancho, 4, Color.white);

            l.Flor(64, 52, 15, 7, 6, 0, Color.white, true);
            l.Circulo(64, 52, 6, Color.white, true);
            l.Circulo(24, 30, 6, Color.white, true);
            l.Circulo(104, 30, 6, Color.white, true);
            l.Circulo(24, 72, 6, Color.white, true);
            l.Circulo(104, 72, 6, Color.white, true);
            for (int x = 14; x <= 114; x += 12)
                l.Circulo(x, 84, 2.5f, Color.white, true);

            return l;
        }

        static Lienzo DibujarParticula()
        {
            Lienzo l = new Lienzo(32, 32);
            l.Circulo(16, 16, 14, Color.white);
            return l;
        }

        class Lienzo
        {
            readonly int ancho;
            readonly int alto;
            readonly Color[] pixeles;

            public Lienzo(int ancho, int alto)
            {
                this.ancho = ancho;
                this.alto = alto;
                pixeles = new Color[ancho * alto];
            }

            public byte[] CodificarPNG()
            {
                Texture2D textura = new Texture2D(ancho, alto, TextureFormat.RGBA32, false);
                textura.SetPixels(pixeles);
                textura.Apply();
                byte[] png = textura.EncodeToPNG();
                UnityEngine.Object.DestroyImmediate(textura);
                return png;
            }

            public void Forma(Func<float, float, float> distancia, Color color, bool borrar = false)
            {
                for (int y = 0; y < alto; y++)
                {
                    for (int x = 0; x < ancho; x++)
                    {
                        float cobertura = Mathf.Clamp01(0.5f - distancia(x + 0.5f, y + 0.5f));
                        if (cobertura <= 0f)
                            continue;

                        int i = y * ancho + x;

                        if (borrar)
                        {
                            pixeles[i].a *= 1f - cobertura;
                            continue;
                        }

                        float a = color.a * cobertura;
                        Color destino = pixeles[i];
                        float alfaFinal = a + destino.a * (1f - a);
                        Color resultado = (color * a + destino * destino.a * (1f - a)) / alfaFinal;
                        resultado.a = alfaFinal;
                        pixeles[i] = resultado;
                    }
                }
            }

            public void Circulo(float cx, float cy, float r, Color color, bool borrar = false)
            {
                Forma((x, y) => Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) - r, color, borrar);
            }

            public void Elipse(float cx, float cy, float rx, float ry, Color color)
            {
                float menor = Mathf.Min(rx, ry);
                Forma((x, y) =>
                {
                    float dx = (x - cx) / rx;
                    float dy = (y - cy) / ry;
                    return (Mathf.Sqrt(dx * dx + dy * dy) - 1f) * menor;
                }, color);
            }

            public void Anillo(float cx, float cy, float rInterior, float rExterior, Color color)
            {
                float medio = (rInterior + rExterior) * 0.5f;
                float grosor = (rExterior - rInterior) * 0.5f;
                Forma((x, y) => Mathf.Abs(Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) - medio) - grosor, color);
            }

            public void Rect(float x0, float y0, float x1, float y1, Color color)
            {
                Forma((x, y) => Mathf.Max(Mathf.Max(x0 - x, x - x1), Mathf.Max(y0 - y, y - y1)), color);
            }

            public void Triangulo(float ax, float ay, float bx, float by, float cx, float cy, Color color)
            {
                float orientacion = Mathf.Sign((bx - ax) * (cy - ay) - (by - ay) * (cx - ax));

                Forma((x, y) => Mathf.Max(
                    Mathf.Max(Borde(ax, ay, bx, by, x, y, orientacion), Borde(bx, by, cx, cy, x, y, orientacion)),
                    Borde(cx, cy, ax, ay, x, y, orientacion)), color);
            }

            static float Borde(float ax, float ay, float bx, float by, float x, float y, float orientacion)
            {
                float ex = bx - ax;
                float ey = by - ay;
                float largo = Mathf.Sqrt(ex * ex + ey * ey);
                return -orientacion * (ex * (y - ay) - ey * (x - ax)) / largo;
            }

            public void Flor(float cx, float cy, float distancia, float radioPetalo, int petalos, float desfase, Color color, bool borrar = false)
            {
                for (int i = 0; i < petalos; i++)
                {
                    float angulo = desfase + i * Mathf.PI * 2f / petalos;
                    Circulo(cx + Mathf.Cos(angulo) * distancia, cy + Mathf.Sin(angulo) * distancia, radioPetalo, color, borrar);
                }
            }
        }
    }
}
