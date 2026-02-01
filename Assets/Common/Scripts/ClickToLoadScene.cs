using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ClickToLoadScene : MonoBehaviour
{
    [SerializeField] private string _sceneName;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(LoadScene);
    }

    private void LoadScene()
    {
        SoundManager.Instance?.PlaySFX(SoundType.SFX_Click_Picture);

        if (!string.IsNullOrEmpty(_sceneName))
        {
            SceneManager.LoadScene(_sceneName);
        }
        else
        {
            // Scene name is empty
        }
    }

    private void OnDestroy()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(LoadScene);
        }
    }
}
