using Cysharp.Threading.Tasks;
using UnityEngine;

public class Chapter3Task : BaseTask
{
    [Header("References")]
    [SerializeField] private ChapterFlowManager _flowManager;

    public override async UniTask<bool> Execute()
    {
        Debug.Log("--- TASK: STARTING CHAPTER 3 ---");

        if (_flowManager == null)
        {
            Debug.LogError("Chưa gán ChapterFlowManager cho Task!");
            return false;
        }

        await _flowManager.RunChapterSequence();

        Debug.Log("--- TASK: CHAPTER 3 COMPLETED ---");
        return true; 
    }
}