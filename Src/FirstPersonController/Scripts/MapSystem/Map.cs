using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Map : MonoBehaviour
{
    public enum MapState { Fullscreen, Minimap }

    [Header("UI References")]
    public GameObject mapCanvas;
    public RawImage mapImage;
    public RectTransform playerMarker;
    public RectTransform mapContainer;
    public RectTransform sizeLimit;
    public GameObject background;
    public GameObject helpText;
    public GameObject colorPalette;
    public RawImage gridOverlay;

    [Header("Map Settings")]
    public int mapResolution = 512;
    public Color backgroundColor = new Color(0.9f, 0.85f, 0.7f);
    public Color brushColor = Color.black;
    [Range(1, 20)]
    public int brushSize = 3;

    [Header("Brush Palette")]
    public Color[] brushColors = new Color[]
    {
        Color.black,
        Color.red,
        Color.blue,
        new Color(0.0f, 0.5f, 0.0f),
        new Color(0.6f, 0.3f, 0.0f)
    };

    [Header("Player Marker")]
    public bool showPlayerMarker = true;

    [Header("World Mapping")]
    public Vector2 worldBoundsMin = new Vector2(-250, -250);
    public Vector2 worldBoundsMax = new Vector2(250, 250);

    [Header("Minimap Settings")]
    [Range(0.1f, 0.4f)]
    public float minimapSize = 0.2f;
    public float minimapMargin = 20f;

    [Header("Grid Overlay")]
    public bool showGrid = true;
    public Color gridColor = new Color(0.2f, 0.15f, 0.1f, 0.8f);
    [Range(1, 10)]
    public int gridLineWidth = 4;

    [Header("Debug")]
    public bool showDebugBounds = true;

    private Texture2D mapTexture;
    private Texture2D gridTexture;
    private MapState currentState = MapState.Minimap;
    private bool isHudVisible = true;
    private Vector2 lastDrawPos;
    private bool hasLastPos;
    private Color[] currentPixels;
    private bool textureNeedsApply;

#if ENABLE_INPUT_SYSTEM
    private PlayerInput playerInput;
    private InputAction mapAction;
#endif

    private void Start()
    {
        InitializeMap();
        CreateGridTexture();
        SetupInput();
        ApplyState();
    }

    private void InitializeMap()
    {
        mapTexture = new Texture2D(mapResolution, mapResolution, TextureFormat.RGBA32, false);
        mapTexture.filterMode = FilterMode.Point;

        currentPixels = new Color[mapResolution * mapResolution];
        for (int i = 0; i < currentPixels.Length; i++)
            currentPixels[i] = backgroundColor;

        mapTexture.SetPixels(currentPixels);
        mapTexture.Apply();

        if (mapImage != null)
            mapImage.texture = mapTexture;

        LoadMap();
    }

    private void CreateGridTexture()
    {
        gridTexture = new Texture2D(mapResolution, mapResolution, TextureFormat.RGBA32, false);
        gridTexture.filterMode = FilterMode.Bilinear;

        Color[] gridPixels = new Color[mapResolution * mapResolution];

        for (int i = 0; i < gridPixels.Length; i++)
            gridPixels[i] = Color.clear;

        int cellSize = mapResolution / 3;

        for (int line = 1; line <= 2; line++)
        {
            int lineCenter = line * cellSize;

            for (int offset = -gridLineWidth; offset <= gridLineWidth; offset++)
            {
                int hPos = lineCenter + offset;
                if (hPos < 0 || hPos >= mapResolution) continue;

                for (int i = 0; i < mapResolution; i++)
                {
                    gridPixels[i * mapResolution + hPos] = gridColor;
                    gridPixels[hPos * mapResolution + i] = gridColor;
                }
            }
        }

        gridTexture.SetPixels(gridPixels);
        gridTexture.Apply();

        if (gridOverlay != null)
        {
            gridOverlay.texture = gridTexture;
            gridOverlay.gameObject.SetActive(showGrid);
            Debug.Log("Grid texture applied to overlay");
        }
        else
        {
            Debug.LogWarning("gridOverlay is not assigned! Re-run Create Map UI in MapSetup.");
        }
    }

    private void SetupInput()
    {
#if ENABLE_INPUT_SYSTEM
        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
            mapAction = playerInput.actions.FindAction("Map");
#endif
    }

    private void Update()
    {
        HandleInput();

        if (isHudVisible)
        {
            if (currentState == MapState.Fullscreen)
            {
                HandleDrawing();
                HandleBrushControls();
            }
            UpdatePlayerMarker();
        }

        if (textureNeedsApply)
        {
            mapTexture.SetPixels(currentPixels);
            mapTexture.Apply();
            textureNeedsApply = false;
        }
    }

    private void HandleInput()
    {
        bool toggleMap = false;
        bool toggleHud = false;

#if ENABLE_INPUT_SYSTEM
        if (mapAction != null)
            toggleMap = mapAction.WasPressedThisFrame();
        else if (Keyboard.current != null)
            toggleMap = Keyboard.current.eKey.wasPressedThisFrame;

        if (Keyboard.current != null)
            toggleHud = Keyboard.current.hKey.wasPressedThisFrame;
#else
        toggleMap = Input.GetKeyDown(KeyCode.E);
        toggleHud = Input.GetKeyDown(KeyCode.H);
#endif

        if (toggleMap)
            ToggleMapState();

        if (toggleHud)
            ToggleHud();
    }

    public void ToggleMapState()
    {
        if (!isHudVisible)
            return;

        currentState = (currentState == MapState.Fullscreen) ? MapState.Minimap : MapState.Fullscreen;
        ApplyState();
    }

    public void ToggleHud()
    {
        if (currentState == MapState.Fullscreen)
            currentState = MapState.Minimap;

        isHudVisible = !isHudVisible;

        if (mapCanvas != null)
            mapCanvas.SetActive(isHudVisible);

        if (!isHudVisible)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
            hasLastPos = false;
            SaveMap();
        }
        else
        {
            ApplyState();
        }
    }

    private void ApplyState()
    {
        if (mapCanvas != null)
            mapCanvas.SetActive(isHudVisible);

        if (!isHudVisible)
            return;

        bool fullscreen = (currentState == MapState.Fullscreen);

        if (background != null)
            background.SetActive(fullscreen);

        if (helpText != null)
            helpText.SetActive(fullscreen);

        if (colorPalette != null)
            colorPalette.SetActive(fullscreen);

        if (gridOverlay != null)
            gridOverlay.gameObject.SetActive(showGrid);

        if (sizeLimit != null)
        {
            if (fullscreen)
            {
                sizeLimit.anchorMin = new Vector2(0.05f, 0.08f);
                sizeLimit.anchorMax = new Vector2(0.95f, 0.92f);
                sizeLimit.offsetMin = Vector2.zero;
                sizeLimit.offsetMax = Vector2.zero;
            }
            else
            {
                sizeLimit.anchorMin = new Vector2(1 - minimapSize, 0);
                sizeLimit.anchorMax = new Vector2(1, minimapSize);
                sizeLimit.offsetMin = new Vector2(-minimapMargin, minimapMargin);
                sizeLimit.offsetMax = new Vector2(-minimapMargin, minimapMargin);
            }
        }

        if (fullscreen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
            hasLastPos = false;
            SaveMap();
        }
    }

    private void HandleDrawing()
    {
        bool drawing = false;
        bool erasing = false;
        Vector2 mousePos;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            drawing = Mouse.current.leftButton.isPressed;
            erasing = Mouse.current.rightButton.isPressed;
            mousePos = Mouse.current.position.ReadValue();
        }
        else return;
