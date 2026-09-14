using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// One-click re-theme of the MainMenu scene to a dark fantasy look for "The Awakening".
/// Open the MainMenu scene, then use: Tools > Awakening > Apply Dark Fantasy Theme
/// Undo is fully supported (Edit > Undo) if you want to revert.
/// </summary>
public static class AwakeningDarkFantasyTheme
{
    // ---- Palette ----
    static readonly Color GoldText      = HexColor("#E7C77E");
    static readonly Color GoldTextDim   = HexColor("#9B7A3A");
    static readonly Color Parchment     = HexColor("#D8CBB0");
    static readonly Color BloodText     = HexColor("#D66A57");
    static readonly Color ShadowColor   = new Color(0f, 0f, 0f, 0.85f);

    const string TexRoot = "Assets/UI/DarkFantasy/Textures";

    [MenuItem("Tools/Awakening/Apply Dark Fantasy Theme")]
    public static void ApplyTheme()
    {
        GameObject background = GameObject.Find("Background");
        if (background == null)
        {
            EditorUtility.DisplayDialog("Awakening Theme",
                "Couldn't find a 'Background' GameObject.\nOpen Assets/Scenes/MainMenu.unity first, then run this again.",
                "OK");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(background, "Apply Dark Fantasy Theme");

        ThemeBackground(background);
        ThemeTitle(background);
        ThemeButtons(background);

        EditorUtility.SetDirty(background);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(background.scene);

        Debug.Log("<color=#E7C77E>The Awakening</color> — dark fantasy menu theme applied.");
    }

    // ------------------------------------------------------------
    static void ThemeBackground(GameObject background)
    {
        var img = background.GetComponent<Image>();
        if (img == null) return;

        Sprite bgSprite = LoadOrImportSprite($"{TexRoot}/background_dark_fantasy.png", Vector4.zero);
        if (bgSprite != null)
        {
            img.sprite = bgSprite;
            img.type = Image.Type.Simple;
            img.preserveAspect = false;
        }
        img.color = Color.white; // was semi-transparent white tint before
    }

    // ------------------------------------------------------------
    static void ThemeTitle(GameObject background)
    {
        Transform title = background.transform.Find("Title");
        if (title == null) return;

        var tmp = title.GetComponent<TextMeshProUGUI>();
        if (tmp == null) return;

        tmp.color = Color.white; // base color, gradient below carries the look
        tmp.enableVertexGradient = true;
        tmp.colorGradient = new VertexGradient(GoldText, GoldText, GoldTextDim, GoldTextDim);
        tmp.fontStyle = FontStyles.Bold;
        tmp.characterSpacing = 6f;
        tmp.text = tmp.text; // keep existing title text ("The Awakening")

        // Soft drop shadow + subtle outline glow using standard UI effects
        // (works with any font asset, no custom material required)
        var shadow = title.GetComponent<Shadow>();
        if (shadow == null) shadow = Undo.AddComponent<Shadow>(title.gameObject);
        shadow.effectColor = ShadowColor;
        shadow.effectDistance = new Vector2(3, -4);
        shadow.useGraphicAlpha = true;

        var outline = title.GetComponent<Outline>();
        if (outline == null) outline = Undo.AddComponent<Outline>(title.gameObject);
        outline.effectColor = new Color(GoldTextDim.r, GoldTextDim.g, GoldTextDim.b, 0.55f);
        outline.effectDistance = new Vector2(1.2f, -1.2f);
    }

    // ------------------------------------------------------------
    static void ThemeButtons(GameObject background)
    {
        Transform buttons = background.transform.Find("Buttons");
        if (buttons == null) return;

        Sprite normalSprite = LoadOrImportSprite($"{TexRoot}/button_normal.png", new Vector4(24, 24, 24, 24));
        Sprite hoverSprite  = LoadOrImportSprite($"{TexRoot}/button_hover.png", new Vector4(24, 24, 24, 24));
        Sprite pressSprite  = LoadOrImportSprite($"{TexRoot}/button_pressed.png", new Vector4(24, 24, 24, 24));
        Sprite quitNormal   = LoadOrImportSprite($"{TexRoot}/button_quit_normal.png", new Vector4(24, 24, 24, 24));
        Sprite quitHover    = LoadOrImportSprite($"{TexRoot}/button_quit_hover.png", new Vector4(24, 24, 24, 24));

        foreach (Transform child in buttons)
        {
            var button = child.GetComponent<Button>();
            var image = child.GetComponent<Image>();
            if (button == null || image == null) continue;

            bool isQuit = child.name.ToLower().Contains("quit");

            image.type = Image.Type.Sliced;
            image.sprite = isQuit ? quitNormal : normalSprite;
            image.color = Color.white;

            button.transition = Selectable.Transition.SpriteSwap;
            var spriteState = new SpriteState
            {
                highlightedSprite = isQuit ? quitHover : hoverSprite,
                pressedSprite     = isQuit ? quitHover : pressSprite,
                selectedSprite    = isQuit ? quitHover : hoverSprite,
                disabledSprite    = image.sprite
            };
            button.spriteState = spriteState;

            // Style the label
            var tmp = child.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.color = isQuit ? BloodText : Parchment;
                tmp.fontStyle = FontStyles.Bold;
                tmp.characterSpacing = 3f;

                var shadow = tmp.GetComponent<Shadow>();
                if (shadow == null) shadow = Undo.AddComponent<Shadow>(tmp.gameObject);
                shadow.effectColor = ShadowColor;
                shadow.effectDistance = new Vector2(1.5f, -1.5f);
                shadow.useGraphicAlpha = true;
            }
        }
    }

    // ------------------------------------------------------------
    /// <summary>Loads a sprite at path, importing/configuring it as a UI Sprite (with optional 9-slice border) if needed.</summary>
    static Sprite LoadOrImportSprite(string path, Vector4 border)
    {
        if (!System.IO.File.Exists(path))
        {
            Debug.LogWarning($"Awakening Theme: texture not found at {path}. Did you copy the Assets/UI/DarkFantasy folder into your project?");
            return null;
        }

        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            bool changed = false;
            if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; changed = true; }
            if (importer.spriteImportMode != SpriteImportMode.Single) { importer.spriteImportMode = SpriteImportMode.Single; changed = true; }
            if (importer.spriteBorder != border) { importer.spriteBorder = border; changed = true; }
            if (importer.mipmapEnabled) { importer.mipmapEnabled = false; changed = true; }
            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static Color HexColor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out var c);
        return c;
    }
}
