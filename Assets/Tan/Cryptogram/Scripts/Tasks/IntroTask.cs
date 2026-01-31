using Cysharp.Threading.Tasks;
using UnityEngine;

public class IntroTask : BaseTask
{
    [Header("References")]
    [SerializeField] private ChapterFlowManager _flowManager;

    public override  UniTask Execute()
    {
        if (_flowManager == null)
        {
            doneTask = true;
            return UniTask.CompletedTask;
        }

        _flowManager.OnIntroCompleted += HandleIntroFinished;
        _flowManager.PlayIntroOnlyAsync().Forget();
        return UniTask.CompletedTask;
    }

    private void HandleIntroFinished()
    {
        _flowManager.OnIntroCompleted -= HandleIntroFinished;
        this.doneTask = true;
    }
    private void OnDisable()
    {
        if (_flowManager != null)
        {
            _flowManager.OnIntroCompleted -= HandleIntroFinished;
        }
    }
}