#else
        drawing = Input.GetMouseButton(0);
        erasing = Input.GetMouseButton(1);
        mousePos = Input.mousePosition;
#endif

        if (drawing || erasing)
        {
            RectTransform rectTransform = mapImage.rectTransform;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, mousePos, null, out Vector2 localPoint))
            {
                Rect rect = rectTransform.rect;
                float normalizedX = (localPoint.x - rect.x) / rect.width;
                float normalizedY = (localPoint.y - rect.y) / rect.height;

                if (normalizedX >= 0 && normalizedX <= 1 && normalizedY >= 0 && normalizedY <= 1)
                {
                    int x = Mathf.FloorToInt(normalizedX * mapResolution);
                    int y = Mathf.FloorToInt(normalizedY * mapResolution);
                    Color colorToUse = erasing ? backgroundColor : brushColor;

                    if (hasLastPos)
                        DrawLine((int)lastDrawPos.x, (int)lastDrawPos.y, x, y, colorToUse);
                    else
                        DrawBrush(x, y, colorToUse);

                    lastDrawPos = new Vector2(x, y);
                    hasLastPos = true;
                }
            }
        }
        else
        {
            hasLastPos = false;
        }
    }

    private void DrawBrush(int centerX, int centerY, Color color)
    {
        int radius = brushSize;

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    int px = centerX + x;
                    int py = centerY + y;

                    if (px >= 0 && px < mapResolution && py >= 0 && py < mapResolution)
                        currentPixels[py * mapResolution + px] = color;
                }
            }
        }
        textureNeedsApply = true;
    }

    private void DrawLine(int x0, int y0, int x1, int y1, Color color)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            DrawBrush(x0, y0, color);
            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

    private void HandleBrushControls()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            for (int i = 0; i < brushColors.Length && i < 9; i++)
            {
                Key numKey = (Key)((int)Key.Digit1 + i);
                if (Keyboard.current[numKey].wasPressedThisFrame)
                {
                    brushColor = brushColors[i];
                    break;
                }
            }
        }

        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.y.ReadValue();
            if (scroll > 0) brushSize = Mathf.Min(brushSize + 1, 20);
            else if (scroll < 0) brushSize = Mathf.Max(brushSize - 1, 1);
        }
