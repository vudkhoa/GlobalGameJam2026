using UnityEngine;

public static partial class Helper
{
    public static Vector3 CalculateCenterPosition(int x, int y, int width, int height, Vector3 spacing)
    {
        float xPos = (x - (width - 1) / 2f) * spacing.x;
        float yPos = (y - (height - 1) / 2f) * spacing.y;

        return new Vector3(xPos, yPos, 0);
    }
}
