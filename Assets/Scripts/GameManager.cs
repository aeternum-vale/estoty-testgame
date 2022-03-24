using System;
using Lean.Touch;
using UnityEngine;

public class GameManager : MonoBehaviour
{

	[SerializeField] private float _minDotToSlideTheCar;

	private Camera _mainCamera;
	private Transform _mainCameraTransform;


	private Vector3 _debugSwipeStart;
	private Vector3 _debugSwipeEnd;
	private Vector3 _debugCarDirStart;
	private Vector3 _debugCarDirEnd;

	private void Awake()
	{
		LeanTouch.OnFingerUp += OnFingerUp;
	}

	private void Start()
	{
		_mainCamera = Camera.main;
		_mainCameraTransform = _mainCamera.transform;
	}

	private void OnFingerUp(LeanFinger finger)
	{
		if (!finger.Swipe) return;

		int carLayer = 6;
		int layerMask = 1 << carLayer;

		Vector3 origin = _mainCameraTransform.position;
		Vector3 startScreenPositionWithZ = new Vector3(finger.StartScreenPosition.x, finger.StartScreenPosition.y, 5f);
		Vector3 lastScreenPositionWithZ = new Vector3(finger.LastScreenPosition.x, finger.LastScreenPosition.y, 5f);
		Vector3 direction = (_mainCamera.ScreenToWorldPoint(startScreenPositionWithZ) - _mainCameraTransform.position).normalized;

		_debugSwipeStart = startScreenPositionWithZ;
		_debugSwipeEnd = lastScreenPositionWithZ;



		if (Physics.Raycast(origin, direction, out RaycastHit hit, 100f, layerMask))
		{
			Debug.Log($"<color=lightblue>{GetType().Name}:</color> hit!");

			_debugCarDirStart = _mainCamera.WorldToScreenPoint(hit.transform.position);
			_debugCarDirEnd = _mainCamera.WorldToScreenPoint(hit.transform.position + hit.transform.forward * 5f);

			Transform car = hit.transform;

			Vector2 swipeScreenDirection = (finger.LastScreenPosition - finger.StartScreenPosition).normalized;
			Vector2 carScreenDirection =
				(_mainCamera.WorldToScreenPoint(car.position + car.forward * 5f) -
				_mainCamera.WorldToScreenPoint(car.position)).normalized;

			float swipeAndCarScreenDirectionDot = Vector2.Dot(swipeScreenDirection, carScreenDirection);

			if (Math.Abs(swipeAndCarScreenDirectionDot) >= _minDotToSlideTheCar)
				Debug.Log($"<color=lightblue>{GetType().Name}:</color> slide! ({swipeAndCarScreenDirectionDot})");
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
