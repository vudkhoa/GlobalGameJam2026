using UnityEngine;

[CreateAssetMenu(fileName = "PickItem_Collection", menuName = "Collection/PickItem")]
public class PickItem : BaseCollection
{
    public override void Collect(Transform collector, Transform itemTarget)
    {
        if (itemTarget == null || collector == null) return;

        Debug.Log($"{collector.name} picked up {itemTarget.name}");
    }
}