#else
        for (int i = 0; i < brushColors.Length && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                brushColor = brushColors[i];
                break;
            }
        }

        float scroll = Input.mouseScrollDelta.y;
        if (scroll > 0) brushSize = Mathf.Min(brushSize + 1, 20);
        else if (scroll < 0) brushSize = Mathf.Max(brushSize - 1, 1);
#endif
    }

    private void UpdatePlayerMarker()
    {
        if (!showPlayerMarker || playerMarker == null || mapImage == null) return;

        Vector3 playerPos = transform.position;
        float worldWidth = worldBoundsMax.x - worldBoundsMin.x;
        float worldHeight = worldBoundsMax.y - worldBoundsMin.y;

        if (worldWidth <= 0 || worldHeight <= 0) return;

        float normalizedX = Mathf.Clamp01((playerPos.x - worldBoundsMin.x) / worldWidth);
        float normalizedY = Mathf.Clamp01((playerPos.z - worldBoundsMin.y) / worldHeight);

        Rect mapRect = mapImage.rectTransform.rect;
        playerMarker.anchoredPosition = new Vector2(
            (normalizedX - 0.5f) * mapRect.width,
            (normalizedY - 0.5f) * mapRect.height
        );
        playerMarker.localRotation = Quaternion.Euler(0, 0, -transform.eulerAngles.y);
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugBounds) return;

        Vector3 min = new Vector3(worldBoundsMin.x, 0, worldBoundsMin.y);
        Vector3 max = new Vector3(worldBoundsMax.x, 0, worldBoundsMax.y);
        Vector3 size = max - min;
        Vector3 center = min + size * 0.5f;
        center.y = transform.position.y;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(center, new Vector3(size.x, 1, size.z));
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(new Vector3(worldBoundsMin.x, center.y, worldBoundsMin.y), 2f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(new Vector3(worldBoundsMax.x, center.y, worldBoundsMax.y), 2f);
    }

    public void AutoConfigureFromTerrain(Terrain terrain)
    {
        if (terrain == null) return;
        Vector3 pos = terrain.transform.position;
        Vector3 size = terrain.terrainData.size;
        worldBoundsMin = new Vector2(pos.x, pos.z);
        worldBoundsMax = new Vector2(pos.x + size.x, pos.z + size.z);
    }

    public void AutoConfigureFromBounds(Bounds bounds)
    {
        worldBoundsMin = new Vector2(bounds.min.x, bounds.min.z);
        worldBoundsMax = new Vector2(bounds.max.x, bounds.max.z);
    }

    public void ClearMap()
    {
        for (int i = 0; i < currentPixels.Length; i++)
            currentPixels[i] = backgroundColor;
        textureNeedsApply = true;
    }

    public void SetGridVisible(bool visible)
    {
        showGrid = visible;
        if (gridOverlay != null)
            gridOverlay.gameObject.SetActive(visible);
    }

    public void SaveMap()
    {
        byte[] data = mapTexture.EncodeToPNG();
        string path = System.IO.Path.Combine(Application.persistentDataPath, "playermap.png");
        System.IO.File.WriteAllBytes(path, data);
    }

    public void LoadMap()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "playermap.png");
        if (System.IO.File.Exists(path))
        {
            byte[] data = System.IO.File.ReadAllBytes(path);
            mapTexture.LoadImage(data);
            currentPixels = mapTexture.GetPixels();
        }
    }

    private void OnDestroy()
    {
        if (mapTexture != null)
            Destroy(mapTexture);
        if (gridTexture != null)
            Destroy(gridTexture);
    }

    private void OnApplicationQuit()
    {
        SaveMap();
    }
}