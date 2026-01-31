using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SetScreenUIPortrait : MonoBehaviour
{
    [Header(" Setting Canvas ")]
    [SerializeField] private CanvasScaler canvasUi;
    [SerializeField] private EventSystem dragHold;
    private float scaler;
    private int width;
    private int height;

    private float GetScale(int width, int height, 
        Vector2 scalerReferenceResolution, float scalerMatchWidthOrHeight)
    {
        return  Mathf.Pow(width / scalerReferenceResolution.x, 1f - scalerMatchWidthOrHeight) *
                Mathf.Pow(height / scalerReferenceResolution.y, scalerMatchWidthOrHeight);
    }

    private void Awake()
    {
        SetCanvasScaler();
    }

    private void Start() { }

    public void SetCanvasScaler()
    {
        height = Screen.height;
        width = Screen.width;
        float sceneScale = 9f / 20;

        scaler = GetScale(width, height, new Vector2(width, height), 1f);

        if (scaler >= sceneScale)
        {
            if (canvasUi != null)
            {
                canvasUi.matchWidthOrHeight = 0f;
            }
        }
        else
        {
            if (canvasUi != null)
            {
                canvasUi.matchWidthOrHeight = 1f;
            }
        }
    }
}
