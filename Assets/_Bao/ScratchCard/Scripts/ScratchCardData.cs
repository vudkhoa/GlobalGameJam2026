using UnityEngine;

namespace ScratchCard
{
    /// <summary>
    /// Runtime data for scratch card state
    /// </summary>
    public class ScratchCardData
    {
        public RenderTexture MaskTexture { get; private set; }
        public float ScratchedPercent { get; private set; }
        public bool IsCompleted { get; private set; }

        private readonly int _resolution;
        private readonly Texture2D _readTexture;
        private int _totalPixels;
        private int _scratchedPixels;

        public ScratchCardData(int resolution)
        {
            _resolution = resolution;
            _totalPixels = resolution * resolution;

            // Create RenderTexture for mask
            MaskTexture = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.R8)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            // Initialize with black (no scratch)
            RenderTexture.active = MaskTexture;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = null;

            // Create texture for reading back pixels (for percentage calculation)
            _readTexture = new Texture2D(resolution, resolution, TextureFormat.R8, false);
        }

        /// <summary>
        /// Calculate scratched percentage by reading back the mask texture
        /// This is expensive, call sparingly (e.g., every N frames or on pointer up)
        /// </summary>
        public void CalculateScratchedPercent()
        {
            RenderTexture.active = MaskTexture;
            _readTexture.ReadPixels(new Rect(0, 0, _resolution, _resolution), 0, 0);
            _readTexture.Apply();
            RenderTexture.active = null;

            Color[] pixels = _readTexture.GetPixels();
            _scratchedPixels = 0;

            foreach (Color pixel in pixels)
            {
                // Count pixels where red channel > 0.5 as scratched
                if (pixel.r > 0.5f)
                {
                    _scratchedPixels++;
                }
            }

            ScratchedPercent = (_scratchedPixels / (float)_totalPixels) * 100f;
        }

        public void MarkAsCompleted()
        {
            IsCompleted = true;
        }

        public void Reset()
        {
            RenderTexture.active = MaskTexture;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = null;

            _scratchedPixels = 0;
            ScratchedPercent = 0f;
            IsCompleted = false;
        }

        public void Dispose()
        {
            if (MaskTexture != null)
            {
                MaskTexture.Release();
                Object.Destroy(MaskTexture);
            }

            if (_readTexture != null)
            {
                Object.Destroy(_readTexture);
            }
        }
    }
}
