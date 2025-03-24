using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerInvincibility : MonoBehaviour
{
    [Header("Invincibility Settings")]
    [SerializeField] private float invincibilityDuration = 3f;
    [SerializeField] private float maxDuration = 30f;
    [SerializeField] private float flickerSpeed = 0.1f;
    [SerializeField] private Color invincibleColor = Color.red;

    [Header("Effects")]
    public GameObject invincibleEffect;
    public GameObject spawnedInvincibleEffect;
    [Header("Material Settings")]
    [SerializeField] private Material invincibleMaterial;
    [SerializeField] private float emissionIntensity = 5f;
    private List<Renderer> _childRenderers = new List<Renderer>();
    private List<Material[]> _originalMaterials = new List<Material[]>();
    private Material _invincibleMaterial;
    private bool _isInvincible = false;
    private float _invincibilityTimer = 0f;
    private Coroutine _flickerCoroutine;

    public bool IsInvincible => _isInvincible;

    void Start()
    {
        GetComponentsInChildren<Renderer>(true, _childRenderers);

        foreach (var renderer in _childRenderers)
        {
            // Создаем копии материалов для каждого рендерера
            Material[] mats = new Material[renderer.materials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = new Material(renderer.materials[i]);
            }
            _originalMaterials.Add(mats);
        }

        // Убедитесь что материал назначен в инспекторе
        if (invincibleMaterial == null)
            Debug.LogError("Assign Invincible Material in inspector!");
    }


    void Update()
    {
        if (_isInvincible)
        {
            _invincibilityTimer -= Time.deltaTime;
            if (_invincibilityTimer <= 0)
            {
                EndInvincibility();
            }
        }
    }

    public void ActivateInvincibility(float invincibleDuration)
    {
        if (invincibleDuration < maxDuration && invincibleDuration > 0)
        {
            invincibilityDuration = invincibleDuration;
        }

        if (!_isInvincible)
        {
            _isInvincible = true;
            _invincibilityTimer = invincibilityDuration;

            StartVisualEffects();
            Debug.Log("Игрок стал неуязвимым!");
        }
    }

    private void StartVisualEffects()
    {
        // Запускаем мерцание
        if (_flickerCoroutine != null) StopCoroutine(_flickerCoroutine);
        _flickerCoroutine = StartCoroutine(FlickerRoutine());

        // Дополнительные эффекты
        if (invincibleEffect != null)
        {
            spawnedInvincibleEffect = Instantiate(invincibleEffect, transform);
        }
    }

    private IEnumerator FlickerRoutine()
    {
        bool isVisible = true;
        while (_isInvincible)
        {
            foreach (var renderer in _childRenderers)
            {
                Material[] mats = renderer.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (isVisible)
                    {
                        mats[i] = invincibleMaterial;
                        mats[i].SetColor("_EmissionColor", invincibleColor * emissionIntensity);
                    }
                    else
                    {
                        mats[i] = _originalMaterials[_childRenderers.IndexOf(renderer)][i];
                    }
                }
                renderer.materials = mats;
            }

            isVisible = !isVisible;
            yield return new WaitForSeconds(flickerSpeed);
        }
        RestoreOriginalMaterials();
    }

    private void RestoreOriginalMaterials()
    {
        for (int i = 0; i < _childRenderers.Count; i++)
        {
            _childRenderers[i].materials = _originalMaterials[i];
        }
    }

    private void EndInvincibility()
    {
        _isInvincible = false;

        if (_flickerCoroutine != null)
        {
            StopCoroutine(_flickerCoroutine);
            _flickerCoroutine = null;
        }

        RestoreOriginalMaterials();

        if (spawnedInvincibleEffect != null)
        {
            Destroy(spawnedInvincibleEffect);
        }

        Debug.Log("Неуязвимость закончилась.");
    }

    void OnDestroy()
    {
        // Чистим материалы
        if (_invincibleMaterial != null)
        {
            Destroy(_invincibleMaterial);
        }
    }
}