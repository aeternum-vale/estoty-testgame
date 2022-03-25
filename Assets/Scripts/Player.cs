using System;
using Gamelogic.Extensions;
using Lean.Touch;
using UnityEngine;

public class Player : MonoBehaviour
{
	[SerializeField] private float _minDotToSlideTheCar;
	[SerializeField] private Camera _mainCamera;

	private Transform _mainCameraTransform;


	private void Awake()
	{
		LeanTouch.OnFingerUp += OnFingerUp;
	}

	private void OnDestroy()
	{
		LeanTouch.OnFingerUp -= OnFingerUp;
	}

	private void Start()
	{
		_mainCamera = Camera.main;
		_mainCameraTransform = _mainCamera.transform;
	}

	private void OnFingerUp(LeanFinger finger)
	{
		if (!finger.Swipe) return;

		TryMoveTheCar(finger.StartScreenPosition, finger.LastScreenPosition);
	}

	private void TryMoveTheCar(Vector3 startScreenPosition, Vector3 lastScreenPosition)
	{
		int layerMask = 1 << GameManager.CarLayer;

		Vector3 origin = _mainCameraTransform.position;
		Vector3 startScreenPositionWithZ = startScreenPosition.WithZ(5f);
		Vector3 lastScreenPositionWithZ = lastScreenPosition.WithZ(5f);
		Vector3 direction =
			(_mainCamera.ScreenToWorldPoint(startScreenPositionWithZ) - _mainCameraTransform.position)
			.normalized;

		if (Physics.Raycast(origin, direction, out RaycastHit hit, 100f, layerMask))
		{
			Transform carTransform = hit.transform;

			Vector2 swipeScreenDirection = (lastScreenPosition - startScreenPosition).normalized;
			Vector2 carScreenDirection =
				(_mainCamera.WorldToScreenPoint(carTransform.position + carTransform.forward * 5f) -
				_mainCamera.WorldToScreenPoint(carTransform.position)).normalized;

			float swipeAndCarScreenDirectionDot = Vector2.Dot(swipeScreenDirection, carScreenDirection);

			if (Math.Abs(swipeAndCarScreenDirectionDot) >= _minDotToSlideTheCar)
			{
				var car = carTransform.GetComponent<Car>();
				if (swipeAndCarScreenDirectionDot > 0)
					car.MoveForward();
				else
					car.MoveBackward();
			}
		}
	}

}
