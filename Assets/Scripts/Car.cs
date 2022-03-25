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
	private ECarState State => _state;

	private BoxCollider _currentRoad;

	public event EventHandler ReachedExit;

	private bool IsMovingForward => _state == ECarState.MovingForward;
	private bool IsMoving => (_state == ECarState.MovingForward || _state == ECarState.MovingBackward);


	[Button]
	public void MoveForward()
	{
		Debug.Log($"<color=lightblue>{GetType().Name}:</color> try move forward");
		if (CanMove(true))
		{
			Debug.Log($"<color=lightblue>{GetType().Name}:</color> success move forward");
			SetState(ECarState.MovingForward);
		}
	}

	[Button]
	public void MoveBackward()
	{
		Debug.Log($"<color=lightblue>{GetType().Name}:</color> try move backward");

		if (CanMove(false))
		{
			Debug.Log($"<color=lightblue>{GetType().Name}:</color> success move backward");

			SetState(ECarState.MovingBackward);
		}
	}

	private bool CanMove(bool forward)
	{
		int layerMask = (1 << GameManager.CarLayer) | (1 << GameManager.ObstacleLayer);

		RaycastHit hit;
		if (Physics.Raycast(transform.position, (forward ? 1 : -1) * transform.forward, out hit, _carLength / 2f + _carSafeDistance, layerMask))
			return false;

		if (Physics.Raycast(transform.position + transform.right * (_carWidth / 2),
			(forward ? 1 : -1) * transform.forward, out hit, _carLength / 2f + _carSafeDistance, layerMask))
			return false;

		if (Physics.Raycast(transform.position - transform.right * (_carWidth / 2),
			(forward ? 1 : -1) * transform.forward, out hit, _carLength / 2f + _carSafeDistance, layerMask))
			return false;

		return true;
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

	private void Update()
	{
		switch (_state)
		{
			case ECarState.StandingStill: StandingStillUpdate(); return;
			case ECarState.MovingForward: MovingUpdate(); return;
			case ECarState.MovingBackward: MovingUpdate(); return;
			case ECarState.Leaving: LeavingUpdate(); return;
		}
	}

	private void StandingStillUpdate() { }

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


	private void OnTriggerEnter(Collider other)
	{
		Debug.Log($"<color=lightblue>{GetType().Name}:</color> OnTriggerEnter {other.gameObject.name}");

		if (_state == ECarState.StandingStill) return;

		if (_state == ECarState.Leaving)
		{
			switch (other.gameObject.layer)
			{
				case GameManager.RoadLayer:
					_currentRoad = (BoxCollider)other;
					break;
				case GameManager.ExitLayer:
					ReachedExit?.Invoke(this, EventArgs.Empty);
					gameObject.SetActive(false);

					break;
			}
			return;
		}

		switch (other.gameObject.layer)
		{
			case GameManager.RoadLayer:
				if (_state == ECarState.MovingForward)
				{
					_currentRoad = (BoxCollider)other;
					SetState(ECarState.Leaving);
				}
				break;
			case GameManager.CarLayer:
				var otherCar = other.gameObject.GetComponent<Car>();

				if (otherCar.State == ECarState.StandingStill)
					otherCar.Punch(transform.forward * (IsMovingForward ? -1 : 1));

				if (otherCar.State == ECarState.Leaving) break;

				Kick();
				break;

			case GameManager.ObstacleLayer:
				var obstacle = other.gameObject.GetComponent<PunchableObject>();
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
		if (IsMovingForward)
			_frontKickParticles.Play();
		else
			_backKickParticles.Play();

		PlayKickSound();

		SetState(ECarState.StandingStill);
	}

	private void PlayKickSound()
	{
		_kickAudioSource.Play();
	}


	private void OnCollisionEnter(Collision other)
	{
		Debug.Log($"<color=lightblue>{GetType().Name}:</color> OnCollisionEnter {other.gameObject.name}");
	}
	private void OnCollisionExit(Collision other)
	{
		Debug.Log($"<color=lightblue>{GetType().Name}:</color> OnCollisionExit {other.gameObject.name}");

	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(transform.position + transform.forward * (_carLength / 2f), 0.1f);
		Gizmos.color = Color.cyan;
		Gizmos.DrawSphere(transform.position + transform.forward * (_carLength / 2f + _carSafeDistance), 0.1f);

		Gizmos.DrawSphere(transform.position + transform.right * (_carWidth / 2), 0.1f);
		Gizmos.DrawSphere(transform.position - transform.right * (_carWidth / 2), 0.1f);

		if (!Application.isPlaying) return;
		if (_currentRoad == null) return;

		Gizmos.color = Color.red;
		Gizmos.DrawSphere(_currentRoad.transform.position + _currentRoad.transform.forward * (_currentRoad.size.z / 2) * _currentRoad.transform.localScale.z, 0.1f);


	}

}
