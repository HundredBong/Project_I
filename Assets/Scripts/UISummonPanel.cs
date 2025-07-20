using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class UISummonPanel : MonoBehaviour
{
    [Header("���� ī�װ�")]
    [SerializeField] private GameObject weaponCategory;
    [SerializeField] private GameObject armorCategory;
    [SerializeField] private GameObject necklaceCategory;
    [SerializeField] private GameObject skillCategory;

    [Header("ī�װ� ��ư")]
    [SerializeField] private Button summonWeaponButton;
    [SerializeField] private Button summonArmorButton;
    [SerializeField] private Button summonNecklaceButton;
    [SerializeField] private Button summonSkillButton;

    [Header("��ȯ ����")]
    [SerializeField] private TextMeshProUGUI summonLevelText;
    [SerializeField] private TextMeshProUGUI summonExpText;
    [SerializeField] private Image summonLevelFillImage;

    [Header("��ȯ ��ư")]
    [SerializeField] private Button summon10Button;
    [SerializeField] private Button summon30Button;

    [Header("���ö�����")]
    [SerializeField] private TextMeshProUGUI summonWeaponButtonText;
    [SerializeField] private TextMeshProUGUI summonArmorButtonText;
    [SerializeField] private TextMeshProUGUI summonNecklaceButtonText;
    [SerializeField] private TextMeshProUGUI summonSkillButtonText;
    [SerializeField] private TextMeshProUGUI summon10ButtonText;
    [SerializeField] private TextMeshProUGUI summon30ButtonText;

    [Header("����")]
    [SerializeField] private SummonRewardPannel rewardPanel;

    private SummonSubCategory category;

    private Dictionary<SummonSubCategory, GameObject> categoryObjecs;
    private Dictionary<SummonSubCategory, Button> categoryButtons;

    private void Awake()
    {
        //���� ��ȯ ���� �߰��Ǹ� ���⿡�� �߰��ؾ� ��

        categoryObjecs = new Dictionary<SummonSubCategory, GameObject>
        {
            { SummonSubCategory.Weapon, weaponCategory },
            { SummonSubCategory.Armor, armorCategory },
            { SummonSubCategory.Necklace, necklaceCategory },
            { SummonSubCategory.Skill, skillCategory },
        };

        categoryButtons = new Dictionary<SummonSubCategory, Button>
        {
            { SummonSubCategory.Weapon, summonWeaponButton },
            { SummonSubCategory.Armor, summonArmorButton },
            { SummonSubCategory.Necklace, summonNecklaceButton },
            { SummonSubCategory.Skill, summonSkillButton },
        };

        foreach (var kvp in categoryButtons)
        {
            SummonSubCategory category = kvp.Key;
            kvp.Value.onClick.AddListener(() => { ShowCategory(category); });
        }
    }

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += SetLocalizedText;
        SetLocalizedText();
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= SetLocalizedText;
    }

    private async void Start()
    {
        //�Ƕ� �λ� Ȥ�� �𸣴� �����½�ũ�� ���α��ϴµ�
        //�̷��� ù �ε������� �ҷ����� �� ���Ŀ� �ʱ�ȭ�ϸ� �������ٵ�
        //���� �߿��� �ε����� ���� 
        await UniTask.WaitUntil(() => GameManager.Instance.summonReady);

        SetLocalizedText();

        Refresh(SummonSubCategory.Weapon);
        ShowCategory(SummonSubCategory.Weapon);
    }

    private void ShowCategory(SummonSubCategory category)
    {
        foreach (var kvp in categoryObjecs)
        {
            kvp.Value.gameObject.SetActive(kvp.Key == category);
        }

        Refresh(category);
        rewardPanel.Refresh(category);
    }

    private void Refresh(SummonSubCategory category)
    {
        this.category = category;

        //��ư
        //��ȯ ��ư�� ������ �ִ� �̺�Ʈ ���� �� � �������� ��ȯ���� ���
        summon10Button.onClick.RemoveAllListeners();
        summon30Button.onClick.RemoveAllListeners();

        //��ų �ܿ��� ������ ��ȯ
        if (category != SummonSubCategory.Skill)
        {
            summon10Button.onClick.AddListener(() => { SummonItems(10); });
            summon30Button.onClick.AddListener(() => { SummonItems(30); });
        }
        //��ų�̸� ��ų ��ȯ
        else
        {
            summon10Button.onClick.AddListener(() => { SummonSkills(10); });
            summon30Button.onClick.AddListener(() => { SummonSkills(30); });
        }

        //��ȯ�ϸ� �޼��� ���� AddItem, AddExp ��������� �� �� �� ���η������� ���� ����
        //GameManager.Instance.statSaver.SaveSummonProgress(GameManager.Instance.SummonManager.GetSummonProgressData());

        //��ȯ ���� �ؽ�Ʈ, �̹��� fill �ʱ�ȭ
        int level = GameManager.Instance.SummonManager.GetLevel(category);
        int currentExp = GameManager.Instance.SummonManager.GetExp(category);
        int maxExp = DataManager.Instance.GetSummonMaxExp(category, level);

        summonLevelText.text = $"{level}"; //SumonManager���� ������ ī�װ��� ����
        summonExpText.text = $"{currentExp}/{maxExp}"; //SummonManager���� ������ ī�װ��� ����ġ
        summonLevelFillImage.fillAmount = (float)currentExp / maxExp; //DataManager���� �ҷ��� ī�װ��� MaxExp��, SummonManager���� ������ ī�װ��� exp��
    }

    private void SetLocalizedText()
    {
        summonWeaponButtonText.text = DataManager.Instance.GetLocalizedText("UI_SummonWeapon");
        summonArmorButtonText.text = DataManager.Instance.GetLocalizedText("UI_SummonArmor");
        summonNecklaceButtonText.text = DataManager.Instance.GetLocalizedText("UI_SummonNecklace");
        summonSkillButtonText.text = DataManager.Instance.GetLocalizedText("UI_SummonSkill");
        summon10ButtonText.text = DataManager.Instance.GetLocalizedText("UI_Summon10");
        summon30ButtonText.text = DataManager.Instance.GetLocalizedText("UI_Summon30");
    }

    private void SummonItems(int count)
    {
        //TODO : ���� ����ǥ CSV ��� �ӽ÷� ����
        int amount = count == 10 ? 1000 : 2500;
        Queue<ItemData> itemDatas = new Queue<ItemData>();
        //���̾� ����
        if (GameManager.Instance.stats.TrySpendItem(PlayerProgressType.Diamond, amount))
        {
            for (int i = 0; i < count; i++)
            {
                //���� ī�װ��� ���� ���
                int summonLevel = GameManager.Instance.SummonManager.GetLevel(category);
                //���� ������ ���� �������� ��� ���
                GradeType grade = DataManager.Instance.GetRandomGrade(category, summonLevel);
                //������ ������� �������� �ܰ� ���
                int stage = DataManager.Instance.GetRandomStage(category, summonLevel, grade);
                //������ �̱�
                int itemId = DataManager.Instance.GetRandomItemId(category, grade, stage);
                //ItemData ��������
                ItemData itemData = DataManager.Instance.GetItemData()[itemId];
                //�κ��丮 �߰�
                InventoryManager.Instance.AddItem(itemData);
                //�˾���
                itemDatas.Enqueue(itemData);
            }

            //count ��ŭ ����ġ ����
            GameManager.Instance.SummonManager.AddExp(category, count);
            //�̱� ���� ����
            GameManager.Instance.statSaver.SaveSummonProgressAsync(GameManager.Instance.SummonManager.BuildSummonProgressData()).Forget();
            //�κ��丮 ���� ����
            GameManager.Instance.statSaver.SaveInventoryDataAsync(InventoryManager.Instance.BuildSaveData()).Forget();
            //��ȭ ���� ����
            GameManager.Instance.statSaver.SavePlayerProgressDataAsync(GameManager.Instance.stats.GetProgressSaveData()).Forget();

            //�κ��丮 ������ ���ΰ�ħ
            if (UIManager.Instance.TryGetPage<UIInventoryPage>(out UIInventoryPage inventoryPage))
            {
                inventoryPage.RefreshAll();
            }

            UIManager.Instance.PopupOpen<UISummonResultPopup>().StartDisplayingResult(itemDatas);
            Refresh(category);
            rewardPanel.Refresh(category);
        }
        else
        {
            Debug.LogWarning($"[UISummonPanel] ���̾ư� ������, {GameManager.Instance.stats.Diamond}");
        }
    }

    private void SummonSkills(int count)
    {
        //TODO : ���� ����ǥ CSV ��� �ӽ÷� ����
        int amount = count == 10 ? 1000 : 2500;
        Queue<SkillData> skillDatas = new Queue<SkillData>();
        //���̾� ����
        if (GameManager.Instance.stats.TrySpendItem(PlayerProgressType.Diamond, amount))
        {
            for (int i = 0; i < count; i++)
            {
                //���� ī�װ��� ���� ���
                int summonLevel = GameManager.Instance.SummonManager.GetLevel(category);
                //���� ������ ���� ��ų ��� ���
                GradeType grade = DataManager.Instance.GetRandomGrade(category, summonLevel);
                //��ų ������� ��ų ���̵� �̱�

                //Debug.Log($"[UISummonPanel] ��ȯ ī�װ� : {category}, ���� : {summonLevel}, ��� : {grade}");

                SkillId skillId = DataManager.Instance.GetRandomSkillId(category, grade);
                skillDatas.Enqueue(DataManager.Instance.GetSkill(skillId));
                SkillManager.Instance.AddSkill(skillId);
            }

            //count ��ŭ ����ġ ����
            GameManager.Instance.SummonManager.AddExp(category, count);
            //�̱� ���� ����
            GameManager.Instance.statSaver.SaveSummonProgressAsync(GameManager.Instance.SummonManager.BuildSummonProgressData()).Forget();
            //��ȭ ���� ����
            GameManager.Instance.statSaver.SavePlayerProgressDataAsync(GameManager.Instance.stats.GetProgressSaveData()).Forget();
            //��ų ���� ����
            GameManager.Instance.statSaver.SavePlayerSkillDataAsync(SkillManager.Instance.BuildSaveData()).Forget();

            //��ų ������ ���ΰ�ħ
            if (UIManager.Instance.TryGetPage<UISkillPage>(out UISkillPage skillPage))
            {
                skillPage.RefreshAll();
            }

            UIManager.Instance.PopupOpen<UISummonResultPopup>().StartDisplayingResult(skillDatas);
            Refresh(category);
            rewardPanel.Refresh(category);
        }
        else
        {
            Debug.LogWarning($"[UISummonPanel] ���̾ư� ������, {GameManager.Instance.stats.Diamond}");
        }
    }
}

