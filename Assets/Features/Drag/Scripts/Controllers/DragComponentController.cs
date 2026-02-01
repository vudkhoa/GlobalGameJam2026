using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class DragComponentController : MonoSingleton<DragComponentController>
{
    [Header(" Setting ")]
    [SerializeField] private DragComponentView[] dragComponenets;
    [SerializeField] private RectOffset rectOffset;
    [SerializeField] private Vector2Int matrixSize;
    [SerializeField] private Vector2 dragComponentSize;
    [SerializeField] private Vector2 offset;

    [SerializeField] private RectTransform rectTransform;
    public RectTransform RectTransformRef => rectTransform;

    [SerializeField] private float awaitDuration;
    public float AwaitDuration => awaitDuration;

    [SerializeField] private float moveSpeed;
    public float MoveSpeed => moveSpeed;

    [SerializeField] private float winMoveDuration = 1f; // Thời gian di chuyển khi Win (tất cả cùng lúc)
    public float WinMoveDuration => winMoveDuration;

    [Header(" Shake Settings ")]
    [SerializeField] private float shakeStrength = 5f; // Độ mạnh của shake
    [SerializeField] private float shakeDuration = 0.1f; // Thời gian mỗi lần shake

    public float ShakeStrength => shakeStrength;
    public float ShakeDuration => shakeDuration;

    [Header(" Boundary Points (4 corners) ")]
    [SerializeField] private RectTransform topLeftCorner;
    [SerializeField] private RectTransform topRightCorner;
    [SerializeField] private RectTransform bottomLeftCorner;
    [SerializeField] private RectTransform bottomRightCorner;

    public RectTransform TopLeftCorner => topLeftCorner;
    public RectTransform TopRightCorner => topRightCorner;
    public RectTransform BottomLeftCorner => bottomLeftCorner;
    public RectTransform BottomRightCorner => bottomRightCorner;

    public List<Vector2> positions;
    public float avgWidth;
    public float avgHeight;
    private bool _hasWon = false;
    public bool HasWon => _hasWon;

    private async void Awake()
    {
        _hasWon = false;
        rectTransform = gameObject.GetComponent<RectTransform>();
        InitPictures();
    }

    private void InitPictures()
    {
        avgWidth = GetAverageImageWidth();
        avgHeight = GetAverageImageHeight();
        CalculateCorrectPositions();
    }

    /// <summary>
    /// Calculates correct positions based on matrixSize (rows x columns)
    /// </summary>
    private void CalculateCorrectPositions()
    {
        if (matrixSize.x <= 0 || matrixSize.y <= 0)
        {
            return;
        }

        positions = new List<Vector2>(matrixSize.x * matrixSize.y);

        // Reference resolution (from Canvas Scaler)
        const float referenceWidth = 1080f;
        const float referenceHeight = 2400f;

        Vector2 area = new Vector2(
            referenceWidth - rectOffset.left - rectOffset.right,
            referenceHeight - rectOffset.top - rectOffset.bottom
        );

        Vector2 startPos = new Vector2(
            (-referenceWidth / 2f) + rectOffset.left + ((area.x / matrixSize.y) / 2f),
            (referenceHeight / 2f) - rectOffset.top - ((area.y / matrixSize.x) / 2f)
        );

        // Calculate position for each cell in the matrix (row x column)
        for (int row = 0; row < matrixSize.x; row++)
        {
            for (int col = 0; col < matrixSize.y; col++)
            {
                Vector2 position = new Vector2(
                    startPos.x + (avgWidth * col),
                    startPos.y - (avgHeight * row)
                );
                positions.Add(position);
            }
        }
    }

    public Vector2Int ConvertToIndexMatrix(int index)
    {
        int x = index / matrixSize.x;
        int y = index % matrixSize.y;
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// Calculates the average width of all images in dragComponenets array
    /// </summary>
    public float GetAverageImageWidth()
    {
        if (dragComponenets == null || dragComponenets.Length == 0)
        {
            return 0f;
        }

        float totalWidth = 0f;
        int validCount = 0;

        for (int i = 0; i < dragComponenets.Length; i++)
        {
            if (dragComponenets[i] == null)
            {
                continue;
            }

            RectTransform rectTransform = dragComponenets[i].GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                totalWidth += rectTransform.rect.width;
                validCount++;
            }
        }

        if (validCount == 0)
        {
            return 0f;
        }

        return totalWidth / validCount;
    }

    /// <summary>
    /// Calculates the average height of all images in dragComponenets array
    /// </summary>
    public float GetAverageImageHeight()
    {
        if (dragComponenets == null || dragComponenets.Length == 0)
        {
            return 0f;
        }

        float totalHeight = 0f;
        int validCount = 0;

        for (int i = 0; i < dragComponenets.Length; i++)
        {
            if (dragComponenets[i] == null)
            {
                continue;
            }

            RectTransform rectTransform = dragComponenets[i].GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                totalHeight += rectTransform.rect.height;
                validCount++;
            }
        }

        if (validCount == 0)
        {
            return 0f;
        }

        return totalHeight / validCount;
    }

    /// <summary>
    /// Calculates distance between two components based on average image width
    /// </summary>
    public float GetDistance(int index1, int index2)
    {
        if (dragComponenets == null || dragComponenets.Length == 0)
        {

            return float.MaxValue;
        }

        if (index1 < 0 || index1 >= dragComponenets.Length || index2 < 0 || index2 >= dragComponenets.Length)
        {
            return float.MaxValue;
        }

        if (dragComponenets[index1] == null || dragComponenets[index2] == null)
        {
            return float.MaxValue;
        }

        RectTransform rectTransform1 = dragComponenets[index1].GetComponent<RectTransform>();
        RectTransform rectTransform2 = dragComponenets[index2].GetComponent<RectTransform>();

        if (rectTransform1 == null || rectTransform2 == null)
        {
            return float.MaxValue;
        }

        Vector2 pos1 = rectTransform1.anchoredPosition;
        Vector2 pos2 = rectTransform2.anchoredPosition;
        float distance = Vector2.Distance(pos1, pos2);

        // Normalize distance by average width
        float averageWidth = GetAverageImageWidth();
        if (averageWidth > 0f)
        {
            return distance / averageWidth;
        }

        return distance;
    }

    /// <summary>
    /// Di chuyển tất cả component đến vị trí đúng (sát nhau theo avgWidth và avgHeight)
    /// Giữ nguyên component ở giữa matrix, chỉ di chuyển các component khác về sát nó
    /// </summary>
    private void MoveAllComponentsToCorrectPositions()
    {
        if (dragComponenets == null || positions == null || dragComponenets.Length == 0)
        {
            return;
        }

        if (dragComponenets.Length != positions.Count)
        {
            return;
        }

        // Tìm vị trí giữa matrix
        int centerRow = matrixSize.x / 2;
        int centerCol = matrixSize.y / 2;
        int centerIndex = centerRow * matrixSize.y + centerCol;

        if (centerIndex >= dragComponenets.Length || dragComponenets[centerIndex] == null)
        {
            return;
        }

        RectTransform centerRect = dragComponenets[centerIndex].GetComponent<RectTransform>();
        if (centerRect == null)
        {
            return;
        }

        // Lấy vị trí hiện tại của component ở giữa (giữ nguyên)
        Vector2 centerCurrentPos = centerRect.anchoredPosition;

        // Hủy tất cả DOTween đang chạy trước khi di chuyển về vị trí đúng
        for (int i = 0; i < dragComponenets.Length; i++)
        {
            if (dragComponenets[i] != null)
            {
                dragComponenets[i].KillAllTweens();
            }
        }

        // Di chuyển từng component đến vị trí đúng trong matrix (trừ component ở giữa)
        for (int row = 0; row < matrixSize.x; row++)
        {
            for (int col = 0; col < matrixSize.y; col++)
            {
                int index = row * matrixSize.y + col;
                
                // Bỏ qua component ở giữa (giữ nguyên vị trí)
                if (index == centerIndex)
                {
                    continue;
                }
                
                if (index >= dragComponenets.Length || dragComponenets[index] == null)
                {
                    continue;
                }

                RectTransform rectTransform = dragComponenets[index].GetComponent<RectTransform>();
                if (rectTransform == null)
                {
                    continue;
                }

                // Tính vị trí đúng dựa trên vị trí hiện tại của component ở giữa
                int rowOffset = row - centerRow;
                int colOffset = col - centerCol;
                
                Vector2 targetPos = new Vector2(
                    centerCurrentPos.x + (avgWidth * colOffset),
                    centerCurrentPos.y - (avgHeight * rowOffset)
                );

                // Di chuyển đến vị trí đúng với cùng duration (tất cả cùng lúc)
                rectTransform.DOAnchorPos(targetPos, winMoveDuration)
                    .SetEase(Ease.OutQuad).SetLink(rectTransform.gameObject);
            }
        }
        AwaitingWinAnim();
    }

    /// <summary>
    /// Checks if all components are in correct positions based on matrixSize
    /// Checks neighbors: distance should not exceed avg + offset
    /// </summary>
    private void Update()
    {
        if (_hasWon) return;

        if (dragComponenets == null || positions == null || dragComponenets.Length == 0)
        {
            return;
        }

        if (avgWidth <= 0f || avgHeight <= 0f)
        {
            return;
        }

        if (dragComponenets.Length != positions.Count)
        {
            return;
        }

        if (matrixSize.x <= 0 || matrixSize.y <= 0)
        {
            return;
        }

        bool allCorrect = true;

        // Check each component's position and its neighbors
        for (int row = 0; row < matrixSize.x; row++)
        {
            for (int col = 0; col < matrixSize.y; col++)
            {
                int currentIndex = row * matrixSize.y + col;
                
                if (currentIndex >= dragComponenets.Length || dragComponenets[currentIndex] == null)
                {
                    allCorrect = false;
                    continue;
                }

                RectTransform currentRect = dragComponenets[currentIndex].GetComponent<RectTransform>();
                if (currentRect == null)
                {
                    allCorrect = false;
                    continue;
                }

                Vector2 currentPos = currentRect.anchoredPosition;

                // Check right neighbor (same row, next column)
                if (col < matrixSize.y - 1)
                {
                    int rightIndex = row * matrixSize.y + (col + 1);
                    if (rightIndex < dragComponenets.Length && dragComponenets[rightIndex] != null)
                    {
                        RectTransform rightRect = dragComponenets[rightIndex].GetComponent<RectTransform>();
                        if (rightRect != null)
                        {
                            float distanceX = Mathf.Abs(rightRect.anchoredPosition.x - currentPos.x);
                            float maxDistanceX = avgWidth + offset.x;
                            
                            if (distanceX > maxDistanceX)
                            {
                                allCorrect = false;
                            }
                        }
                    }
                }

                // Check bottom neighbor (next row, same column)
                if (row < matrixSize.x - 1)
                {
                    int bottomIndex = (row + 1) * matrixSize.y + col;
                    if (bottomIndex < dragComponenets.Length && dragComponenets[bottomIndex] != null)
                    {
                        RectTransform bottomRect = dragComponenets[bottomIndex].GetComponent<RectTransform>();
                        if (bottomRect != null)
                        {
                            float distanceY = Mathf.Abs(bottomRect.anchoredPosition.y - currentPos.y);
                            float maxDistanceY = avgHeight + offset.y;
                            
                            if (distanceY > maxDistanceY)
                            {
                                allCorrect = false;
                            }
                        }
                    }
                }
            }
        }

        if (allCorrect && !_hasWon)
        {
            MoveAllComponentsToCorrectPositions();
            _hasWon = true;

        }
        else if (!allCorrect)
        {
            _hasWon = false;
        }
    }

    private void AwaitingWinAnim()
    {
        gameObject.GetComponent<LogicTask>().ExecuteAsyncTask(winMoveDuration + 0.15f);
    }
}