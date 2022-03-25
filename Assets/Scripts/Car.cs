using System;
using NaughtyAttributes;
using UnityEngine;

public class Car : PunchableObject
{
	enum ECarState { StandingStill, MovingForward, MovingBackward, Leaving }

	[SerializeField] private float _speed;
	[SerializeField] private float _rotateSpeed = 0.1f;

	[SerializeField] private float _carLength;
	[SerializeField] private float _carWidth;
	[SerializeField] private float _carSafeDistance = 0.1f;

	[SerializeField] private ParticleSystem _frontKickParticles;
	[SerializeField] private ParticleSystem _backKickParticles;

	[SerializeField] private AudioSource _kickAudioSource;


	private ECarState _state = ECarState.StandingStill;
	private BoxCollider _currentRoad;

	public event EventHandler ReachedExit;

	private bool IsMovingForward => _state == ECarState.MovingForward;
	private ECarState State => _state;


	private void Update()
	{
		if (_state == ECarState.StandingStill) return;

		if (_state == ECarState.Leaving)
		{
			LeavingUpdate();
			return;
		}

		MovingUpdate();
	}

	private void MovingUpdate()
	{
		int dir = IsMovingForward ? 1 : -1;

		transform.position += transform.forward * dir * _speed * Time.deltaTime;
	}

	private void LeavingUpdate()
	{
		Vector3 target = _currentRoad.transform.position +
			_currentRoad.transform.forward *
			(_currentRoad.size.z / 2) *
			_currentRoad.transform.localScale.z;


		Vector3 dirToTarget = (target - transform.position).normalized;
		dirToTarget = new Vector3(dirToTarget.x, 0, dirToTarget.z);

		transform.forward = Vector3.Slerp(transform.forward, dirToTarget, _rotateSpeed * Time.deltaTime);

		transform.position += transform.forward * _speed * Time.deltaTime;
	}

	[Button] public void MoveForward() => TryMove(true);
	[Button] public void MoveBackward() => TryMove(false);

	private void TryMove(bool isForward)
	{
		int layerMask = (1 << GameManager.CarLayer) | (1 << GameManager.ObstacleLayer);

		RaycastHit hit;
		bool wasHit = false;

		if (Physics.Raycast(transform.position, (isForward ? 1 : -1) * transform.forward,
			out hit, _carLength / 2f + _carSafeDistance, layerMask))
			wasHit = true;

		if (!wasHit)
			if (Physics.Raycast(transform.position + transform.right * (_carWidth / 2),
				(isForward ? 1 : -1) * transform.forward,
				out hit, _carLength / 2f + _carSafeDistance, layerMask))
				wasHit = true;

		if (!wasHit)
			if (Physics.Raycast(transform.position - transform.right * (_carWidth / 2),
				(isForward ? 1 : -1) * transform.forward,
				out hit, _carLength / 2f + _carSafeDistance, layerMask))
				wasHit = true;

		if (wasHit)
		{
			var po = hit.transform.gameObject.GetComponent<PunchableObject>();
			po.Punch(transform.forward * (isForward ? -1 : 1));
			return;
		}

		if (isForward)
			SetState(ECarState.MovingForward);
		else
			SetState(ECarState.MovingBackward);
	}

	private void SetState(ECarState newState)
	{
		switch (_state)
		{
			case ECarState.StandingStill:
				break;
			case ECarState.MovingForward:
				if (newState == ECarState.MovingForward ||
					newState == ECarState.MovingBackward)
					return;
				break;
			case ECarState.MovingBackward:
				if (newState == ECarState.MovingForward ||
					newState == ECarState.MovingBackward)
					return;
				break;
			case ECarState.Leaving: return;
		}
		_state = newState;
	}

	private void OnTriggerEnter(Collider otherCollider)
	{
		if (_state == ECarState.StandingStill) return;

		if (_state == ECarState.Leaving)
		{
			HandleTriggerEnterWhileLeaving(otherCollider);
			return;
		}

		HandleTriggerEnterWhileMoving(otherCollider);
	}

	private void HandleTriggerEnterWhileLeaving(Collider otherCollider)
	{
		switch (otherCollider.gameObject.layer)
		{
			case GameManager.RoadLayer:
				_currentRoad = (BoxCollider)otherCollider;
				break;
			case GameManager.ExitLayer:
				ReachedExit?.Invoke(this, EventArgs.Empty);
				gameObject.SetActive(false);
				break;
		}
	}

	private void HandleTriggerEnterWhileMoving(Collider otherCollider)
	{
		switch (otherCollider.gameObject.layer)
		{
			case GameManager.RoadLayer:
				if (IsMovingForward)
				{
					_currentRoad = (BoxCollider)otherCollider;
					SetState(ECarState.Leaving);
				}
				break;
			case GameManager.CarLayer:
				var otherCar = otherCollider.gameObject.GetComponent<Car>();

				if (otherCar.State == ECarState.StandingStill)
					otherCar.Punch(transform.forward * (IsMovingForward ? -1 : 1));

				if (otherCar.State == ECarState.Leaving) break;

				Kick();
				break;

			case GameManager.ObstacleLayer:
				var obstacle = otherCollider.gameObject.GetComponent<PunchableObject>();
				obstacle.Punch(transform.forward * (IsMovingForward ? -1 : 1));
				Kick();
				break;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		Debug.Log($"<color=lightblue>{GetType().Name}:</color> OnTriggerExit {other.gameObject.name}");

		if (other.gameObject.layer == GameManager.RoadLayer)
		{
			if (_state == ECarState.MovingBackward)
			{
				_currentRoad = (BoxCollider)other;
				SetState(ECarState.Leaving);
			}
		}
	}

	private void Kick()
	{
		PlayKickSound();

		if (IsMovingForward)
			_frontKickParticles.Play();
		else
			_backKickParticles.Play();

		SetState(ECarState.StandingStill);
	}

	private void PlayKickSound()
	{
		_kickAudioSource.Play();
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(transform.position + transform.forward * (_carLength / 2f), 0.05f);
		Gizmos.color = Color.cyan;
		Gizmos.DrawSphere(transform.position + transform.forward *
			(_carLength / 2f + _carSafeDistance), 0.05f);

		Gizmos.DrawSphere(transform.position + transform.right * (_carWidth / 2), 0.05f);
		Gizmos.DrawSphere(transform.position - transform.right * (_carWidth / 2), 0.05f);

		if (!Application.isPlaying) return;
		if (_currentRoad == null) return;

		Gizmos.color = Color.red;
		Gizmos.DrawSphere(_currentRoad.transform.position + _currentRoad.transform.forward *
			(_currentRoad.size.z / 2) * _currentRoad.transform.localScale.z, 0.05f);
	}

#endif

}
