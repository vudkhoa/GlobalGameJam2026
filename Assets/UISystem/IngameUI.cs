using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IngameUI : UICanvas
{
    [SerializeField] private Button replayButton;

    private void OnEnable()
    {
        replayButton.onClick.AddListener(OnClickReplayButton);
    }

    private void OnDisable()
    {
        replayButton.onClick.AddListener(OnClickReplayButton);
    }

    private void OnClickReplayButton()
    {
        SoundManager.Instance.PlaySFX(SoundType.SFX_Click_Picture);
        SceneManager.LoadScene("Init", LoadSceneMode.Single);
        /*UIManager.Instance.gameObject.SetActive(false);*/
    }
}
