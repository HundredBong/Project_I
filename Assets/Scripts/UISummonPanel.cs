using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class UISummonPanel : MonoBehaviour
{
    [Header("서브 카테고리")]
    [SerializeField] private GameObject weaponCategory;
    [SerializeField] private GameObject armorCategory;
    [SerializeField] private GameObject necklaceCategory;
    [SerializeField] private GameObject skillCategory;

    [Header("카테고리 버튼")]
    [SerializeField] private Button summonWeaponButton;
    [SerializeField] private Button summonArmorButton;
    [SerializeField] private Button summonNecklaceButton;
    [SerializeField] private Button summonSkillButton;

    [Header("소환 레벨")]
    [SerializeField] private TextMeshProUGUI summonLevelText;
    [SerializeField] private TextMeshProUGUI summonExpText;
    [SerializeField] private Image summonLevelFillImage;

    [Header("버튼")]
    [SerializeField] private Button summon10Button;
    [SerializeField] private Button summon30Button;

    private SummonSubCategory category;

    private Dictionary<SummonSubCategory, GameObject> categoryObjecs;
    private Dictionary<SummonSubCategory, Button> categoryButtons;

    private void Awake()
    {
        //추후 소환 종류 추가되면 여기에도 추가해야 함

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
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= SetLocalizedText;
    }

    private async void Start()
    {
        //또또 인생 혹시 모르니 유니태스크로 감싸긴하는데
        //이런건 첫 로딩씬에서 불러오고 그 이후에 초기화하면 안전할텐데
        //제일 중요한 로딩씬이 없네 
        await UniTask.WaitUntil(() => GameManager.Instance.summonReady);

        SetLocalizedText();

        Refresh(SummonSubCategory.Weapon);
    }

    private void ShowCategory(SummonSubCategory category)
    {
        foreach (var kvp in categoryObjecs)
        {
            kvp.Value.gameObject.SetActive(kvp.Key == category);
        }

        Refresh(category);
    }

    private void Refresh(SummonSubCategory category)
    {
        this.category = category;

        //버튼
        //소환 버튼에 기존에 있던 이벤트 삭제 및 어떤 아이템을 소환할지 등록
        summon10Button.onClick.RemoveAllListeners();
        summon30Button.onClick.RemoveAllListeners();

        summon10Button.onClick.AddListener(() => { SummonItems(10); });
        summon30Button.onClick.AddListener(() => { SummonItems(30); });
        //소환하면 메서드 끝에 AddItem, AddExp 실행해줘야 함 둘 다 내부로직에서 저장 안함
        //GameManager.Instance.statSaver.SaveSummonProgress(GameManager.Instance.SummonManager.GetSummonProgressData());

        //소환 레벨 텍스트 및 FillAmount 초기화
        int level = GameManager.Instance.SummonManager.GetLevel(category);
        int currentExp = GameManager.Instance.SummonManager.GetExp(category);
        int maxExp = DataManager.Instance.GetSummonMaxExp(category, level);

        summonLevelText.text = $"{level}"; //SumonManager에서 가져온 카테고리별 레벨
        summonExpText.text = $"{currentExp}/{maxExp}"; //SummonManager에서 가져온 카테고리별 경험치
        summonLevelFillImage.fillAmount = (float)currentExp / maxExp; //DataManager에서 불러온 카테고리별 MaxExp랑, SummonManager에서 가져온 카테고리별 exp값
    }

    private void SetLocalizedText()
    {
        summonWeaponButton.GetComponentInChildren<TextMeshProUGUI>().text = DataManager.Instance.GetLocalizedText("UI_SummonWeapon");
        summonArmorButton.GetComponentInChildren<TextMeshProUGUI>().text = DataManager.Instance.GetLocalizedText("UI_SummonArmor");
        summonNecklaceButton.GetComponentInChildren<TextMeshProUGUI>().text = DataManager.Instance.GetLocalizedText("UI_SummonNecklace");
        summonSkillButton.GetComponentInChildren<TextMeshProUGUI>().text = DataManager.Instance.GetLocalizedText("UI_SummonSkill");
        summon10Button.GetComponentInChildren<TextMeshProUGUI>().text = DataManager.Instance.GetLocalizedText("UI_Summon10");
        summon30Button.GetComponentInChildren<TextMeshProUGUI>().text = DataManager.Instance.GetLocalizedText("UI_Summon30");
    }

    private void SummonItems(int count)
    {
        //상점 가격표 CSV 대신 임시로 설정
        int amount = count == 10 ? 1000 : 2500;
        Queue<ItemData> itemDatas = new Queue<ItemData>();
        //다이아 감소
        if (GameManager.Instance.stats.TrySpendItem(PlayerProgressType.Diamond, amount))
        {
            for (int i = 0; i < count; i++)
            {
                //현재 카테고리의 레벨 계산
                int summonLevel = GameManager.Instance.SummonManager.GetLevel(category);
                //현재 레벨로 뽑을 아이템의 등급 계산
                GradeType grade = DataManager.Instance.GetRandomGrade(category, summonLevel);
                //레벨과 등급으로 아이템의 단계 계산
                int stage = DataManager.Instance.GetRandomStage(category, summonLevel, grade);
                //아이템 뽑기
                int itemId = DataManager.Instance.GetRandomItemId(category, grade, stage);
                //ItemData 가져오기
                ItemData itemData = DataManager.Instance.GetItemData()[itemId];
                //인벤토리 추가
                InventoryManager.Instance.AddItem(itemData);
                //팝업용
                itemDatas.Enqueue(itemData);
            }

            //count 만큼 경험치 증가
            GameManager.Instance.SummonManager.AddExp(category, count);
            //뽑기 정보 저장
            GameManager.Instance.statSaver.SaveSummonProgress(GameManager.Instance.SummonManager.GetSummonProgressData());
            //인벤토리 정보 저장
            GameManager.Instance.statSaver.SaveInventoryData(InventoryManager.Instance.GetSaveData());
            //재화 정보 저장
            GameManager.Instance.statSaver.SavePlayerProgressData(GameManager.Instance.stats.GetProgressSaveData());
            //스킬 뽑기일 경우 스킬 정보 저장
            if (category == SummonSubCategory.Skill)
            {
                GameManager.Instance.statSaver.SavePlayerSkillData(SkillManager.Instance.BuildSaveData());
            }

            //UIInventoryPage, SkillPage의 RefreshAll 실행시켜 줘야함
            //UIManager.Instance.TryGetPage<UISkillPage>().Refresh();
            if (UIManager.Instance.TryGetPage<UISkillPage>(out UISkillPage skillPage))
            {
                skillPage.RefreshAll();
            }

            if (UIManager.Instance.TryGetPage<UIInventoryPage>(out UIInventoryPage inventoryPage))
            {
                inventoryPage.RefreshAll();
            }

            UIManager.Instance.PopupOpen<UISummonResultPopup>().StartDisplayingResult(itemDatas);
    }
        else
        {
            Debug.Log($"[UISummonPanel] 다이아가 부족함, {GameManager.Instance.stats.Diamond}");
        }
    }
}

