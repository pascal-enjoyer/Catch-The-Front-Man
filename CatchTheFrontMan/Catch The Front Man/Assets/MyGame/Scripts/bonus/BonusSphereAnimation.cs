using UnityEngine;

public class BonusSphereAnimation : MonoBehaviour
{
    public float floatAmplitude = 0.5f; // Высота движения вверх-вниз
    public float floatFrequency = 1f;   // Частота движения вверх-вниз
    public float rotationSpeed = 50f;   // Скорость вращения вокруг оси

    private Vector3 startPosition; // Начальная позиция объекта

    void Start()
    {
        // Запоминаем начальную позицию объекта
        startPosition = transform.position;
    }

    void Update()
    {
        // Движение вверх-вниз по синусоиде
        float newY = startPosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Вращение вокруг оси Y
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
