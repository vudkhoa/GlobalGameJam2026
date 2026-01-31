using Cysharp.Threading.Tasks;
using UnityEngine;

public class ActiveObjectTask : BaseTask
{
    [SerializeField] private GameObject _target;

    public override UniTask Execute()
    {
        _target.SetActive(true);
        return UniTask.WaitUntil(() => doneTask == true);
    }
}