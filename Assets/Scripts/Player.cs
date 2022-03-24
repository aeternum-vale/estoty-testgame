using System;
using Gamelogic.Extensions;
using Lean.Touch;
using UnityEngine;

public class Player : MonoBehaviour
{
	[SerializeField] private float _minDotToSlideTheCar;
	[SerializeField] private Camera _mainCamera;
	private Transform _mainCameraTransform;


	private Vector3 _debugSwipeStart;
	private Vector3 _debugSwipeEnd;
	private Vector3 _debugCarDirStart;
	private Vector3 _debugCarDirEnd;

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

		int layerMask = 1 << GameManager.CarLayer;

		Vector3 origin = _mainCameraTransform.position;
		Vector3 startScreenPositionWithZ = ((Vector3)finger.StartScreenPosition).WithZ(5f);
		Vector3 lastScreenPositionWithZ = ((Vector3)finger.LastScreenPosition).WithZ(5f);
		Vector3 direction = (_mainCamera.ScreenToWorldPoint(startScreenPositionWithZ) - _mainCameraTransform.position).normalized;

		_debugSwipeStart = startScreenPositionWithZ;
		_debugSwipeEnd = lastScreenPositionWithZ;

		if (Physics.Raycast(origin, direction, out RaycastHit hit, 100f, layerMask))
		{
			_debugCarDirStart = _mainCamera.WorldToScreenPoint(hit.transform.position);
			_debugCarDirEnd = _mainCamera.WorldToScreenPoint(hit.transform.position + hit.transform.forward * 5f);

			Transform carTransform = hit.transform;

			Vector2 swipeScreenDirection = (finger.LastScreenPosition - finger.StartScreenPosition).normalized;
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

	private void OnDrawGizmos()
	{
		if (!Application.isPlaying) return;

		Gizmos.color = Color.red;
		Gizmos.DrawLine(
			_mainCamera.ScreenToWorldPoint(_debugSwipeStart),
			_mainCamera.ScreenToWorldPoint(_debugSwipeEnd));

		Gizmos.color = Color.blue;
		Gizmos.DrawLine(
			_mainCamera.ScreenToWorldPoint(_debugCarDirStart),
			_mainCamera.ScreenToWorldPoint(_debugCarDirEnd));
	}


}
