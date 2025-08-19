using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    public EnemyPool enemyPool;
    public ProjectilePool projectilePool;
    public ParticlePool particlePool;
    public UIPool uiPool;
    public AudioPool audioPool;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.activeSceneChanged += ReturnAllPools;
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= ReturnAllPools;
    }

    private void ReturnAllPools(Scene arg0, Scene arg1)
    {
        enemyPool.ReturnAllEnemies();
        projectilePool.ReturnAllProjectiles();
        particlePool.ReturnAllParticles();
    }
}
