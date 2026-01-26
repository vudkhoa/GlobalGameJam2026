using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "Collect_Task", menuName = "Tasks/Action/Collect")]
public class CollectTask : BaseTask
{
    [SerializeField] private BaseCollection _collection;
    [SerializeField] private float _interactionRange = 1f;
    [SerializeField] private float _duration = 1f;

    public override async UniTask<bool> Execute(Transform transform, Vector3 position)
    {
        if (_collection == null) return false;

        // Check distance to target (position is assumed to be the item position)
        if (Vector3.Distance(transform.position, position) > _interactionRange)
        {
            Debug.LogWarning($"{transform.name} is too far to collect item at {position}");
            return false;
        }

        Collider[] hits = Physics.OverlapSphere(position, 0.1f);
        Transform itemTarget = hits.Length > 0 ? hits[0].transform : null;

        _collection.Collect(transform, itemTarget);

        await UniTask.Delay(TimeSpan.FromSeconds(_duration));
        return true;
    }
}
