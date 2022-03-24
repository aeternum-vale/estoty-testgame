using NaughtyAttributes;
using UnityEngine;

public class Car : MonoBehaviour
{
	enum ECarState { Waiting, MovingForward, MovingBackward, Leaving }
	private const string RoadLayerName = "Road";

	[SerializeField] private float _speed;
	[SerializeField] private float _rotateSpeed = 0.1f;

	private ECarState _state = ECarState.Waiting;

	private BoxCollider _currentRoad;




	[Button]
	public void MoveForward()
	{
		SetState(ECarState.MovingForward);
	}

	[Button]
	public void MoveBackward()
	{
		SetState(ECarState.MovingBackward);
	}

	private void SetState(ECarState newState)
	{
		switch (_state)
		{
			case ECarState.Waiting:
				break;
			case ECarState.MovingForward:
				if (newState == ECarState.Leaving)
					break;
				else
					return;
			case ECarState.MovingBackward:
				if (newState == ECarState.Leaving)
					break;
				else
					return;
			case ECarState.Leaving: return;
		}


		_state = newState;
	}

	private void Update()
	{
		switch (_state)
		{
			case ECarState.Waiting: WaitingUpdate(); return;
			case ECarState.MovingForward: MovingUpdate(); return;
			case ECarState.MovingBackward: MovingUpdate(); return;
			case ECarState.Leaving: LeavingUpdate(); return;
		}
	}

	private void WaitingUpdate() { }

	private void MovingUpdate()
	{
		bool isForward = _state == ECarState.MovingForward;
		int dir = isForward ? 1 : -1;
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
		Debug.Log($"<color=lightblue>{GetType().Name}:</color> OnTriggerEnter");
		if (other.gameObject.layer == LayerMask.NameToLayer(RoadLayerName))
		{
			if (_state == ECarState.Leaving)
				_currentRoad = (BoxCollider)other;

			if (_state == ECarState.MovingForward)
			{
				_currentRoad = (BoxCollider)other;
				SetState(ECarState.Leaving);
			}
		}

	}

	private void OnTriggerExit(Collider other)
	{
		Debug.Log($"<color=lightblue>{GetType().Name}:</color> OnTriggerExit");

		if (other.gameObject.layer == LayerMask.NameToLayer(RoadLayerName))
		{
			if (_state == ECarState.MovingBackward)
			{
				_currentRoad = (BoxCollider)other;
				SetState(ECarState.Leaving);
			}
		}
	}


	private void OnDrawGizmos()
	{
		if (!Application.isPlaying) return;
		if (_currentRoad == null) return;

		Gizmos.color = Color.red;

		Gizmos.DrawSphere(_currentRoad.transform.position + _currentRoad.transform.forward * (_currentRoad.size.z / 2) * _currentRoad.transform.localScale.z, 0.1f);
	}

}
