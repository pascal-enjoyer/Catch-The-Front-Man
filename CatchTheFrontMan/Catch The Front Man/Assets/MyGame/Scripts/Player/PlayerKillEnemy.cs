using UnityEngine;

public class PlayerKillEnemy : MonoBehaviour
{
    [Range(90, 180)]
    public float killAngle = 120f; // Угол для убийства врага
    [SerializeField] private PlayerController playerController;

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();

        if (enemy != null && !enemy.IsDead)
        {
            Transform enemyTransform = enemy.transform;
            // Вычисляем направление к игроку относительно врага
            Vector3 directionToPlayer = transform.position - enemyTransform.position;
            directionToPlayer.y = 0;
            directionToPlayer.Normalize();
            // Направление вперёд врага
            Vector3 enemyForward = enemyTransform.forward;
            enemyForward.y = 0;
            enemyForward.Normalize();

            // Проверяем угол направления игрока и врага
            float angle = Vector3.Angle(enemyForward, directionToPlayer);

            if (angle >= killAngle)
            {
                playerController.animManager.ChangeAnimation("Stab");
                Debug.Log("Enemy killed from behind");
                enemy.Die();
                // Оглушаем игрока
                if (playerController != null)
                    playerController.TriggerKillStun();
            }
            other.gameObject.GetComponent<CapsuleCollider>().isTrigger = true;
        }
    }
}