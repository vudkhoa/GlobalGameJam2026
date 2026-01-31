using System;
using UnityEngine;

public class ChapterScreenManager : MonoBehaviour
{
    private Action _doneAction;

    public void SetAction(Action doneAction)
    {
        this._doneAction = doneAction;
    }

    public void PlayAction()
    {
        _doneAction?.Invoke();
    }
    
}
