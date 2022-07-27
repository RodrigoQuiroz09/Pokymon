using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class Character : MonoBehaviour
{
    public bool isMoving { get; private set; }
    [SerializeField] private float speed;
    private CharacterAnimator _animator;
    public CharacterAnimator Animator => _animator;
    void Awake()
    {
        _animator = GetComponent<CharacterAnimator>();
    }

    public IEnumerator MoveTowards(Vector3 moveVector, UnityAction OnMoveFinish = null)
    {
        if (moveVector.x != 0) moveVector.y = 0;
        _animator.MoveX = Mathf.Clamp(moveVector.x, -1, 1);
        _animator.MoveY = Mathf.Clamp(moveVector.y, -1, 1);

        var targetPosition = transform.position;
        targetPosition.x += moveVector.x;
        targetPosition.y += moveVector.y;

        if (!IsPathAvailable(targetPosition)) yield break;

        isMoving = true;
        while (Vector3.Distance(transform.position, targetPosition) > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
        OnMoveFinish?.Invoke();
    }

    public void LookTowards(Vector3 target)
    {
        var diff = target - transform.position;
        var xdiff = Mathf.FloorToInt(diff.x);
        var ydiff = Mathf.FloorToInt(diff.y);

        if (xdiff == 0 || ydiff == 0)
        {
            _animator.MoveX = Mathf.Clamp(xdiff, -1, 1);
            _animator.MoveY = Mathf.Clamp(ydiff, -1, 1);
        }
        else
        {
            Debug.LogError("Error: El personaje no puede moverse y mirar en diagonal...");
        }
    }

    public void HandleUpdate()
    {
        _animator.isMoving = isMoving;
    }

    private bool IsPathAvailable(Vector3 target)
    {
        var path = target - transform.position;
        var direction = path.normalized;
        return !Physics2D.BoxCast(transform.position + direction,
                new Vector2(0.3f, 0.3f), 0f, direction, path.magnitude - 1,
                GameLayers.SharedInstance.CollisionLayers);
    }

    private bool IsAvailable(Vector3 target)
    {
        if (Physics2D.OverlapCircle(target, 0.2f, GameLayers.SharedInstance.SolidObjLayers | GameLayers.SharedInstance.InteractableLayer) != null)
        {
            return false;
        }
        return true;
    }
}