//엄
//생각을 해봅시다
//카테고리 누르면 그 카테고리에 맞게 새로고침 해줘야 함
//새로고침 할때 현재 소환 레벨이나, 경험치 같은것도 초기화 해줘야 함.
//그럼 Enum에서 서브카테고리하나 만들고,
//Refresh에서는 그 enum을 인자로 받아서 초기화 해주면 될 거 같음
//사실 Refresh만 있고, Init이 꼭 있어야 하나 싶기도 함
//첫 실행시 Refresh(SubCategory.Weapon) 해주고,
//버튼 누를 때마다 그 카테고리 인자 넣어서 초기화 해주면 될거같은데

//현재 어떤 카테고리인지 enum필드를 하나 선언 및 Init때 초기화 하고
//뽑기버튼 누르면 그 카테고리에서 AddItem같은걸 하면 되지 않을까
//현재 카테고리가 뭐 무기면 무기뽑기 이런식으로
//결국 if문 내지는 switch문으로 쓰겠다는거 아님?
//그래도 버튼마다 함수 다 따로 만드는거보다 나은건가?

//그럼 데이터 보관은
//서브카테고리 enum이 있으니
//데이터매니저에서 csv 읽어와서
//딕셔너리<서브카테고리, int[]>에 넣고?
//배열? int배열?
//소환레벨이니까 사실 크게 올라가진 않을거임 대부분의 키우기류 게임이 만렙이 1000넘어가지는 않잖음
//그럼 뭐 따로 테이블 안쓰고 헤더 한 줄에서 세미콜론으로 10;20;30 이런 식으로 나누고 배열이 안되네 리스트에 넣으면?
//아니면 exp테이블이 int, float 딕셔너리임 레벨, 필요한 경험치 량
//그럼 이중 딕셔너리 써야하나
//딕셔너리 <서브카테고리, 딕셔너리<int, int>> 소환레벨 데이터
//어 복잡한데
//소규모니까 이중 딕셔너리 대신 그러니까 만렙을 3렙까지만 할꺼니까
//일단은 <서브카테고리, List<int>>로 간단하게 해보고
//만약 추가된다면 딕셔너리안에 딕셔너리로 리팩토링을 하던 하면 될 거 같음

//데이터 보관 얘기하다가 왜 얘기가 저렇게 된거지
//데이터 보관은 MonoBehaviour를 상속받지 않는 순수 C#클래스 하나 작성 
//UI를 따로 건드리는것도 아니고, 생명주기도 필요 없음
//그냥 특정 소환의 레벨이 몇이냐, 경험치가 얼마냐, 이런것만 보관하고
//첫 실행시 GameManager가 로드 요청, 변동이 있을때 GameManager에 저장 요청만 하면 될듯??
//근데 이게 됨?

//뽑기용 확률표 작성도 해야함
//??
//딕셔너리<카테고리, 딕셔너리<레벨, 리스트<경험치>>>?????????????
//Dictionry<SummonSubCategory, Dictionary<int, List<float>>>?
//            소환종류                    레벨       경험치
//차라리 클래스로 하는건
//public class Test
//{
//    public SummonSubCategory Category;
//    public Dictionary<int, float> ExpTable;
//}