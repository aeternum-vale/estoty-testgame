using DG.Tweening;
using Gamelogic.Extensions;
using UnityEngine;

public class PunchableObject : MonoBehaviour
{
	[SerializeField] private float _punchForce;
	private Tween _punchTween;

	public void Punch(Vector3 direction)
	{
		_punchTween.Kill();
		transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
		direction = new Vector3(-direction.z, direction.z, direction.x);

		var localDirection = transform.InverseTransformDirection(direction);

		_punchTween = transform.DOPunchRotation(localDirection.WithY(0) * _punchForce, 1f, 4);
	}

}
