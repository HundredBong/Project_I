using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIShopPage : UIPage
{
    [Header("카테고리")]
    [SerializeField] private GameObject summonCategory;
    [SerializeField] private GameObject normalCategory;
    [SerializeField] private GameObject skillCategory;
    [SerializeField] private GameObject scoreCategory;
    [SerializeField] private GameObject packageCategory;
    //[SerializeField] private GameObject cashCategory;

    [Header("각 카테고리 버튼")]
    [SerializeField] private Button summonButton;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button skillButton;
    [SerializeField] private Button scoreButton;
    [SerializeField] private Button packageButton;
    //[SerializeField] private Button cashButton;

    private Dictionary<ShopCategory, GameObject> categoryObjects;
    private Dictionary<ShopCategory, Button> categoryButtons;

    protected override void Awake()
    {
        base.Awake();

        //추후 상점 종류 추가되면 여기에도 추가해야 함

        categoryObjects = new Dictionary<ShopCategory, GameObject>
        {
            { ShopCategory.Summon, summonCategory },
            { ShopCategory.Normal, normalCategory },
            { ShopCategory.Skill, skillCategory },
            { ShopCategory.Score, scoreCategory },
            { ShopCategory.Package, packageCategory },
            //{ ShopCategory.Cash, cashCategory },
        };

        categoryButtons = new Dictionary<ShopCategory, Button>
        {
            { ShopCategory.Summon, summonButton },
            { ShopCategory.Normal, normalButton },
            { ShopCategory.Skill, skillButton },
            { ShopCategory.Score, scoreButton },
            { ShopCategory.Package, packageButton },
            //{ ShopCategory.Cash, cashButton },
        };

        foreach (var kvp in categoryButtons)
        {
            ShopCategory category = kvp.Key;
            kvp.Value.onClick.AddListener(() => ShowCategory(category));
        }
    }

    private void ShowCategory(ShopCategory targetCategory)
    {
        //인자로 들어온 카테고리에 해당하는 오브젝트만 활성화, 그 외 비활성화
        foreach (var kvp in categoryObjects)
        {
            kvp.Value.SetActive(kvp.Key == targetCategory);
        }
    }

    private void Start()
    {
        //처음 상점 들어오면 소환 먼저 보이게
        ShowCategory(ShopCategory.Summon);
    }
}
