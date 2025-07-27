using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ImageRegister : MonoBehaviour
{
    private Image _fadeImage;

    private void Awake()
    {
        _fadeImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_fadeImage != null)
        {
            UIManager.Instance.RegisterFadeImage(_fadeImage);
        }
        else
        {
            Debug.LogWarning("[ImageRegister] _fadeImage∞° null¿”");
        }
    }
}
