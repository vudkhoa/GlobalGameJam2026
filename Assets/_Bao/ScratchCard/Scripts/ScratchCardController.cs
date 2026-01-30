using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScratchCard
{
    /// <summary>
    /// Main controller for scratch card mechanics
    /// Handles input, rendering, and win condition detection
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class ScratchCardController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ScratchCardSettings _settings;
        [SerializeField] private Image _scratchLayer;
        [SerializeField] private Image _resultLayer;

        [Header("Events")]
        public UnityEvent<float> OnScratchProgressChanged;
        public UnityEvent OnScratchCompleted;

        private ScratchCardData _data;
        private Material _scratchMaterial;
        private RectTransform _rectTransform;
        private Camera _mainCamera;

        private Vector2 _lastScratchPosition;
        private bool _isScratching;
        private int _frameCounter;
        private const int CALCULATE_PERCENT_EVERY_N_FRAMES = 10;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _mainCamera = Camera.main;

            ValidateReferences();
            InitializeScratchCard();
        }

        private void ValidateReferences()
        {
            if (_settings == null)
            {
                Debug.LogError("[ScratchCardController] Settings is null! Please assign ScratchCardSettings.");
            }

            if (_scratchLayer == null)
            {
                Debug.LogError("[ScratchCardController] Scratch Layer is null! Please assign the scratch layer Image.");
            }

            if (_resultLayer == null)
            {
                Debug.LogError("[ScratchCardController] Result Layer is null! Please assign the result layer Image.");
            }
        }

        private void InitializeScratchCard()
        {
            // Create data
            _data = new ScratchCardData(_settings.maskResolution);

            // Create material instance from shader
            Shader shader = Shader.Find("Custom/ScratchCard");
            if (shader == null)
            {
                Debug.LogError("[ScratchCardController] Shader 'Custom/ScratchCard' not found!");
                return;
            }

            _scratchMaterial = new Material(shader);
            _scratchMaterial.SetTexture("_MainTex", _settings.scratchLayerTexture);
            _scratchMaterial.SetTexture("_MaskTex", _data.MaskTexture);
            _scratchMaterial.SetColor("_Color", _settings.scratchLayerColor);

            // Apply material to scratch layer
            _scratchLayer.material = _scratchMaterial;

            Debug.Log("[ScratchCardController] Initialized successfully");
        }

        private void Update()
        {
            if (_data == null || _data.IsCompleted) return;

            HandleInput();
        }

        private void HandleInput()
        {
            // Mouse/Touch input
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 localPoint;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rectTransform,
                    Input.mousePosition,
                    _mainCamera,
                    out localPoint))
                {
                    _isScratching = true;
                    _lastScratchPosition = localPoint;
                    ScratchAtPosition(localPoint);
                }
            }
            else if (Input.GetMouseButton(0) && _isScratching)
            {
                Vector2 localPoint;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rectTransform,
                    Input.mousePosition,
                    _mainCamera,
                    out localPoint))
                {
                    // Check minimum distance to avoid overdraw
                    if (Vector2.Distance(localPoint, _lastScratchPosition) >= _settings.minScratchDistance)
                    {
                        ScratchAtPosition(localPoint);
                        _lastScratchPosition = localPoint;
                    }
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _isScratching = false;

                // Calculate percentage when user stops scratching
                _data.CalculateScratchedPercent();
                OnScratchProgressChanged?.Invoke(_data.ScratchedPercent);

                CheckWinCondition();
            }
        }

        private void ScratchAtPosition(Vector2 localPosition)
        {
            // Convert local position to UV coordinates (0-1 range)
            Vector2 uv = LocalPositionToUV(localPosition);

            // Draw brush on mask texture
            DrawBrush(uv);

            // Periodically calculate percentage for feedback
            _frameCounter++;
            if (_frameCounter >= CALCULATE_PERCENT_EVERY_N_FRAMES)
            {
                _frameCounter = 0;
                _data.CalculateScratchedPercent();
                OnScratchProgressChanged?.Invoke(_data.ScratchedPercent);
            }
        }

        private Vector2 LocalPositionToUV(Vector2 localPosition)
        {
            Rect rect = _rectTransform.rect;

            // Normalize to 0-1 range
            float u = (localPosition.x - rect.xMin) / rect.width;
            float v = (localPosition.y - rect.yMin) / rect.height;

            return new Vector2(u, v);
        }

        private void DrawBrush(Vector2 uv)
        {
            // Save current RenderTexture
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = _data.MaskTexture;

            // Calculate brush size in UV space
            float brushSizeUV = _settings.brushSize / _settings.maskResolution;

            // Draw using GL
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, 1, 1, 0);

            // Create brush material
            Material brushMat = new Material(Shader.Find("Hidden/Internal-Colored"));
            brushMat.SetPass(0);

            GL.Begin(GL.QUADS);
            GL.Color(new Color(1, 1, 1, _settings.brushOpacity));

            // Draw quad at brush position
            float halfSize = brushSizeUV * 0.5f;
            GL.Vertex3(uv.x - halfSize, uv.y - halfSize, 0);
            GL.Vertex3(uv.x + halfSize, uv.y - halfSize, 0);
            GL.Vertex3(uv.x + halfSize, uv.y + halfSize, 0);
            GL.Vertex3(uv.x - halfSize, uv.y + halfSize, 0);

            GL.End();
            GL.PopMatrix();

            // Restore RenderTexture
            RenderTexture.active = previous;

            Destroy(brushMat);
        }

        private void CheckWinCondition()
        {
            if (_data.ScratchedPercent >= _settings.winThresholdPercent && !_data.IsCompleted)
            {
                _data.MarkAsCompleted();
                OnScratchCompleted?.Invoke();
                Debug.Log($"[ScratchCardController] Scratch completed! {_data.ScratchedPercent:F1}% scratched");
            }
        }

        /// <summary>
        /// Reset the scratch card to initial state
        /// </summary>
        public void ResetScratchCard()
        {
            _data.Reset();
            _frameCounter = 0;
            OnScratchProgressChanged?.Invoke(0f);
        }

        /// <summary>
        /// Get current scratch progress (0-100)
        /// </summary>
        public float GetScratchProgress()
        {
            return _data.ScratchedPercent;
        }

        private void OnDestroy()
        {
            _data?.Dispose();

            if (_scratchMaterial != null)
            {
                Destroy(_scratchMaterial);
            }
        }
    }
}
