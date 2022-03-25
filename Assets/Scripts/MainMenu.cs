using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
	[SerializeField] private Button _playButton;

	private void Awake()
	{
		_playButton.onClick.AddListener(OnPlayButtonClick);
	}

	private void OnDestroy()
	{
		_playButton.onClick.RemoveListener(OnPlayButtonClick);
	}

	private void OnPlayButtonClick()
	{
		SceneManager.LoadScene(GameManager.GameSceneIndex);
	}
}
