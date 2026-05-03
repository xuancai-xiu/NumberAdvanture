using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Level1Manager : MonoBehaviour
{
    [Header("Level Setup")]
    [SerializeField] private Transform spawnPoint;        // 玩家出生点
    [SerializeField] private Transform goalPoint;         // 终点（未使用）
    [SerializeField] private float fallDeathHeight = -10f;
    [SerializeField] private Player player;               // 玩家引用
    [SerializeField] private int levelIndex = 1;          // 当前关卡编号

    [System.Serializable]
    public struct FruitDataEntry
    {
        public FruitDefinition fruitDef;   // 水果定义（名称、图标、动态数值）
        public GameObject prefab;          // 对应的水果预制体（自带精灵和动画）
    }

    [Header("Fruit Spawning & Data")]
    [SerializeField] private FruitDataEntry[] fruitData;   // 所有水果类型的数据
    [SerializeField] private Transform[] spawnPoints;      // 场景中水果生成点

    [Header("UI References")]
    [SerializeField] private TMP_Text levelLabelText;
    [SerializeField] private TMP_Text formulaTitleText;
    [SerializeField] private RectTransform fruitLegendContainer;
    [SerializeField] private RectTransform formulaEntriesContainer;
    [SerializeField] private TMP_Text errorMessageText;

    [Header("Fruit Legend Layout")]
    [SerializeField] private Vector2 fruitLegendItemSize = new Vector2(200, 200f);
    [SerializeField] private Vector2 fruitIconSize = new Vector2(160f, 160f);
    [SerializeField] private float fruitValueFontSize = 60f;

    [Header("Formula Layout")]
    [SerializeField] private Vector2 formulaItemSize = new Vector2(800f, 100f);
    [SerializeField] private float formulaFontSize = 60f;

    [Header("Formula Settings")]
    [SerializeField] private int maxOperandValue = 5;      // 水果数值上限（1~maxOperandValue）
    [SerializeField] private int maxFormulaCount = 1;      // 最大公式数量（实际数量受关卡等级影响）

    private bool levelComplete = false;
    private readonly List<FormulaData> formulas = new List<FormulaData>();   // 当前关卡的所有公式

    public Transform SpawnPoint => spawnPoint;

    private void Start()
    {
        // 1. 为每种水果分配互不重复的动态数值
        AssignDistinctFruitValues();

        // 2. 生成关卡 UI（图例、公式）
        SetupLevelUI(levelIndex);

        // 3. 生成场景中的水果实例（确保每个公式的正确答案至少出现一次）
        SpawnFruits();

        // 4. 放置玩家
        if (spawnPoint != null && player != null)
            player.transform.position = spawnPoint.position;
    }

    private void Update()
    {
        if (player.transform.position.y < fallDeathHeight)
        {
            player.Die();
            RestartLevel();
        }
        if (Input.GetKeyDown(KeyCode.R))
            RestartLevel();
        if (Input.GetKeyDown(KeyCode.Escape))
            BackToLevelSelect();
    }

    // ==================== 水果数值分配（互不重复） ====================
    private void AssignDistinctFruitValues()
    {
        int fruitCount = fruitData.Length;
        if (fruitCount > maxOperandValue)
        {
            Debug.LogWarning($"水果种类数量({fruitCount})超过数值范围(1~{maxOperandValue})，数值将被迫重复。建议增加 maxOperandValue 或减少水果种类。");
        }

        // 创建候选数值列表 1..maxOperandValue
        List<int> availableValues = new List<int>();
        for (int i = 1; i <= maxOperandValue; i++)
            availableValues.Add(i);

        // 随机打乱候选列表
        for (int i = 0; i < availableValues.Count; i++)
        {
            int rand = Random.Range(i, availableValues.Count);
            int temp = availableValues[i];
            availableValues[i] = availableValues[rand];
            availableValues[rand] = temp;
        }

        // 为每个水果分配数值（循环使用候选列表，超出范围则重复）
        for (int i = 0; i < fruitCount; i++)
        {
            int valueIndex = i % availableValues.Count;
            fruitData[i].fruitDef.dynamicValue = availableValues[valueIndex];
        }
    }

    // ==================== UI 构建 ====================
    private void SetupLevelUI(int level)
    {
        if (levelLabelText != null)
            levelLabelText.text = "Level: " + level;

        if (formulaTitleText != null)
            formulaTitleText.text = "Fruit value & formulas";

        errorMessageText.text = "";

        GenerateFruitLegend();          // 生成图例（显示每种水果的数值）
        GenerateFormulaEntries(level);  // 生成算式
    }

    private void GenerateFruitLegend()
    {
        foreach (var entry in fruitData)
        {
            FruitDefinition fruit = entry.fruitDef;

            GameObject legendItem = new GameObject(fruit.name + " Legend", typeof(RectTransform));
            legendItem.transform.SetParent(fruitLegendContainer, false);
            legendItem.GetComponent<RectTransform>().sizeDelta = fruitLegendItemSize;

            HorizontalLayoutGroup layout = legendItem.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 5;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // 水果图标
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(legendItem.transform, false);
            LayoutElement iconLayout = iconObj.AddComponent<LayoutElement>();
            iconLayout.preferredWidth = fruitIconSize.x;
            iconLayout.preferredHeight = fruitIconSize.y;
            Image icon = iconObj.GetComponent<Image>();
            icon.sprite = fruit.icon;
            icon.preserveAspect = true;

            // 水果数值文本
            GameObject valueObj = new GameObject("Value", typeof(RectTransform));
            valueObj.transform.SetParent(legendItem.transform, false);
            LayoutElement valueLayout = valueObj.AddComponent<LayoutElement>();
            valueLayout.preferredWidth = 40;
            valueLayout.preferredHeight = 200;
            TextMeshProUGUI valueText = valueObj.AddComponent<TextMeshProUGUI>();
            valueText.text = fruit.dynamicValue.ToString();
            valueText.fontSize = fruitValueFontSize;
            valueText.alignment = TextAlignmentOptions.Center;
            valueText.color = Color.white;
        }
    }

    // 生成公式（保证每个公式的 targetValue 等于某个水果的数值）
    private void GenerateFormulaEntries(int level)
    {
        // 根据关卡等级计算本关公式数量
        int formulaCount = maxFormulaCount;

        // 收集所有水果的数值（用于随机选取作为未知数）
        List<int> fruitValues = new List<int>();
        foreach (var entry in fruitData)
            fruitValues.Add(entry.fruitDef.dynamicValue);

        for (int i = 0; i < formulaCount; i++)
        {
            // 随机选择一个水果数值作为未知数的值（即需要收集的数值）
            int targetValue = fruitValues[Random.Range(0, fruitValues.Count)];

            // 随机决定未知数在左侧还是右侧
            bool unknownIsLeft = Random.value < 0.5f;

            int leftValue, rightValue, result;
            if (unknownIsLeft)
            {
                // 未知在左侧，右侧是1~9的随机数
                leftValue = targetValue;
                rightValue = Random.Range(1, 10);
                result = leftValue * rightValue;
            }
            else
            {
                // 未知在右侧，左侧是1~9的随机数
                leftValue = Random.Range(1, 10);
                rightValue = targetValue;
                result = leftValue * rightValue;
            }

            FormulaData formula = new FormulaData(
                left: leftValue,
                right: rightValue,
                op: Operator.Multiply,
                result: result,
                unknownSide: unknownIsLeft ? UnknownSide.Left : UnknownSide.Right,
                targetValue: targetValue
            );
            formulas.Add(formula);

            // 创建该算式的 UI 文本
            GameObject formulaItem = new GameObject("Formula " + (i + 1), typeof(RectTransform));
            formulaItem.transform.SetParent(formulaEntriesContainer, false);
            TextMeshProUGUI formulaText = formulaItem.AddComponent<TextMeshProUGUI>();
            formulaText.fontSize = formulaFontSize;
            formulaText.alignment = TextAlignmentOptions.Center;
            formulaText.color = Color.white;
            formula.uiText = formulaText;
            LayoutElement layout = formulaItem.AddComponent<LayoutElement>();
            layout.preferredWidth = formulaItemSize.x;
            layout.preferredHeight = formulaItemSize.y;

            UpdateFormulaDisplay(formula);
        }
    }

    // 更新单个算式的显示文本和颜色
    private void UpdateFormulaDisplay(FormulaData formula)
    {
        if (formula.uiText != null)
        {
            formula.uiText.text = GetFormulaDisplayText(formula);
            formula.uiText.color = formula.error ? Color.red :
                                   formula.completed ? Color.green : Color.white;
        }
    }

    private string GetFormulaDisplayText(FormulaData formula)
    {
        string op = formula.op == Operator.Multiply ? "×" : "÷";
        string leftStr, rightStr;

        if (formula.error)
        {
            // 错误时显示收集到的错误数值
            if (formula.unknownSide == UnknownSide.Left)
            {
                leftStr = formula.collectedValue.ToString();
                rightStr = formula.right.ToString();
            }
            else
            {
                leftStr = formula.left.ToString();
                rightStr = formula.collectedValue.ToString();
            }
        }
        else if (formula.completed)
        {
            // 完成后显示完整算式
            leftStr = formula.left.ToString();
            rightStr = formula.right.ToString();
        }
        else
        {
            // 未完成时未知数显示为问号
            if (formula.unknownSide == UnknownSide.Left)
            {
                leftStr = "?";
                rightStr = formula.right.ToString();
            }
            else
            {
                leftStr = formula.left.ToString();
                rightStr = "?";
            }
        }
        return $"{leftStr} {op} {rightStr} = {formula.result}";
    }

    // ==================== 水果生成（保证每个公式的正确答案至少有一个） ====================
    private void SpawnFruits()
    {
        if (fruitData == null || fruitData.Length == 0) return;
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        // 建立数值 -> 对应水果预制体列表的映射
        Dictionary<int, List<GameObject>> valueToPrefabs = new Dictionary<int, List<GameObject>>();
        foreach (var entry in fruitData)
        {
            int val = entry.fruitDef.dynamicValue;
            if (!valueToPrefabs.ContainsKey(val))
                valueToPrefabs[val] = new List<GameObject>();
            valueToPrefabs[val].Add(entry.prefab);
        }

        // 收集所有公式需要的目标数值
        List<int> requiredValues = new List<int>();
        foreach (FormulaData formula in formulas)
        {
            requiredValues.Add(formula.targetValue);
        }

        // 生成计划列表：存放要生成的 (预制体, 数值)
        List<(GameObject prefab, int value)> spawnPlan = new List<(GameObject, int)>();

        // 1. 为每个必需的数值生成一个水果（
        foreach (int reqVal in requiredValues)
        {
            if (valueToPrefabs.TryGetValue(reqVal, out var prefabs) && prefabs.Count > 0)
            {
                GameObject chosenPrefab = prefabs[0];
                spawnPlan.Add((chosenPrefab, reqVal));
            }
            else
            {
                Debug.LogError($"严重错误：数值 {reqVal} 是公式所需，但没有水果拥有此数值！请检查水果数值分配逻辑。");
            }
        }

        // 2. 准备干扰项候选池
        HashSet<int> requiredSet = new HashSet<int>(requiredValues); 
        List<FruitDataEntry> decoyCandidates = new List<FruitDataEntry>();
        foreach (var entry in fruitData)
        {
            if (!requiredSet.Contains(entry.fruitDef.dynamicValue))
                decoyCandidates.Add(entry);
        }

        // 3. 剩余生成点数量
        int remaining = spawnPoints.Length - spawnPlan.Count;
        for (int i = 0; i < remaining; i++)
        {
            var randomEntry = decoyCandidates[Random.Range(0, decoyCandidates.Count)];
            spawnPlan.Add((randomEntry.prefab, randomEntry.fruitDef.dynamicValue));
        }

        // 打乱生成顺序，让正确答案和干扰项混在一起
        //for (int i = 0; i < spawnPlan.Count; i++)
        //{
        //    int rand = Random.Range(i, spawnPlan.Count);
        //    var temp = spawnPlan[i];
        //    spawnPlan[i] = spawnPlan[rand];
        //    spawnPlan[rand] = temp;
        //}

        // 实例化水果
        for (int i = 0; i < spawnPoints.Length && i < spawnPlan.Count; i++)
        {
            Transform spawnPos = spawnPoints[i];
            var (prefab, value) = spawnPlan[i];
            if (prefab == null) continue;

            GameObject newFruit = Instantiate(prefab, spawnPos.position, Quaternion.identity);
            Fruit fruitComp = newFruit.GetComponent<Fruit>();
            if (fruitComp != null)
                fruitComp.Initialize(value);
            else
                Debug.LogError("水果预制体缺少 Fruit 脚本！");
        }
    }

    // ==================== 收集逻辑 ====================
    public void OnFruitCollected(int value)
    {
        // 找到第一个未完成且未出错的公式
        FormulaData currentFormula = null;
        foreach (FormulaData f in formulas)
        {
            if (!f.completed && !f.error)
            {
                currentFormula = f;
                break;
            }
        }

        if (currentFormula == null)
        {
            Debug.Log("没有活跃的公式需要验证，忽略收集的水果");
            return;
        }

        if (value == currentFormula.targetValue)
        {
            // 收集正确
            currentFormula.completed = true;
            UpdateFormulaDisplay(currentFormula);
            Debug.Log($"公式完成：{currentFormula.left} × {currentFormula.right} = {currentFormula.result}");
        }
        else
        {
            // 收集错误，关卡失败
            currentFormula.collectedValue = value;
            currentFormula.error = true;
            UpdateFormulaDisplay(currentFormula);
            if (errorMessageText != null)
                errorMessageText.text = "Wrong fruit!";
            Invoke(nameof(RestartLevel), 2f);
        }
    }

    public void TryCompleteLevel()
    {
        if (AreAllFormulasComplete())
        {
            LevelComplete();
        }
        else
        {
            if (errorMessageText != null)
                errorMessageText.text = "Formulas not finished!";
            CancelInvoke(nameof(ClearErrorMessage));
            Invoke(nameof(ClearErrorMessage), 2f);
        }
    }

    private void ClearErrorMessage()
    {
        if (errorMessageText != null)
            errorMessageText.text = "";
    }

    private bool AreAllFormulasComplete()
    {
        foreach (FormulaData f in formulas)
            if (!f.completed) return false;
        return true;
    }

    public void LevelComplete()
    {
        if (!levelComplete)
        {
            levelComplete = true;
            Debug.Log("Level Complete!");
            CancelInvoke();
            if (errorMessageText != null)
                errorMessageText.text = "Level Complete!";
            Invoke(nameof(GoToLevelSelect), 2f);
        }
    }

    public void RestartLevel() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    private void GoToLevelSelect() => SceneManager.LoadScene("LevelSelect");

    public void BackToLevelSelect()
    {
        SceneManager.LoadScene("LevelSelect");
    }

}

// ==================== 数据定义 ====================
[System.Serializable]
public class FruitDefinition
{
    public string name;
    public Sprite icon;
    [HideInInspector] public int dynamicValue;      // 运行时分配的数值
}

public enum Operator { Multiply, Divide }
public enum UnknownSide { Left, Right }

public class FormulaData
{
    public int left, right, result;
    public Operator op;
    public UnknownSide unknownSide;
    public int targetValue;        // 需要玩家收集的水果数值
    public int collectedValue;     // 玩家实际收集的数值（错误时记录）
    public bool completed;
    public bool error;
    public TMP_Text uiText;

    public FormulaData(int left, int right, Operator op, int result, UnknownSide unknownSide, int targetValue)
    {
        this.left = left;
        this.right = right;
        this.op = op;
        this.result = result;
        this.unknownSide = unknownSide;
        this.targetValue = targetValue;
        completed = false;
        error = false;
        uiText = null;
    }
}