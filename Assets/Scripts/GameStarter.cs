using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStarter : MonoBehaviour
{
    [SerializeField] private DungeonType _dungeonType;

    private void Start()
    {
        StageManager.Instance.StartStage(_dungeonType);
    }
}