//��
//������ �غ��ô�
//ī�װ� ������ �� ī�װ��� �°� ���ΰ�ħ ����� ��
//���ΰ�ħ �Ҷ� ���� ��ȯ �����̳�, ����ġ �����͵� �ʱ�ȭ ����� ��.
//�׷� Enum���� ����ī�װ��ϳ� �����,
//Refresh������ �� enum�� ���ڷ� �޾Ƽ� �ʱ�ȭ ���ָ� �� �� ����
//��� Refresh�� �ְ�, Init�� �� �־�� �ϳ� �ͱ⵵ ��
//ù ����� Refresh(SubCategory.Weapon) ���ְ�,
//��ư ���� ������ �� ī�װ� ���� �־ �ʱ�ȭ ���ָ� �ɰŰ�����

//���� � ī�װ����� enum�ʵ带 �ϳ� ���� �� Init�� �ʱ�ȭ �ϰ�
//�̱��ư ������ �� ī�װ����� AddItem������ �ϸ� ���� ������
//���� ī�װ��� �� ����� ����̱� �̷�������
//�ᱹ if�� ������ switch������ ���ڴٴ°� �ƴ�?
//�׷��� ��ư���� �Լ� �� ���� ����°ź��� �����ǰ�?

//�׷� ������ ������
//����ī�װ� enum�� ������
//�����͸Ŵ������� csv �о�ͼ�
//��ųʸ�<����ī�װ�, int[]>�� �ְ�?
//�迭? int�迭?
//��ȯ�����̴ϱ� ��� ũ�� �ö��� �������� ��κ��� Ű���� ������ ������ 1000�Ѿ���� ������
//�׷� �� ���� ���̺� �Ⱦ��� ��� �� �ٿ��� �����ݷ����� 10;20;30 �̷� ������ ������ �迭�� �ȵǳ� ����Ʈ�� ������?
//�ƴϸ� exp���̺��� int, float ��ųʸ��� ����, �ʿ��� ����ġ ��
//�׷� ���� ��ųʸ� ����ϳ�
//��ųʸ� <����ī�װ�, ��ųʸ�<int, int>> ��ȯ���� ������
//�� �����ѵ�
//�ұԸ�ϱ� ���� ��ųʸ� ��� �׷��ϱ� ������ 3�������� �Ҳ��ϱ�
//�ϴ��� <����ī�װ�, List<int>>�� �����ϰ� �غ���
//���� �߰��ȴٸ� ��ųʸ��ȿ� ��ųʸ��� �����丵�� �ϴ� �ϸ� �� �� ����

//������ ���� ����ϴٰ� �� ��Ⱑ ������ �Ȱ���
//������ ������ MonoBehaviour�� ��ӹ��� �ʴ� ���� C#Ŭ���� �ϳ� �ۼ� 
//UI�� ���� �ǵ帮�°͵� �ƴϰ�, �����ֱ⵵ �ʿ� ����
//�׳� Ư�� ��ȯ�� ������ ���̳�, ����ġ�� �󸶳�, �̷��͸� �����ϰ�
//ù ����� GameManager�� �ε� ��û, ������ ������ GameManager�� ���� ��û�� �ϸ� �ɵ�??
//�ٵ� �̰� ��?

//�̱�� Ȯ��ǥ �ۼ��� �ؾ���
//??
//��ųʸ�<ī�װ�, ��ųʸ�<����, ����Ʈ<����ġ>>>?????????????
//Dictionry<SummonSubCategory, Dictionary<int, List<float>>>?
//            ��ȯ����                    ����       ����ġ
//���� Ŭ������ �ϴ°�
//public class Test
//{
//    public SummonSubCategory Category;
//    public Dictionary<int, float> ExpTable;

//}