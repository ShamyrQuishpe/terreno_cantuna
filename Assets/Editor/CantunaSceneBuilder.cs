using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

// Genera una vez la escena editable y sus recursos nativos de Unity.
// Disponible también desde Herramientas > Cantuña > Crear escena.
public static class CantunaSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/AtrioIncompleto.unity";
    private const int TerrainResolution = 257;
    private const int TextureResolution = 128;
    private const float TerrainSide = 100f;
    private static Terrain terrain;

    [MenuItem("Herramientas/Cantuña/Crear escena")]
    public static void Build()
    {
        if (File.Exists(ToAbsolutePath(ScenePath)))
            throw new InvalidOperationException("La escena ya existe. Se conservaron los cambios que hayas hecho en ella.");

        EditorSettings.serializationMode = SerializationMode.ForceText;
        Directory.CreateDirectory(ToAbsolutePath("Assets/Scenes"));
        Directory.CreateDirectory(ToAbsolutePath("Assets/Materials"));
        Directory.CreateDirectory(ToAbsolutePath("Assets/Terrain"));
        Directory.CreateDirectory(ToAbsolutePath("Assets/Textures"));
        Directory.CreateDirectory(ToAbsolutePath("Docs"));

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Texture2D stoneTexture = MakeTexture("Piedra", new Color(0.50f, 0.49f, 0.47f), 3.2f);
        Texture2D earthTexture = MakeTexture("Tierra", new Color(0.38f, 0.29f, 0.20f), 5.7f);
        Texture2D grassTexture = MakeTexture("Pasto", new Color(0.26f, 0.40f, 0.22f), 8.1f);

        TerrainLayer stoneLayer = MakeTerrainLayer("Piedra del atrio", stoneTexture);
        TerrainLayer earthLayer = MakeTerrainLayer("Tierra de obra", earthTexture);
        TerrainLayer grassLayer = MakeTerrainLayer("Pasto de ladera", grassTexture);
        MakeTerrain(stoneLayer, earthLayer, grassLayer);

        Material lightStone = MakeMaterial("Piedra clara", new Color(0.65f, 0.61f, 0.52f));
        Material darkStone = MakeMaterial("Piedra volcanica", new Color(0.29f, 0.30f, 0.32f));
        Material paving = MakeMaterial("Losas", new Color(0.68f, 0.65f, 0.57f));
        Material playerRed = MakeMaterial("Jugador rojo", new Color(0.68f, 0.16f, 0.12f));
        Material playerNose = MakeMaterial("Frente del jugador", new Color(0.95f, 0.76f, 0.31f));

        GameObject sceneRoot = new GameObject("El atrio incompleto de Cantuna");
        GameObject facade = MakeGroup("01 - Fachada sencilla (5 cubos)", sceneRoot.transform);
        MakeCube("Torre izquierda", facade.transform, -12f, 30f, 0f, new Vector3(3f, 6f, 3f), darkStone);
        MakeCube("Torre derecha", facade.transform, 12f, 30f, 0f, new Vector3(3f, 6f, 3f), darkStone);
        MakeCube("Entrada izquierda", facade.transform, -3.2f, 30f, 0f, new Vector3(2.1f, 4f, 2f), lightStone);
        MakeCube("Entrada derecha", facade.transform, 3.2f, 30f, 0f, new Vector3(2.1f, 4f, 2f), lightStone);
        MakeCube("Dintel sobre la entrada", facade.transform, 0f, 30f, 4f, new Vector3(8.5f, 1f, 2.4f), lightStone);

        GameObject looseStones = MakeGroup("02 - Bloques de construccion (4 cubos)", sceneRoot.transform);
        MakeCube("Bloque izquierda 1", looseStones.transform, -19f, 12f, 0f, new Vector3(2f, 1.2f, 1.7f), darkStone);
        MakeCube("Bloque izquierda 2", looseStones.transform, -16f, 16f, 0f, new Vector3(1.7f, 1f, 1.7f), lightStone);
        MakeCube("Bloque derecha 1", looseStones.transform, 19f, 12f, 0f, new Vector3(2f, 1.2f, 1.7f), darkStone);
        MakeCube("Bloque derecha 2", looseStones.transform, 16f, 16f, 0f, new Vector3(1.7f, 1f, 1.7f), lightStone);

        GameObject slabs = MakeGroup("03 - La piedra faltante (3 cubos)", sceneRoot.transform);
        MakeCube("Losa 1", slabs.transform, 2.8f, -4f, 0.02f, new Vector3(2.1f, 0.16f, 2.1f), paving);
        MakeCube("Losa 2", slabs.transform, 5.2f, -4f, 0.02f, new Vector3(2.1f, 0.16f, 2.1f), paving);
        MakeCube("Losa 3", slabs.transform, 2.8f, -1.6f, 0.02f, new Vector3(2.1f, 0.16f, 2.1f), paving);

        GameObject player = new GameObject("Jugador - W S avanzar, A D girar");
        player.transform.SetParent(sceneRoot.transform);
        player.transform.position = new Vector3(0f, GroundHeight(0f, -20f) + 0.1f, -20f);
        CharacterController controller = player.AddComponent<CharacterController>();
        controller.center = new Vector3(0f, 0.9f, 0f);
        controller.height = 1.8f;
        controller.radius = 0.46f;
        controller.stepOffset = 0.3f;
        controller.slopeLimit = 45f;
        player.AddComponent<PlayerController>();

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Cuerpo rojo";
        body.transform.SetParent(player.transform, false);
        body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        body.transform.localScale = new Vector3(0.9f, 1.75f, 0.9f);
        body.GetComponent<Renderer>().sharedMaterial = playerRed;
        UnityEngine.Object.DestroyImmediate(body.GetComponent<BoxCollider>());

        GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
        nose.name = "Frente indica el eje Z";
        nose.transform.SetParent(player.transform, false);
        nose.transform.localPosition = new Vector3(0f, 1.05f, 0.5f);
        nose.transform.localScale = new Vector3(0.26f, 0.26f, 0.26f);
        nose.GetComponent<Renderer>().sharedMaterial = playerNose;
        UnityEngine.Object.DestroyImmediate(nose.GetComponent<BoxCollider>());

        GameObject cameraObject = new GameObject("Camara - tercera persona");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(player.transform, false);
        cameraObject.transform.localPosition = new Vector3(0f, 12f, -13f);
        cameraObject.transform.LookAt(player.transform.position + Vector3.forward * 12f);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 62f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 180f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.28f, 0.34f, 0.46f);
        cameraObject.AddComponent<AudioListener>();

        GameObject lightObject = new GameObject("Luz del amanecer");
        lightObject.transform.SetParent(sceneRoot.transform);
        lightObject.transform.rotation = Quaternion.Euler(35f, -38f, 0f);
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.78f, 0.58f);
        light.intensity = 1.1f;
        light.shadows = LightShadows.Soft;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.36f, 0.40f, 0.52f);

        AssetDatabase.SaveAssets();
        if (!EditorSceneManager.SaveScene(scene, ScenePath))
            throw new IOException("Unity no pudo guardar la escena.");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        VerifyScene();

        try
        {
            CapturePreview(UnityEngine.Object.FindFirstObjectByType<Camera>());
        }
        catch (Exception exception)
        {
            Debug.LogWarning("La escena se guardo, pero no se pudo capturar la vista previa: " + exception.Message);
        }

        Debug.Log("CANTUNA_BUILD_OK: escena, Terrain, 12 cubos de entorno y jugador creados.");
    }

    [MenuItem("Herramientas/Cantuña/Verificar escena")]
    public static void VerifyScene()
    {
        if (!File.Exists(ToAbsolutePath(ScenePath)))
            throw new FileNotFoundException("Falta la escena principal.", ScenePath);

        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Terrain builtTerrain = UnityEngine.Object.FindFirstObjectByType<Terrain>();
        if (builtTerrain == null || builtTerrain.terrainData == null || builtTerrain.GetComponent<TerrainCollider>() == null)
            throw new InvalidOperationException("Falta Terrain, TerrainData o TerrainCollider.");
        if (builtTerrain.terrainData.terrainLayers.Length < 2)
            throw new InvalidOperationException("Faltan las texturas pintables del Terrain.");
        if (builtTerrain.terrainData.size.x != 100f || builtTerrain.terrainData.size.z != 100f)
            throw new InvalidOperationException("El Terrain no mide 100 por 100 metros.");

        GameObject facade = GameObject.Find("01 - Fachada sencilla (5 cubos)");
        GameObject looseStones = GameObject.Find("02 - Bloques de construccion (4 cubos)");
        GameObject slabs = GameObject.Find("03 - La piedra faltante (3 cubos)");
        if (facade == null || looseStones == null || slabs == null)
            throw new InvalidOperationException("Faltan grupos de cubos del entorno.");
        int environmentCubeCount = CountCubes(facade) + CountCubes(looseStones) + CountCubes(slabs);
        if (environmentCubeCount != 12)
            throw new InvalidOperationException("Se esperaban 12 cubos de entorno y hay " + environmentCubeCount + ".");

        PlayerController player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        if (player == null || player.GetComponent<CharacterController>() == null || player.GetComponentInChildren<Camera>() == null)
            throw new InvalidOperationException("Falta el jugador, su controlador o la camara hija.");

        Debug.Log("CANTUNA_VERIFY_OK: Terrain 100 x 100, 3 capas, 12 cubos y jugador navegable.");
    }

    [MenuItem("Herramientas/Cantuña/Guardar vista previa")]
    public static void CaptureSavedPreview()
    {
        VerifyScene();
        Camera camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
        if (camera == null)
            throw new InvalidOperationException("No se encontro la camara del jugador.");
        CapturePreview(camera);
        Debug.Log("CANTUNA_PREVIEW_OK: Docs/VistaJuego.png");
    }

    // Actualiza la primera versión generada sin borrar ni reconstruir la escena.
    [MenuItem("Herramientas/Cantuña/Ajustar texturas y losas")]
    public static void PolishScene()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        terrain = UnityEngine.Object.FindFirstObjectByType<Terrain>();
        if (terrain == null)
            throw new InvalidOperationException("No se encontro el Terrain de la escena.");

        MakeTexture("Piedra", new Color(0.50f, 0.49f, 0.47f), 3.2f);
        MakeTexture("Tierra", new Color(0.38f, 0.29f, 0.20f), 5.7f);
        MakeTexture("Pasto", new Color(0.26f, 0.40f, 0.22f), 8.1f);

        GameObject slabs = GameObject.Find("03 - La piedra faltante (3 cubos)");
        if (slabs == null)
            throw new InvalidOperationException("No se encontraron las tres losas.");
        MoveSlab(slabs.transform.Find("Losa 1"), 2.8f, -4f);
        MoveSlab(slabs.transform.Find("Losa 2"), 5.2f, -4f);
        MoveSlab(slabs.transform.Find("Losa 3"), 2.8f, -1.6f);

        TerrainData data = terrain.terrainData;
        int resolution = data.alphamapResolution;
        float[,,] paint = data.GetAlphamaps(0, 0, resolution, resolution);
        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float worldX = -50f + (float)x / (resolution - 1) * TerrainSide;
                float worldZ = -50f + (float)z / (resolution - 1) * TerrainSide;
                if (Mathf.Abs(worldX - 7f) < 0.65f && Mathf.Abs(worldZ - 7f) < 0.65f)
                {
                    paint[z, x, 0] = 1f;
                    paint[z, x, 1] = 0f;
                    paint[z, x, 2] = 0f;
                }
                if (Mathf.Abs(worldX - 5.2f) < 1.05f && Mathf.Abs(worldZ + 1.6f) < 1.05f)
                {
                    paint[z, x, 0] = 0f;
                    paint[z, x, 1] = 1f;
                    paint[z, x, 2] = 0f;
                }
            }
        }
        data.SetAlphamaps(0, 0, paint);
        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();
        if (!EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene()))
            throw new IOException("No se pudo guardar el ajuste de las losas.");
        CaptureSavedPreview();
        Debug.Log("CANTUNA_POLISH_OK: texturas y piedra faltante visibles.");
    }

    private static void MoveSlab(Transform slab, float x, float z)
    {
        if (slab == null)
            throw new InvalidOperationException("Falta una de las losas de la escena.");
        slab.localScale = new Vector3(2.1f, 0.16f, 2.1f);
        slab.position = new Vector3(x, GroundHeight(x, z) + 0.1f, z);
    }

    private static int CountCubes(GameObject group)
    {
        return group.GetComponentsInChildren<MeshRenderer>(true).Length;
    }

    private static string ToAbsolutePath(string projectRelativePath)
    {
        return Path.Combine(Path.GetDirectoryName(Application.dataPath), projectRelativePath);
    }

    private static GameObject MakeGroup(string name, Transform parent)
    {
        GameObject group = new GameObject(name);
        group.transform.SetParent(parent);
        return group;
    }

    private static GameObject MakeCube(string name, Transform parent, float x, float z, float bottomOffset, Vector3 scale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent);
        cube.transform.position = new Vector3(x, GroundHeight(x, z) + bottomOffset + scale.y * 0.5f, z);
        cube.transform.localScale = scale;
        cube.GetComponent<Renderer>().sharedMaterial = material;
        return cube;
    }

    private static float GroundHeight(float x, float z)
    {
        return terrain.SampleHeight(new Vector3(x, 0f, z)) + terrain.transform.position.y;
    }

    private static Texture2D MakeTexture(string name, Color baseColor, float seed)
    {
        const int side = 64;
        Texture2D texture = new Texture2D(side, side, TextureFormat.RGB24, true);
        Color[] pixels = new Color[side * side];
        for (int y = 0; y < side; y++)
        {
            for (int x = 0; x < side; x++)
            {
                float variation = Mathf.PerlinNoise(seed + x * 0.12f, seed + y * 0.12f);
                float factor = Mathf.Lerp(0.76f, 1.17f, variation);
                pixels[y * side + x] = new Color(baseColor.r * factor, baseColor.g * factor, baseColor.b * factor);
            }
        }
        texture.SetPixels(pixels);
        texture.Apply();

        string assetPath = "Assets/Textures/" + name + ".png";
        File.WriteAllBytes(ToAbsolutePath(assetPath), texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
        Texture2D imported = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        if (imported == null)
            throw new IOException("Unity no pudo importar la textura " + assetPath);
        return imported;
    }

    private static TerrainLayer MakeTerrainLayer(string name, Texture2D texture)
    {
        TerrainLayer layer = new TerrainLayer
        {
            diffuseTexture = texture,
            tileSize = new Vector2(7f, 7f),
            name = name
        };
        AssetDatabase.CreateAsset(layer, "Assets/Terrain/" + name + ".terrainlayer");
        return layer;
    }

    private static Material MakeMaterial(string name, Color color)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null)
            throw new InvalidOperationException("Falta el shader Standard de Unity para los cubos.");
        Material material = new Material(shader) { name = name, color = color };
        AssetDatabase.CreateAsset(material, "Assets/Materials/" + name + ".mat");
        return material;
    }

    private static void MakeTerrain(TerrainLayer stone, TerrainLayer earth, TerrainLayer grass)
    {
        TerrainData data = new TerrainData
        {
            heightmapResolution = TerrainResolution,
            alphamapResolution = TextureResolution,
            size = new Vector3(TerrainSide, 20f, TerrainSide)
        };

        float[,] heights = new float[TerrainResolution, TerrainResolution];
        for (int z = 0; z < TerrainResolution; z++)
        {
            for (int x = 0; x < TerrainResolution; x++)
            {
                float worldX = -50f + (float)x / (TerrainResolution - 1) * TerrainSide;
                float worldZ = -50f + (float)z / (TerrainResolution - 1) * TerrainSide;
                float distanceFromCenter = Mathf.Max(Mathf.Abs(worldX), Mathf.Abs(worldZ));
                float rim = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((distanceFromCenter - 28f) / 18f));
                float smallVariation = Mathf.PerlinNoise(x * 0.035f, z * 0.035f) * 0.055f;
                heights[z, x] = 0.045f + rim * (0.19f + smallVariation);
            }
        }
        data.SetHeights(0, 0, heights);
        data.terrainLayers = new[] { stone, earth, grass };

        float[,,] paint = new float[TextureResolution, TextureResolution, 3];
        for (int z = 0; z < TextureResolution; z++)
        {
            for (int x = 0; x < TextureResolution; x++)
            {
                float worldX = -50f + (float)x / (TextureResolution - 1) * TerrainSide;
                float worldZ = -50f + (float)z / (TextureResolution - 1) * TerrainSide;
                float distance = Mathf.Max(Mathf.Abs(worldX), Mathf.Abs(worldZ));
                float stoneAmount = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((distance - 24f) / 6f));
                float earthAmount = (1f - stoneAmount) * 0.68f;
                float grassAmount = (1f - stoneAmount) * 0.32f;

                // Cuarta posición: suelo oscuro al ras, sin un agujero en el Terrain.
                if (Mathf.Abs(worldX - 5.2f) < 1.05f && Mathf.Abs(worldZ + 1.6f) < 1.05f)
                {
                    stoneAmount = 0f;
                    earthAmount = 1f;
                    grassAmount = 0f;
                }

                paint[z, x, 0] = stoneAmount;
                paint[z, x, 1] = earthAmount;
                paint[z, x, 2] = grassAmount;
            }
        }
        data.SetAlphamaps(0, 0, paint);
        data.name = "Terreno - 100 x 100";
        AssetDatabase.CreateAsset(data, "Assets/Terrain/Terreno.asset");

        GameObject terrainObject = Terrain.CreateTerrainGameObject(data);
        terrainObject.name = "Terrain - plaza y laderas";
        terrainObject.transform.position = new Vector3(-50f, 0f, -50f);
        terrain = terrainObject.GetComponent<Terrain>();
    }

    private static void CapturePreview(Camera playerCamera)
    {
        const int width = 1280;
        const int height = 720;
        RenderTexture target = new RenderTexture(width, height, 24);
        Texture2D image = new Texture2D(width, height, TextureFormat.RGB24, false);
        RenderTexture previous = RenderTexture.active;
        RenderTexture previousTarget = playerCamera.targetTexture;
        try
        {
            playerCamera.targetTexture = target;
            playerCamera.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            image.Apply();
            File.WriteAllBytes(ToAbsolutePath("Docs/VistaJuego.png"), image.EncodeToPNG());
        }
        finally
        {
            playerCamera.targetTexture = previousTarget;
            RenderTexture.active = previous;
            UnityEngine.Object.DestroyImmediate(image);
            UnityEngine.Object.DestroyImmediate(target);
        }
    }
}
