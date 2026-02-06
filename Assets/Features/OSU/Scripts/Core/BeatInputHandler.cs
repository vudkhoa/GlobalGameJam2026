using System.Collections.Generic;
using UnityEngine;

public class BeatInputHandler
{
    private readonly Camera _gameCamera;
    private readonly LayerMask _beatLayerMask;
    private readonly bool _debugMode;

    public BeatInputHandler(Camera gameCamera, LayerMask beatLayerMask, bool debugMode = false)
    {
        _gameCamera = gameCamera;
        _beatLayerMask = beatLayerMask;
        _debugMode = debugMode;
    }

    // Thay đổi: Trả về List thay vì 1 cái
    public List<BeatCircle> ProcessInput()
    {
        List<BeatCircle> hitBeats = new List<BeatCircle>();
        if (_gameCamera == null) return hitBeats;

        // 1. Xử lý chuột (cho Editor/PC)
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = _gameCamera.ScreenToWorldPoint(Input.mousePosition);
            BeatCircle beat = CastRay(mousePos);
            if (beat != null && !hitBeats.Contains(beat)) hitBeats.Add(beat);
        }

        // 2. Xử lý cảm ứng (cho Mobile) - Lấy TẤT CẢ ngón tay
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    Vector2 touchPos = _gameCamera.ScreenToWorldPoint(touch.position);
                    BeatCircle beat = CastRay(touchPos);

                    // Logic quan trọng: Cho phép Multitouch (nhiều nốt cùng lúc)
                    if (beat != null && !hitBeats.Contains(beat))
                    {
                        hitBeats.Add(beat);
                    }
                }
            }
        }

        return hitBeats;
    }

    private BeatCircle CastRay(Vector2 worldPos)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(worldPos, Vector2.zero, Mathf.Infinity, _beatLayerMask);

        foreach (var hit in hits)
        {
            if (hit.collider != null)
            {
                BeatCircle beat = hit.collider.GetComponent<BeatCircle>();
                // Chỉ lấy nốt đang Active
                if (beat != null && beat.IsActive)
                {
                    return beat; 
                }
            }
        }

        return null;
    }
}