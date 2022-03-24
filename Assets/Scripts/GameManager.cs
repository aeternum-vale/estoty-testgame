using System;
using System.Linq;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public const int MainMenuSceneIndex = 0;
	public const int GameSceneIndex = 1;

	public const int CarLayer = 6;
	public const int RoadLayer = 7;
	public const int ExitLayer = 8;


	[SerializeField] private Transform _carsContainer;

	[Header("UI")]

	[SerializeField] private CanvasGroup _gameUIScreen;
	[SerializeField] private CanvasGroup _levelCompletedScreen;

	[SerializeField] private float _screenFadeDuration = 2f;

	[SerializeField] private Button _homeButton;
	[SerializeField] private Button _restartButton;
	[SerializeField] private Button _levelCompletedScreenHomeButton;
	[SerializeField] private Button _levelCompletedScreenRestartButton;

	private Car[] _cars;
	private int _reachedExitCarCount = 0;

	private void Awake()
	{
		_cars = _carsContainer.GetComponentsInChildren<Car>();
		_cars.ToList().ForEach(c => c.ReachedExit += OnCarReachedExit);

		_homeButton.onClick.AddListener(OnHomeButtonClicked);
		_restartButton.onClick.AddListener(OnRestartButtonClicked);
		_levelCompletedScreenHomeButton.onClick.AddListener(OnHomeButtonClicked);
		_levelCompletedScreenRestartButton.onClick.AddListener(OnRestartButtonClicked);
	}

	private void OnCarReachedExit(object sender, EventArgs args)
	{
		_reachedExitCarCount++;

		if (_reachedExitCarCount == _cars.Length)
			FinishLevel();
	}

	[Button]
	private void FinishLevel()
	{
		Debug.Log($"<color=lightblue>{GetType().Name}:</color> Level finished");

		_gameUIScreen.DOFade(0f, _screenFadeDuration)
			.OnComplete(() => _gameUIScreen.gameObject.SetActive(false));

		_levelCompletedScreen.gameObject.SetActive(true);
		_levelCompletedScreen.DOFade(1f, _screenFadeDuration);
		_levelCompletedScreen.transform.DOScale(Vector3.one, _screenFadeDuration).SetEase(Ease.OutBack);

	}

	private void OnHomeButtonClicked()
	{
		SceneManager.LoadScene(GameManager.MainMenuSceneIndex);
	}

	private void OnRestartButtonClicked()
	{
		SceneManager.LoadScene(GameManager.GameSceneIndex, LoadSceneMode.Single);

	}
}