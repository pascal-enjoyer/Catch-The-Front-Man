using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TypewriterEffect : MonoBehaviour
{
    // Событие, вызываемое по завершении анимации
    public event Action OnTypewriterComplete;

    [SerializeField] private float characterDelay = 0.05f; // Задержка между символами в секундах

    private Text uiText; // Для стандартного UI Text
    private TextMeshProUGUI tmpText; // Для TextMeshPro
    private Coroutine typewriterCoroutine; // Для управления корутиной

    // Инициализация: определяем, какой тип текста используется
    private void Awake()
    {
        uiText = GetComponent<Text>();
        tmpText = GetComponent<TextMeshProUGUI>();

        if (uiText == null && tmpText == null)
        {
            Debug.LogError("TypewriterEffect: No Text or TextMeshProUGUI component found on this GameObject.");
        }
    }

    // Запуск анимации набора текста
    public void StartTyping(string text)
    {
        // Останавливаем предыдущую анимацию, если она активна
        StopTyping();

        // Запускаем новую корутину
        typewriterCoroutine = StartCoroutine(TypeTextCoroutine(text));
    }

    // Остановка анимации
    public void StopTyping()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        // Очищаем текст
        if (uiText != null) uiText.text = "";
        if (tmpText != null) tmpText.text = "";
    }

    // Пропуск анимации (моментальное отображение текста)
    public void SkipTyping(string text)
    {
        StopTyping();
        if (uiText != null) uiText.text = text;
        if (tmpText != null) tmpText.text = text;
        OnTypewriterComplete?.Invoke();
    }

    // Корутина для анимации набора текста
    private IEnumerator TypeTextCoroutine(string text)
    {
        // Инициализируем пустой текст
        if (uiText != null) uiText.text = "";
        if (tmpText != null) tmpText.text = "";

        // Выводим текст по одному символу
        foreach (char c in text)
        {
            if (uiText != null) uiText.text += c;
            if (tmpText != null) tmpText.text += c;
            yield return new WaitForSeconds(characterDelay);
        }

        // Вызываем событие по завершении
        OnTypewriterComplete?.Invoke();
    }
}