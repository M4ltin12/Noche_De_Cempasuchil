using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NocheDeMuertosEditor
{
    public static class ConstructorEscena
    {
        const string Raiz = "Assets/NocheDeMuertos";
        const string RutaPrefabs = Raiz + "/Prefabs";
        const string RutaEscenas = Raiz + "/Scenes";
        const string RutaAudio = Raiz + "/Audio";
        const string RutaMenu = RutaEscenas + "/MainMenu.unity";
        const string RutaJuego = RutaEscenas + "/Juego.unity";

        static readonly Color ColorFondo = new Color(0.07f, 0.04f, 0.14f);
        static readonly Color ColorNaranja = new Color(1f, 0.6f, 0.1f);
        static readonly Color ColorMorado = new Color(0.45f, 0.15f, 0.6f);
        static readonly Color ColorPanel = new Color(0.05f, 0.02f, 0.1f, 0.88f);

        static readonly Color[] ColoresPapelPicado =
        {
            new Color(1f, 0.3f, 0.6f),
            new Color(1f, 0.6f, 0.1f),
            new Color(0.6f, 0.3f, 0.9f),
            new Color(0.3f, 0.8f, 0.4f),
            new Color(0.2f, 0.7f, 0.95f),
            new Color(1f, 0.85f, 0.2f)
        };

        [MenuItem("Noche de Muertos/Construir juego")]
        public static void ConstruirJuego()
        {
            GeneradorSprites.GenerarTodos();

            Directory.CreateDirectory(RutaPrefabs);
            Directory.CreateDirectory(RutaEscenas);
            AssetDatabase.Refresh();

            Material materialParticulas = CrearMaterialParticulas();

            GameObject petalos = CrearPrefabParticulas(
                "Petalos", new Color(1f, 0.5f, 0.05f), new Color(1f, 0.85f, 0.2f), 0.6f, materialParticulas);
            GameObject ectoplasma = CrearPrefabParticulas(
                "Ectoplasma", new Color(0.85f, 0.95f, 1f), new Color(0.5f, 1f, 0.7f), -0.3f, materialParticulas);
            GameObject destellos = CrearPrefabParticulas(
                "Destellos", new Color(1f, 0.4f, 0.7f), new Color(0.3f, 0.9f, 0.95f), 0.2f, materialParticulas);

            GameObject flor = CrearPrefabFlor(petalos);
            GameObject fantasma = CrearPrefabFantasma(ectoplasma);
            GameObject calaverita = CrearPrefabCalaverita(destellos);

            CrearEscenaMenu();
            CrearEscenaJuego(flor, fantasma, calaverita, CargarSprite("Lapida"));

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(RutaMenu, true),
                new EditorBuildSettingsScene(RutaJuego, true)
            };

            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(RutaMenu);

            Debug.Log("NocheDeMuertos: juego construido. Abre MainMenu y presiona Play.");
        }

        // ---------- Recursos ----------

        static Sprite CargarSprite(string nombre)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(GeneradorSprites.RutaSprites + "/" + nombre + ".png");
        }

        static AudioClip CargarAudio(string archivo)
        {
            return AssetDatabase.LoadAssetAtPath<AudioClip>(RutaAudio + "/" + archivo);
        }

        static Material CrearMaterialParticulas()
        {
            string ruta = Raiz + "/Particulas.mat";
            Shader shader = Shader.Find("Sprites/Default");

            Material material = AssetDatabase.LoadAssetAtPath<Material>(ruta);

            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, ruta);
            }

            material.shader = shader;
            material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(GeneradorSprites.RutaSprites + "/Particula.png");
            EditorUtility.SetDirty(material);

            return material;
        }

        static GameObject GuardarPrefab(GameObject objeto)
        {
            string ruta = RutaPrefabs + "/" + objeto.name + ".prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(objeto, ruta);
            Object.DestroyImmediate(objeto);
            return prefab;
        }

        static GameObject CrearSprite(string nombre, string sprite, Vector3 posicion, float escala, int orden, Transform padre = null)
        {
            GameObject objeto = new GameObject(nombre);

            if (padre != null)
                objeto.transform.SetParent(padre, false);

            objeto.transform.localPosition = posicion;
            objeto.transform.localScale = Vector3.one * escala;

            SpriteRenderer renderer = objeto.AddComponent<SpriteRenderer>();
            renderer.sprite = CargarSprite(sprite);
            renderer.sortingOrder = orden;

            return objeto;
        }

        static Light2D CrearLuz(string nombre, Transform padre, Vector3 posicion, Light2D.LightType tipo, Color color, float intensidad, float radio = 1f)
        {
            GameObject objeto = new GameObject(nombre);

            if (padre != null)
                objeto.transform.SetParent(padre, false);

            objeto.transform.localPosition = posicion;

            Light2D luz = objeto.AddComponent<Light2D>();
            luz.lightType = tipo;
            luz.color = color;
            luz.intensity = intensidad;

            if (tipo == Light2D.LightType.Point)
            {
                luz.pointLightOuterRadius = radio;
                luz.pointLightInnerRadius = radio * 0.15f;
            }

            AplicarATodasLasCapas(luz);
            return luz;
        }

        static void AplicarATodasLasCapas(Light2D luz)
        {
            SerializedObject serializado = new SerializedObject(luz);
            SerializedProperty capas = serializado.FindProperty("m_ApplyToSortingLayers");

            if (capas == null)
                return;

            SortingLayer[] todas = SortingLayer.layers;
            capas.arraySize = todas.Length;

            for (int i = 0; i < todas.Length; i++)
                capas.GetArrayElementAtIndex(i).intValue = todas[i].id;

            serializado.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- Prefabs ----------

        static GameObject CrearPrefabParticulas(string nombre, Color colorA, Color colorB, float gravedad, Material material)
        {
            GameObject objeto = new GameObject(nombre);
            ParticleSystem particulas = objeto.AddComponent<ParticleSystem>();
            particulas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule principal = particulas.main;
            principal.duration = 1f;
            principal.loop = false;
            principal.playOnAwake = true;
            principal.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
            principal.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
            principal.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.22f);
            principal.startColor = new ParticleSystem.MinMaxGradient(colorA, colorB);
            principal.gravityModifier = gravedad;
            principal.simulationSpace = ParticleSystemSimulationSpace.World;
            principal.stopAction = ParticleSystemStopAction.Destroy;

            ParticleSystem.EmissionModule emision = particulas.emission;
            emision.rateOverTime = 0f;
            emision.SetBursts(new[] { new ParticleSystem.Burst(0f, 24) });

            ParticleSystem.ShapeModule forma = particulas.shape;
            forma.shapeType = ParticleSystemShapeType.Circle;
            forma.radius = 0.2f;

            Gradient desvanecer = new Gradient();
            desvanecer.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });

            ParticleSystem.ColorOverLifetimeModule colorVida = particulas.colorOverLifetime;
            colorVida.enabled = true;
            colorVida.color = new ParticleSystem.MinMaxGradient(desvanecer);

            ParticleSystemRenderer renderer = objeto.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.sortingOrder = 20;

            return GuardarPrefab(objeto);
        }

        static GameObject CrearPrefabFlor(GameObject petalos)
        {
            // Pétalo/flor pequeña tipo "dot" de Pac-Man.
            GameObject flor = CrearSprite("Cempasuchil", "Cempasuchil", Vector3.zero, 0.32f, 5);

            Rigidbody2D rb = flor.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            CircleCollider2D colision = flor.AddComponent<CircleCollider2D>();
            colision.isTrigger = true;
            colision.radius = 0.35f;

            Cempasuchil componente = flor.AddComponent<Cempasuchil>();
            componente.efectoPetalos = petalos;
            componente.sonidoRecoger = CargarAudio("Recoger.wav");
            componente.puntos = 10;

            CrearLuz("Brillo", flor.transform, Vector3.zero, Light2D.LightType.Point, ColorNaranja, 0.35f, 0.7f);

            return GuardarPrefab(flor);
        }

        static GameObject CrearPrefabFantasma(GameObject ectoplasma)
        {
            GameObject fantasma = CrearSprite("Fantasma", "Fantasma", Vector3.zero, 0.72f, 12);
            fantasma.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.85f);

            Rigidbody2D rb = fantasma.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            CircleCollider2D colision = fantasma.AddComponent<CircleCollider2D>();
            colision.isTrigger = true;
            colision.radius = 0.38f;

            Fantasma componente = fantasma.AddComponent<Fantasma>();
            componente.efectoDesvanecer = ectoplasma;
            componente.velocidad = 2.6f;

            CrearLuz("Aura", fantasma.transform, Vector3.zero, Light2D.LightType.Point, new Color(0.5f, 1f, 0.75f), 0.45f, 1.4f);

            return GuardarPrefab(fantasma);
        }

        static GameObject CrearPrefabCalaverita(GameObject destellos)
        {
            GameObject calaverita = CrearSprite("Calaverita", "Calaverita", Vector3.zero, 0.6f, 6);

            Rigidbody2D rb = calaverita.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            CircleCollider2D colision = calaverita.AddComponent<CircleCollider2D>();
            colision.isTrigger = true;
            colision.radius = 0.45f;

            CalaveritaBonus componente = calaverita.AddComponent<CalaveritaBonus>();
            componente.efecto = destellos;
            componente.sonidoBonus = CargarAudio("Bonus.wav");
            componente.puntos = 50;
            componente.duracionBonus = 6f;

            CrearLuz("Brillo", calaverita.transform, Vector3.zero, Light2D.LightType.Point, new Color(1f, 0.5f, 0.8f), 0.9f, 1.5f);

            return GuardarPrefab(calaverita);
        }

        // ---------- Escenario común ----------

        static void CrearCamara()
        {
            GameObject objeto = new GameObject("Main Camera");
            objeto.tag = "MainCamera";
            objeto.transform.position = new Vector3(0f, 0f, -10f);

            Camera camara = objeto.AddComponent<Camera>();
            camara.orthographic = true;
            camara.orthographicSize = 5f;
            camara.clearFlags = CameraClearFlags.SolidColor;
            camara.backgroundColor = ColorFondo;

            objeto.AddComponent<UniversalAdditionalCameraData>();
            objeto.AddComponent<AudioListener>();
        }

        static void CrearMusica(GameObject objeto)
        {
            AudioSource musica = objeto.AddComponent<AudioSource>();
            musica.clip = CargarAudio("MusicaPanteon.flac");
            musica.loop = true;
            musica.playOnAwake = true;
            musica.volume = 0.45f;
        }

        static void CrearPanteon(float intensidadGlobal, bool decoracionLibre = true)
        {
            Transform decoracion = new GameObject("Panteon").transform;

            CrearLuz("LuzGlobal", decoracion, Vector3.zero, Light2D.LightType.Global, new Color(0.65f, 0.6f, 0.95f), intensidadGlobal);

            float x = -8.4f;
            for (int i = 0; x <= 8.5f; i++, x += 1.2f)
            {
                GameObject banderita = CrearSprite("PapelPicado", "PapelPicado", new Vector3(x, 4.45f, 0f), 1.15f, -2, decoracion);
                banderita.GetComponent<SpriteRenderer>().color = ColoresPapelPicado[i % ColoresPapelPicado.Length];
                banderita.transform.rotation = Quaternion.Euler(0f, 0f, (i % 2 == 0 ? 4f : -4f));
            }

            if (!decoracionLibre)
                return;

            Vector3[] lapidas =
            {
                new Vector3(-7.2f, 2.4f), new Vector3(-4.6f, -2.6f), new Vector3(-1.8f, 2.7f),
                new Vector3(2.4f, -2.9f), new Vector3(5.3f, 2.3f), new Vector3(7.4f, -1.2f),
                new Vector3(-7.5f, -2.9f), new Vector3(0.6f, 0.9f)
            };
            foreach (Vector3 posicion in lapidas)
                CrearSprite("Lapida", "Lapida", posicion, Random.Range(0.9f, 1.2f), -5, decoracion);

            Vector3[] cruces = { new Vector3(-5.6f, 0.6f), new Vector3(3.8f, 0.4f), new Vector3(6.6f, 0.2f) };
            foreach (Vector3 posicion in cruces)
                CrearSprite("Cruz", "Cruz", posicion, 0.9f, -5, decoracion);

            Vector3[] calabazas = { new Vector3(-8.1f, -0.4f), new Vector3(8.1f, 1.3f), new Vector3(-2.9f, -0.9f) };
            foreach (Vector3 posicion in calabazas)
            {
                GameObject calabaza = CrearSprite("Calabaza", "Calabaza", posicion, 0.8f, -4, decoracion);
                CrearLuz("Brillo", calabaza.transform, Vector3.zero, Light2D.LightType.Point, ColorNaranja, 0.9f, 1.8f);
            }

            Vector3[] velas =
            {
                new Vector3(-6.5f, 2.0f), new Vector3(-1.2f, 2.3f), new Vector3(4.7f, 1.9f),
                new Vector3(1.8f, -3.3f), new Vector3(-3.9f, -3.2f), new Vector3(7.9f, -2.0f)
            };
            foreach (Vector3 posicion in velas)
            {
                GameObject vela = CrearSprite("Vela", "Vela", posicion, 0.5f, -3, decoracion);
                CrearLuz("Llama", vela.transform, new Vector3(0f, 0.25f, 0f), Light2D.LightType.Point, new Color(1f, 0.7f, 0.3f), 1f, 2.2f);
            }
        }

        // ---------- UI ----------

        static Transform CrearCanvas()
        {
            GameObject objeto = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            objeto.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler escalador = objeto.GetComponent<CanvasScaler>();
            escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escalador.referenceResolution = new Vector2(1920f, 1080f);
            escalador.matchWidthOrHeight = 0.5f;

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            return objeto.transform;
        }

        static RectTransform CrearRect(string nombre, Transform padre, Vector2 anclaMin, Vector2 anclaMax, Vector2 posicion, Vector2 tamano)
        {
            GameObject objeto = new GameObject(nombre, typeof(RectTransform));
            objeto.transform.SetParent(padre, false);

            RectTransform rect = (RectTransform)objeto.transform;
            rect.anchorMin = anclaMin;
            rect.anchorMax = anclaMax;
            rect.pivot = (anclaMin + anclaMax) * 0.5f;
            rect.anchoredPosition = posicion;
            rect.sizeDelta = tamano;

            return rect;
        }

        static TextMeshProUGUI CrearTexto(Transform padre, string nombre, string contenido, float tamano, Color color,
            TextAlignmentOptions alineacion, Vector2 ancla, Vector2 posicion, Vector2 tamanoRect)
        {
            RectTransform rect = CrearRect(nombre, padre, ancla, ancla, posicion, tamanoRect);
            TextMeshProUGUI texto = rect.gameObject.AddComponent<TextMeshProUGUI>();

            if (TMP_Settings.defaultFontAsset != null)
                texto.font = TMP_Settings.defaultFontAsset;

            texto.text = contenido;
            texto.fontSize = tamano;
            texto.color = color;
            texto.alignment = alineacion;
            texto.fontStyle = FontStyles.Bold;
            texto.raycastTarget = false;

            return texto;
        }

        static void CrearBoton(Transform padre, string etiqueta, Vector2 posicion, UnityAction accion)
        {
            Vector2 centro = new Vector2(0.5f, 0.5f);
            RectTransform rect = CrearRect("Boton" + etiqueta, padre, centro, centro, posicion, new Vector2(380f, 80f));

            Image imagen = rect.gameObject.AddComponent<Image>();
            imagen.color = Color.white;

            Button boton = rect.gameObject.AddComponent<Button>();
            ColorBlock colores = boton.colors;
            colores.normalColor = ColorMorado;
            colores.highlightedColor = ColorNaranja;
            colores.selectedColor = ColorMorado;
            colores.pressedColor = new Color(0.8f, 0.4f, 0.05f);
            boton.colors = colores;

            UnityEventTools.AddPersistentListener(boton.onClick, accion);

            TextMeshProUGUI texto = CrearTexto(rect, "Texto", etiqueta, 40f, Color.white,
                TextAlignmentOptions.Center, centro, Vector2.zero, new Vector2(380f, 80f));
            texto.raycastTarget = false;
        }

        static GameObject CrearPanel(Transform padre, string nombre)
        {
            RectTransform rect = CrearRect(nombre, padre, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            rect.gameObject.AddComponent<Image>().color = ColorPanel;
            return rect.gameObject;
        }

        // ---------- Escenas ----------

        static void CrearEscenaMenu()
        {
            Scene escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CrearCamara();
            CrearPanteon(0.75f);

            CrearSprite("CatrinaPortada", "Catrina", new Vector3(-6f, -1.6f, 0f), 2.6f, 10);
            CrearLuz("LuzCatrina", null, new Vector3(-6f, -1.6f, 0f), Light2D.LightType.Point, new Color(1f, 0.8f, 0.55f), 1f, 4f);

            GameObject fantasma = CrearSprite("FantasmaPortada", "Fantasma", new Vector3(6.2f, -0.8f, 0f), 2f, 12);
            fantasma.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.75f);

            CrearSprite("FlorPortada", "Cempasuchil", new Vector3(-3.4f, -3.5f, 0f), 0.8f, 5);
            CrearSprite("FlorPortada", "Cempasuchil", new Vector3(3.8f, -3.6f, 0f), 0.7f, 5);
            CrearSprite("CalaveritaPortada", "Calaverita", new Vector3(4.4f, 2.3f, 0f), 0.8f, 6);

            GameObject control = new GameObject("MenuPrincipal");
            MenuPrincipal menu = control.AddComponent<MenuPrincipal>();
            CrearMusica(control);

            Transform canvas = CrearCanvas();
            Vector2 centro = new Vector2(0.5f, 0.5f);

            CrearTexto(canvas, "Titulo", "NOCHE DE CEMPASÚCHIL", 100f, ColorNaranja,
                TextAlignmentOptions.Center, centro, new Vector2(0f, 300f), new Vector2(1600f, 140f));
            CrearTexto(canvas, "Subtitulo", "Pac-Man del panteón · Día de Muertos", 40f, new Color(1f, 0.55f, 0.8f),
                TextAlignmentOptions.Center, centro, new Vector2(0f, 205f), new Vector2(1400f, 70f));
            CrearTexto(canvas, "Instrucciones",
                "Son 3 noches, cada una con su mapa.\n" +
                "Recoge los puntos turquesa. El camino naranja es cempasúchil: frena fantasmas.\n" +
                "La calaverita los asusta. Pedro, Rosa, Tito y Luna quitan vidas.\n\n" +
                "WASD / Flechas: moverse     Esc: pausa",
                30f, Color.white, TextAlignmentOptions.Center, centro, new Vector2(0f, 40f), new Vector2(1200f, 260f));

            CrearBoton(canvas, "JUGAR", new Vector2(0f, -170f), menu.Jugar);
            CrearBoton(canvas, "SALIR", new Vector2(0f, -270f), menu.Salir);

            EditorSceneManager.SaveScene(escena, RutaMenu);
        }

        static void CrearEscenaJuego(GameObject flor, GameObject fantasma, GameObject calaverita, Sprite spriteMuro)
        {
            Scene escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CrearCamara();
            CrearPanteon(0.55f, decoracionLibre: false);

            // Laberinto (muros + flores + calaveritas)
            GameObject objetoLaberinto = new GameObject("Laberinto");
            Laberinto laberinto = objetoLaberinto.AddComponent<Laberinto>();
            laberinto.prefabFlor = flor;
            laberinto.prefabCalaverita = calaverita;
            laberinto.spriteMuro = spriteMuro;
            laberinto.tamanoCelda = PixelArte.Celda;

            // Catrina (jugador)
            GameObject catrina = CrearSprite("Catrina", "Catrina", Vector3.zero, 0.85f, 10);
            catrina.tag = "Player";

            Rigidbody2D rb = catrina.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

            CircleCollider2D colision = catrina.AddComponent<CircleCollider2D>();
            colision.isTrigger = true;
            colision.radius = 0.32f;

            catrina.AddComponent<AudioSource>().playOnAwake = false;

            Catrina componenteCatrina = catrina.AddComponent<Catrina>();
            componenteCatrina.sonidoDano = CargarAudio("Dano.wav");
            componenteCatrina.velocidad = 4.2f;

            CrearLuz("Farol", catrina.transform, Vector3.zero, Light2D.LightType.Point, new Color(1f, 0.85f, 0.6f), 0.9f, 2.8f);

            // Control, fantasmas y pausa
            GameObject objetoControl = new GameObject("ControlJuego");
            ControlJuego control = objetoControl.AddComponent<ControlJuego>();
            control.duracionPoder = 6f;
            CrearMusica(objetoControl);

            GeneradorObjetos generador = new GameObject("GeneradorObjetos").AddComponent<GeneradorObjetos>();
            generador.prefabFantasma = fantasma;

            MenuPausa pausa = new GameObject("MenuPausa").AddComponent<MenuPausa>();

            // HUD
            Transform canvas = CrearCanvas();

            RectTransform barra = CrearRect("BarraHUD", canvas, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 40f), new Vector2(0f, 80f));
            barra.pivot = new Vector2(0.5f, 0.5f);
            barra.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

            control.textoOfrenda = CrearTexto(barra, "TextoOfrenda", "OFRENDA: 0 / ?", 36f, ColorNaranja,
                TextAlignmentOptions.Left, new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(520f, 70f));
            control.textoPuntos = CrearTexto(barra, "TextoPuntos", "PUNTOS: 0", 36f, Color.white,
                TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400f, 70f));
            control.textoVidas = CrearTexto(barra, "TextoVidas", "VIDAS: 3", 36f, new Color(1f, 0.45f, 0.7f),
                TextAlignmentOptions.Right, new Vector2(1f, 0.5f), new Vector2(-40f, 0f), new Vector2(400f, 70f));

            Vector2 centro = new Vector2(0.5f, 0.5f);

            // Pausa
            GameObject panelPausa = CrearPanel(canvas, "PanelPausa");
            CrearTexto(panelPausa.transform, "Titulo", "PAUSA", 96f, ColorNaranja,
                TextAlignmentOptions.Center, centro, new Vector2(0f, 150f), new Vector2(1200f, 140f));
            CrearBoton(panelPausa.transform, "CONTINUAR", new Vector2(0f, -20f), pausa.Continuar);
            CrearBoton(panelPausa.transform, "MENÚ", new Vector2(0f, -120f), pausa.IrMenu);
            pausa.panelPausa = panelPausa;

            // Game Over
            GameObject panelGameOver = CrearPanel(canvas, "PanelGameOver");
            CrearTexto(panelGameOver.transform, "Titulo", "GAME OVER", 110f, new Color(0.7f, 0.9f, 1f),
                TextAlignmentOptions.Center, centro, new Vector2(0f, 200f), new Vector2(1400f, 150f));
            control.textoFinalGameOver = CrearTexto(panelGameOver.transform, "TextoFinal", "", 40f, Color.white,
                TextAlignmentOptions.Center, centro, new Vector2(0f, 50f), new Vector2(1400f, 180f));
            CrearBoton(panelGameOver.transform, "REINTENTAR", new Vector2(0f, -110f), control.Reiniciar);
            CrearBoton(panelGameOver.transform, "MENÚ", new Vector2(0f, -210f), control.IrMenu);
            control.panelGameOver = panelGameOver;

            // Victoria
            GameObject panelVictoria = CrearPanel(canvas, "PanelVictoria");
            CrearTexto(panelVictoria.transform, "Titulo", "¡OFRENDA COMPLETA!", 100f, ColorNaranja,
                TextAlignmentOptions.Center, centro, new Vector2(0f, 200f), new Vector2(1600f, 150f));
            control.textoFinalVictoria = CrearTexto(panelVictoria.transform, "TextoFinal", "", 40f, Color.white,
                TextAlignmentOptions.Center, centro, new Vector2(0f, 50f), new Vector2(1400f, 180f));
            CrearBoton(panelVictoria.transform, "JUGAR DE NUEVO", new Vector2(0f, -110f), control.Reiniciar);
            CrearBoton(panelVictoria.transform, "MENÚ", new Vector2(0f, -210f), control.IrMenu);
            control.panelVictoria = panelVictoria;

            panelPausa.SetActive(false);
            panelGameOver.SetActive(false);
            panelVictoria.SetActive(false);

            EditorSceneManager.SaveScene(escena, RutaJuego);
        }

        // ---------- Capturas para revisar el resultado ----------

        [MenuItem("Noche de Muertos/Capturar vista previa")]
        public static void CapturarVistaPrevia()
        {
            Directory.CreateDirectory("Capturas");

            EditorSceneManager.OpenScene(RutaMenu);
            Capturar("Capturas/MainMenu.png");

            EditorSceneManager.OpenScene(RutaJuego);
            Capturar("Capturas/Juego.png");

            EditorSceneManager.OpenScene(RutaMenu);
            Debug.Log("NocheDeMuertos: capturas guardadas en la carpeta Capturas del proyecto.");
        }

        static void Capturar(string ruta)
        {
            Camera camara = Camera.main;
            RenderTexture textura = new RenderTexture(1280, 720, 24);
            camara.targetTexture = textura;
            // El primer render en modo batch puede salir con texturas sin cargar.
            camara.Render();
            camara.Render();

            RenderTexture.active = textura;
            Texture2D imagen = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            imagen.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            imagen.Apply();

            camara.targetTexture = null;
            RenderTexture.active = null;

            File.WriteAllBytes(ruta, imagen.EncodeToPNG());
            Object.DestroyImmediate(imagen);
            Object.DestroyImmediate(textura);
        }
    }
}
