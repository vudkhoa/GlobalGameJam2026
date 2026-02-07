using Cysharp.Threading.Tasks;
using UnityEngine;

public class EndingChoiceTask : BaseTask
{
    [SerializeField] private PuzzleController _controller;
    [SerializeField] private ChapterFlowManager _flowManager; 

    public static PlayerDecision FinalDecision = PlayerDecision.None;

    public override async UniTask Execute()
    {
        if (_controller == null)
        {
            doneTask = true;
            return;
        }

        int choiceIndex = await _controller.ShowEndingChoiceAndWaitAsync();

        FinalDecision = (choiceIndex == 0) ? PlayerDecision.Denial : PlayerDecision.Acceptance;
        Debug.Log($"Player chose: {FinalDecision}");

        doneTask = true;
    }
}