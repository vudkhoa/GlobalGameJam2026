using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;

namespace ScratchCard.Editor
{
    /// <summary>
    /// Editor utility to quickly setup scratch card in scene
    /// Menu: GameObject > UI > Scratch Card
    /// </summary>
    public static class ScratchCardSetup
    {
        [MenuItem("GameObject/UI/Scratch Card", false, 10)]
        private static void CreateScratchCard(MenuCommand menuCommand)
        {
            // Get or create Canvas
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                
                Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
            }
            
            // Create main container
            GameObject container = new GameObject("ScratchCard");
            RectTransform containerRect = container.AddComponent<RectTransform>();
            container.transform.SetParent(canvas.transform, false);
            
            // Set size (400x600 default)
            containerRect.sizeDelta = new Vector2(400, 600);
            containerRect.anchoredPosition = Vector2.zero;
            
            // Create Result Layer
            GameObject resultLayer = new GameObject("ResultLayer");
            resultLayer.transform.SetParent(container.transform, false);
            RectTransform resultRect = resultLayer.AddComponent<RectTransform>();
            Image resultImage = resultLayer.AddComponent<Image>();
            
            resultRect.anchorMin = Vector2.zero;
            resultRect.anchorMax = Vector2.one;
            resultRect.sizeDelta = Vector2.zero;
            resultRect.anchoredPosition = Vector2.zero;
            
            resultImage.color = new Color(1f, 0.8f, 0.2f); // Gold placeholder
            
            // Add placeholder text
            GameObject resultText = new GameObject("ResultText");
            resultText.transform.SetParent(resultLayer.transform, false);
            RectTransform textRect = resultText.AddComponent<RectTransform>();
            Text text = resultText.AddComponent<Text>();
            
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            text.text = "YOU WIN!\n🎉";
            text.fontSize = 48;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            
            // Create Scratch Layer
            GameObject scratchLayer = new GameObject("ScratchLayer");
            scratchLayer.transform.SetParent(container.transform, false);
            RectTransform scratchRect = scratchLayer.AddComponent<RectTransform>();
            Image scratchImage = scratchLayer.AddComponent<Image>();
            
            scratchRect.anchorMin = Vector2.zero;
            scratchRect.anchorMax = Vector2.one;
            scratchRect.sizeDelta = Vector2.zero;
            scratchRect.anchoredPosition = Vector2.zero;
            
            scratchImage.color = new Color(0.7f, 0.7f, 0.7f); // Silver placeholder
            
            // Add Controller
            ScratchCardController controller = container.AddComponent<ScratchCardController>();
            
            // Try to find settings asset
            string[] guids = AssetDatabase.FindAssets("t:ScratchCardSettings");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                ScratchCardSettings settings = AssetDatabase.LoadAssetAtPath<ScratchCardSettings>(path);
                
                SerializedObject serializedController = new SerializedObject(controller);
                serializedController.FindProperty("_settings").objectReferenceValue = settings;
                serializedController.FindProperty("_scratchLayer").objectReferenceValue = scratchImage;
                serializedController.FindProperty("_resultLayer").objectReferenceValue = resultImage;
                serializedController.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning("[ScratchCardSetup] No ScratchCardSettings found. Please create one via: Create > ScratchCard > Settings");
            }
            
            // Register undo
            Undo.RegisterCreatedObjectUndo(container, "Create Scratch Card");
            
            // Select the created object
            Selection.activeGameObject = container;
            
            Debug.Log("[ScratchCardSetup] Scratch Card created successfully! Don't forget to assign Settings if not auto-assigned.");
        }
        
        [MenuItem("Assets/Create/ScratchCard/Settings")]
        private static void CreateSettings()
        {
            ScratchCardSettings settings = ScriptableObject.CreateInstance<ScratchCardSettings>();
            
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (!string.IsNullOrEmpty(System.IO.Path.GetExtension(path)))
            {
                path = path.Replace(System.IO.Path.GetFileName(path), "");
            }
            
            string assetPath = AssetDatabase.GenerateUniqueAssetPath(path + "/ScratchCardSettings.asset");
            AssetDatabase.CreateAsset(settings, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = settings;
            
            Debug.Log($"[ScratchCardSetup] Settings created at: {assetPath}");
        }
    }
}
#endif
