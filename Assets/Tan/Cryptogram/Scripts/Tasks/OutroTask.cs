using Cysharp.Threading.Tasks;
using UnityEngine;

public class OutroTask : BaseTask
{
    [SerializeField] private ChapterFlowManager _flowManager;

    public override async UniTask Execute()
    {
        if (_flowManager == null)
        {
            doneTask = true;
            return;
        }

        PlayerDecision decision = EndingChoiceTask.FinalDecision;

        if (decision == PlayerDecision.None) decision = PlayerDecision.Denial;

        await _flowManager.TriggerOutro(decision);

        doneTask = true;
    }
}