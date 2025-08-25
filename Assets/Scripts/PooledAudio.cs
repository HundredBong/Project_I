using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledAudio : MonoBehaviour, IPooledObject
{
    public GameObject prefabReference { get; set; }
    private AudioSource _source;
    private Transform _targetTransform;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
    }

    public void PlaySFX(string clip)
    {
        _source.loop = false;
        _source.spatialBlend = 0f;

        transform.position = Vector3.zero;

        _source.clip = DataManager.Instance.GetAudioClipByKey(clip);
        _source.volume = AudioController.Instance.sfxVolume;
        _source.Play();

        float length = _source.clip.length;
        DelayCallManager.Instance.CallLater(length, () =>
        {
            ObjectPoolManager.Instance.audioPool.Return(this);
        });
    }

    public void PlaySFX(string clip, Transform target)
    {
        _targetTransform = target;

        _source.loop = false;
        _source.spatialBlend = 1f;

        _source.clip = DataManager.Instance.GetAudioClipByKey(clip);
        _source.volume = AudioController.Instance.sfxVolume;
        _source.Play();

        float length = _source.clip.length;
        DelayCallManager.Instance.CallLater(length, () =>
        {
            transform.position = Vector3.zero;
            _targetTransform = null;
            ObjectPoolManager.Instance.audioPool.Return(this);
        });
    }

    public void PlaySFX(string clip, Vector2 position)
    {
        _targetTransform = null;

        _source.loop = false;
        _source.spatialBlend = 1f;

        transform.position = new Vector3(position.x, position.y, 0);

        _source.clip = DataManager.Instance.GetAudioClipByKey(clip);
        _source.volume = AudioController.Instance.sfxVolume;
        _source.Play();

        float length = _source.clip.length;
        DelayCallManager.Instance.CallLater(length, () =>
        {
            transform.position = Vector3.zero;
            ObjectPoolManager.Instance.audioPool.Return(this);
        });
    }

    private void LateUpdate()
    {
        if (_targetTransform != null)
        {
            transform.position = _targetTransform.position;
        }
    }
}
