using UnityEngine;
using UnityEngine.UI;

public class LevelSelectButton : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Button button;

    [Header("Visual")]
    [SerializeField] private RectTransform buttonTransform;

    [SerializeField] private Image buttonImage;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;

    [SerializeField] private Color selectedColor =
        new Color(1f, 0.8f, 0.3f, 1f);

    [Header("Scale")]
    [SerializeField] private float normalScale = 1f;

    [SerializeField] private float selectedScale = 1.08f;

    private static LevelSelectButton selectedButton;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (buttonTransform == null)
        {
            buttonTransform =
                GetComponent<RectTransform>();
        }

        if (buttonImage == null)
        {
            buttonImage =
                GetComponent<Image>();
        }

        SetNormalVisual();
    }

    public void SelectButton()
    {
        if (selectedButton != null &&
            selectedButton != this)
        {
            selectedButton.SetNormalVisual();
        }

        selectedButton = this;

        SetSelectedVisual();
    }

    private void SetSelectedVisual()
    {
        if (buttonTransform != null)
        {
            buttonTransform.localScale =
                Vector3.one * selectedScale;
        }

        if (buttonImage != null)
        {
            buttonImage.color =
                selectedColor;
        }
    }

    private void SetNormalVisual()
    {
        if (buttonTransform != null)
        {
            buttonTransform.localScale =
                Vector3.one * normalScale;
        }

        if (buttonImage != null)
        {
            buttonImage.color =
                normalColor;
        }
    }
}