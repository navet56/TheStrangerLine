using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapSetup : MonoBehaviour
{
    [Header("Auto-generated references")]
    public Map playerMap;

    [ContextMenu("Create Map UI")]
    public void CreateMapUI()
    {
        GameObject canvasObj = new GameObject("MapCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Fond semi-transparent (cache en mode minimap)
        GameObject background = new GameObject("Background");
        background.transform.SetParent(canvasObj.transform, false);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f);
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Zone de limite pour le mode plein ecran
        GameObject sizeLimit = new GameObject("SizeLimit");
        sizeLimit.transform.SetParent(canvasObj.transform, false);
        RectTransform limitRect = sizeLimit.AddComponent<RectTransform>();
        limitRect.anchorMin = new Vector2(0.05f, 0.08f);
        limitRect.anchorMax = new Vector2(0.95f, 0.92f);
        limitRect.offsetMin = Vector2.zero;
        limitRect.offsetMax = Vector2.zero;

        // Conteneur carre
        GameObject mapContainer = new GameObject("MapContainer");
        mapContainer.transform.SetParent(sizeLimit.transform, false);
        RectTransform containerRect = mapContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;
        containerRect.pivot = new Vector2(0.5f, 0.5f);

        AspectRatioFitter fitter = mapContainer.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = 1f;

        // Bordure
        GameObject border = new GameObject("Border");
        border.transform.SetParent(mapContainer.transform, false);
        Image borderImage = border.AddComponent<Image>();
        borderImage.color = new Color(0.3f, 0.2f, 0.1f);
        RectTransform borderRect = border.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-10, -10);
        borderRect.offsetMax = new Vector2(10, 10);

        // Image de la carte
        GameObject mapImageObj = new GameObject("MapImage");
        mapImageObj.transform.SetParent(mapContainer.transform, false);
        RawImage mapImage = mapImageObj.AddComponent<RawImage>();
        RectTransform mapRect = mapImageObj.GetComponent<RectTransform>();
        mapRect.anchorMin = Vector2.zero;
        mapRect.anchorMax = Vector2.one;
        mapRect.offsetMin = Vector2.zero;
        mapRect.offsetMax = Vector2.zero;

        // Marqueur joueur
        GameObject markerObj = new GameObject("PlayerMarker");
        markerObj.transform.SetParent(mapImageObj.transform, false);
        Image markerImage = markerObj.AddComponent<Image>();
        markerImage.color = Color.red;
        RectTransform markerRect = markerObj.GetComponent<RectTransform>();
        markerRect.sizeDelta = new Vector2(15, 15);
        CreateTriangleSprite(markerImage);

        // Texte d'aide en bas (cache en mode minimap)
        GameObject helpText = new GameObject("HelpText");
        helpText.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI tmp = helpText.AddComponent<TextMeshProUGUI>();
        tmp.text = "LMB: Draw | RMB: Erase | 1-5: Colors | Scroll: Brush size | R: Minimap | E: Close";
        tmp.fontSize = 18;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        RectTransform helpRect = helpText.GetComponent<RectTransform>();
        helpRect.anchorMin = new Vector2(0, 0);
        helpRect.anchorMax = new Vector2(1, 0);
        helpRect.pivot = new Vector2(0.5f, 0);
        helpRect.anchoredPosition = new Vector2(0, 10);
        helpRect.sizeDelta = new Vector2(0, 30);

        // Palette en haut (cache en mode minimap)
        GameObject palette = CreateColorPalette(canvasObj.transform);

        if (playerMap != null)
        {
            playerMap.mapCanvas = canvasObj;
            playerMap.mapImage = mapImage;
            playerMap.playerMarker = markerRect;
            playerMap.mapContainer = containerRect;
            playerMap.sizeLimit = limitRect;
            playerMap.background = background;
            playerMap.helpText = helpText;
            playerMap.colorPalette = palette;
        }

        canvasObj.SetActive(false);
        Debug.Log("Map UI created.");
    }

    private GameObject CreateColorPalette(Transform canvasTransform)
    {
        GameObject palette = new GameObject("ColorPalette");
        palette.transform.SetParent(canvasTransform, false);
        HorizontalLayoutGroup layout = palette.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 10;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        RectTransform paletteRect = palette.GetComponent<RectTransform>();
        paletteRect.anchorMin = new Vector2(0.5f, 1);
        paletteRect.anchorMax = new Vector2(0.5f, 1);
        paletteRect.pivot = new Vector2(0.5f, 1);
        paletteRect.anchoredPosition = new Vector2(0, -10);
        paletteRect.sizeDelta = new Vector2(300, 40);

        Color[] colors = new Color[]
        {
            Color.black,
            Color.red,
            Color.blue,
            new Color(0.0f, 0.5f, 0.0f),
            new Color(0.6f, 0.3f, 0.0f)
        };

        for (int i = 0; i < colors.Length; i++)
        {
            GameObject colorBtn = new GameObject("Color_" + (i + 1));
            colorBtn.transform.SetParent(palette.transform, false);
            Image btnImage = colorBtn.AddComponent<Image>();
            btnImage.color = colors[i];

            LayoutElement layoutElement = colorBtn.AddComponent<LayoutElement>();
            layoutElement.minWidth = 40;
            layoutElement.minHeight = 40;

            GameObject numText = new GameObject("Number");
            numText.transform.SetParent(colorBtn.transform, false);
            TextMeshProUGUI num = numText.AddComponent<TextMeshProUGUI>();
            num.text = (i + 1).ToString();
            num.fontSize = 16;
            num.alignment = TextAlignmentOptions.Center;
            num.color = colors[i] == Color.black ? Color.white : Color.black;
            RectTransform numRect = numText.GetComponent<RectTransform>();
            numRect.anchorMin = Vector2.zero;
            numRect.anchorMax = Vector2.one;
            numRect.sizeDelta = Vector2.zero;
        }

        return palette;
    }

    private void CreateTriangleSprite(Image image)
    {
        Texture2D tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[32 * 32];

        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float nx = (x - 16) / 16f;
                float ny = (y - 16) / 16f;

                bool inTriangle = ny > -0.5f &&
                                  ny < 0.8f &&
                                  Mathf.Abs(nx) < (0.8f - ny) * 0.6f;

                pixels[y * 32 + x] = inTriangle ? Color.white : Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        image.sprite = sprite;
    }
}