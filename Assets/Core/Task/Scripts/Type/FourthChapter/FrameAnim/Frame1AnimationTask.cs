using BrunoMikoski.AnimationSequencer;
using Cysharp.Threading.Tasks;

public class Frame1AnimationTask : BaseTask
{
    public AnimationSequencerController anim;
    public float dur;

    public override UniTask Execute()
    {
        if (anim != null)
        {
            anim.Play();
        }
        AwaitDoneTask(dur);
        return UniTask.WaitUntil(() => doneTask == true);
    }

    private async void AwaitDoneTask(float time)
    {
        await UniTask.WaitForSeconds(time);
        this.doneTask = true;
    }
}