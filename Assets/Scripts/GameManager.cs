using System;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public const int CarLayer = 6;
	public const int RoadLayer = 7;
	public const int ExitLayer = 8;

	[SerializeField] private Transform _carsContainer;

	private Car[] _cars;

	private int _reachedExitCarCount = 0;

	private void Awake()
	{
		_cars = _carsContainer.GetComponentsInChildren<Car>();
		_cars.ToList().ForEach(c => c.ReachedExit += OnCarReachedExit);
	}

	private void OnCarReachedExit(object sender, EventArgs args)
	{
		_reachedExitCarCount++;

		if (_reachedExitCarCount == _cars.Length)
			FinishLevel();
	}

	private void FinishLevel()
	{
		Debug.Log($"<color=lightblue>{GetType().Name}:</color> Level finished");
	}
}