using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TypewriterEffect : MonoBehaviour
{
    // �������, ���������� �� ���������� ��������
    public event Action OnTypewriterComplete;

    [SerializeField] private float characterDelay = 0.05f; // �������� ����� ��������� � ��������

    private Text uiText; // ��� ������������ UI Text
    private TextMeshProUGUI tmpText; // ��� TextMeshPro
    private Coroutine typewriterCoroutine; // ��� ���������� ���������

    // �������������: ����������, ����� ��� ������ ������������
    private void Awake()
    {
        uiText = GetComponent<Text>();
        tmpText = GetComponent<TextMeshProUGUI>();

        if (uiText == null && tmpText == null)
        {
            Debug.LogError("TypewriterEffect: No Text or TextMeshProUGUI component found on this GameObject.");
        }
    }

    // ������ �������� ������ ������
    public void StartTyping(string text)
    {
        // ������������� ���������� ��������, ���� ��� �������
        StopTyping();

        // ��������� ����� ��������
        typewriterCoroutine = StartCoroutine(TypeTextCoroutine(text));
    }

    // ��������� ��������
    public void StopTyping()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        // ������� �����
        if (uiText != null) uiText.text = "";
        if (tmpText != null) tmpText.text = "";
    }

    // ������� �������� (������������ ����������� ������)
    public void SkipTyping(string text)
    {
        StopTyping();
        if (uiText != null) uiText.text = text;
        if (tmpText != null) tmpText.text = text;
        OnTypewriterComplete?.Invoke();
    }

    // �������� ��� �������� ������ ������
    private IEnumerator TypeTextCoroutine(string text)
    {
        // �������������� ������ �����
        if (uiText != null) uiText.text = "";
        if (tmpText != null) tmpText.text = "";

        // ������� ����� �� ������ �������
        foreach (char c in text)
        {
            if (uiText != null) uiText.text += c;
            if (tmpText != null) tmpText.text += c;
            yield return new WaitForSeconds(characterDelay);
        }

        // �������� ������� �� ����������
        OnTypewriterComplete?.Invoke();
    }
}