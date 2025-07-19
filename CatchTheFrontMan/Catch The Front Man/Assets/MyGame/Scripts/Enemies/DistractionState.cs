using UnityEngine;
using UnityEngine.UI;

public class DistractedState : IEnemyState
{
    private readonly Vector3 _distractionPoint;
    private float _timer;
    private GameObject _spriteInstance;
    private FillableObject _fillableObject;

    public DistractedState(Vector3 point)
    {
        _distractionPoint = point;
    }

    public void Enter(EnemyStateMachine context)
    {
        EnemyEars ears = context.GetComponent<EnemyEars>();
        if (ears == null)
        {
            Debug.LogError($"EnemyEars component missing on {context.gameObject.name}!");
            return;
        }

        _timer = ears.DistractionDuration;
        context.Animator.ChangeAnimation("Idle");

        // ������� ������ ��� ������
        if (ears.DistractionSpritePrefab != null)
        {
            _spriteInstance = Object.Instantiate(
                ears.DistractionSpritePrefab,
                context.transform.position + Vector3.up * ears.SpriteHeightOffset,
                Quaternion.identity,
                context.transform
            );
            _fillableObject = _spriteInstance.GetComponent<FillableObject>();
            if (_fillableObject != null)
            {
                _fillableObject.SetFillColor(Color.red);
                _fillableObject.SetFillMethod(Image.FillMethod.Vertical, Image.OriginVertical.Top);
                _fillableObject.SetFillAmount(1f);
            }
            else
            {
                Debug.LogError($"Distraction sprite on {context.gameObject.name} missing FillableObject component!");
            }
        }
        else
        {
            Debug.LogError($"Distraction Sprite Prefab not assigned on {context.gameObject.name}!");
        }

        Debug.Log($"Enemy {context.gameObject.name} distracted to look at {_distractionPoint} for {ears.DistractionDuration}s");
    }

    public IEnemyState Update(EnemyStateMachine context)
    {
        if (context.Enemy.IsDead || !context.Enemy.IsActive)
        {
            return new PatrolState(); // ������������ � �������, ���� ���� ���������
        }

        // ���� ����� �����, ��������� � ChaseState
        if (context.EnemyVision.IsPlayerVisible)
        {
            return new ChaseState();
        }

        // �������������� � ����� ����������
        Vector3 direction = (_distractionPoint - context.transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            context.transform.rotation = Quaternion.Slerp(
                context.transform.rotation,
                lookRotation,
                context.Movement.RotationSpeed * Time.deltaTime
            );
        }

        // ��������� ������ � ������
        EnemyEars ears = context.GetComponent<EnemyEars>();
        if (ears != null)
        {
            _timer -= Time.deltaTime;
            if (_fillableObject != null)
            {
                _fillableObject.SetFillAmount(_timer / ears.DistractionDuration);
            }

            // ���� ����� ���������� �������, ������������ � �������
            if (_timer <= 0)
            {
                return new PatrolState();
            }
        }

        return null; // �������� � ������� ���������
    }

    public void Exit(EnemyStateMachine context)
    {
        if (_spriteInstance != null)
        {
            Object.Destroy(_spriteInstance);
        }
        context.Animator.ChangeAnimation("Idle");
        Debug.Log($"Enemy {context.gameObject.name} stopped distraction");
    }
}