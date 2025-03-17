using UnityEngine;

public class PlayerKillEnemy : MonoBehaviour
{
    [Range(90, 180)]
    public float killAngle = 120f; // ���� ��� �������� ����� (����� ����������� � ����������)
    [SerializeField] private PlayerController playerController;

    private void OnTriggerEnter(Collider other)
    {
        EnemyPatrol enemy = other.gameObject.GetComponent<EnemyPatrol>();

        if (enemy != null && !enemy.isDead)
        {
            Transform enemyTransform = enemy.transform;
            // ������������ ����������� � ������ � �������������� ���������
            Vector3 directionToPlayer = transform.position - enemyTransform.position;
            directionToPlayer.y = 0;
            directionToPlayer.Normalize();
            // �������� ����������� ������� ����� � �������������� ���������
            Vector3 enemyForward = enemyTransform.forward;
            enemyForward.y = 0;
            enemyForward.Normalize();
            

            // ��������� ���� ����� ������������ ������� � �������
            float angle = Vector3.Angle(enemyForward, directionToPlayer);

            if (angle >= killAngle)
            {
                playerController.animManager.ChangeAnimation("Stab");
                Debug.Log("Enemy killed from behind");
                enemy.Die();
                // ���������� ���� ������
                if (playerController != null)
                    playerController.TriggerKillStun();
            }
            other.gameObject.GetComponent<CapsuleCollider>().isTrigger = true;
        }

    }
}