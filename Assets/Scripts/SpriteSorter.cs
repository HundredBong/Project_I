using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(SortingGroup))]
public class SpriteSorter : MonoBehaviour
{
    private SortingGroup sortingGroup;

    private void Awake()
    {
        sortingGroup = GetComponent<SortingGroup>();
    }

    private void LateUpdate()
    {
        int order = Mathf.RoundToInt(-transform.position.y * 10);

        if (sortingGroup.sortingOrder != order)
        {
            sortingGroup.sortingOrder = order;
        }
    }
}
