using UnityEngine;

public class PlayerKillEnemy : MonoBehaviour
{
    [Range(90, 180)]
    public float killAngle = 120f; // ”гол дл€ убийства сзади (можно настраивать в инспекторе)
    [SerializeField] private PlayerController playerController;
    private void OnCollisionEnter(Collision collision)
    {
        EnemyPatrol enemy = collision.gameObject.GetComponent<EnemyPatrol>();

        if (enemy != null && !enemy.isDead)
        {
            playerController.animManager.ChangeAnimation("Stab");
            Transform enemyTransform = enemy.transform;

            // –ассчитываем направление к игроку в горизонтальной плоскости
            Vector3 directionToPlayer = transform.position - enemyTransform.position;
            directionToPlayer.y = 0;
            directionToPlayer.Normalize();

            // ѕолучаем направление взгл€да врага в горизонтальной плоскости
            Vector3 enemyForward = enemyTransform.forward;
            enemyForward.y = 0;
            enemyForward.Normalize();

            // ¬ычисл€ем угол между направлением взгл€да и игроком
            float angle = Vector3.Angle(enemyForward, directionToPlayer);


            if (angle >= killAngle)
            {
                Debug.Log("Enemy killed from behind");
                enemy.Die();

                // јктивируем стан игрока
                if (playerController != null)
                    playerController.TriggerKillStun();
            }
            collision.gameObject.GetComponent<CapsuleCollider>().isTrigger = true;
        }
    }
}