using Cysharp.Threading.Tasks;
using UnityEngine;

public class IntroTask : BaseTask
{
    [Header("References")]
    [SerializeField] private ChapterFlowManager _flowManager;

    public override async UniTask Execute()
    {
        if (_flowManager == null)
        {
            return;
        }
        await _flowManager.PlayIntroOnlyAsync();
        this.doneTask = true;
    }
}