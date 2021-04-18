//拉霸模擬測試單元
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestSimulationUnit : MonoBehaviour
{
    [Header("遊戲進行狀態")]
    public int unitLevel; //等級
    public int blankTimes; //落空次數
    public int littlePrizeTimes; //小獎次數
    public int bigPrizeTimes; //大獎次數
    public int gainTimes; //獲利>投注次數
    public int lossTimes; //獲利≦投注次數
    public int simulationTotalPrize; //測試程序中累計中獎獎金
    public int betCost; //下注金額

    public static int s_totalTimes; //總模擬次數

    [Header("參考物件")]
    public Text levelTxt; //等級
    public Text betCostTxt; //下注金額
    public Text blankProbabilityTxt; //落空次數&機率
    public Text littlePrizeProbabilityTxt; //小獎次數&機率
    public Text bigPrizeProbabilityTxt; //大獎次數&機率
    public Text totalTimesTxt; //總次數
    public Text gainProbabilityTxt; //獲利>投注 次數&機率
    public Text lossProbabilityTxt; //獲利≦投注 次數&機率
    public Text averageGainTxt; //平均獲利
    public Text rorTxt; //投資報酬率

    //---------------------------------------------------------------------------------------------------------------------------------------------

    //初始化
    //[param] level = 等級 , betCost = 下注金額
    public void Initialize(int level, int cost)
    {
        unitLevel = level; //設定等級
        levelTxt.text = level.ToString(); //等級顯示

        betCost = cost; //設定下注金額
        betCostTxt.text = betCost.ToString(); //下注金額顯示

        if (!gameObject.activeSelf) gameObject.SetActive(true); //顯示物件

        ResetParameter(); //重設參數
    }

    //重設參數
    public void ResetParameter()
    {
        blankTimes = 0; //落空次數歸零
        blankProbabilityTxt.text = "0(0.0%)"; //落空次數&機率

        littlePrizeTimes = 0; //小獎次數歸零
        littlePrizeProbabilityTxt.text = "0(0.0%)"; //小獎次數&機率

        bigPrizeTimes = 0; //大獎次數歸零
        bigPrizeProbabilityTxt.text = "0(0.0%)"; //大獎次數&機率

        totalTimesTxt.text = "0"; //總次數

        gainTimes = 0; //獲利>投注 次數歸零
        gainProbabilityTxt.text = "0(0.0%)"; //獲利>投注 次數&機率

        lossTimes = 0; //獲利≦投注 次數歸零
        lossProbabilityTxt.text = "0(0.0%)"; //獲利≦投注 次數&機率

        simulationTotalPrize = 0; //總獲利歸零

        averageGainTxt.text = "0"; //平均獲利
        rorTxt.text = "0.0"; //投資報酬率
    }

    //無效化
    public void Invalid()
    {
        this.gameObject.SetActive(false);
        unitLevel = -1;
    }

    //拉霸結果數據轉換
    public void TransferSlotData(List<PrizeLineInfo> lines)
    {
        int _blank = 0; //落空次數
        int _little = 0; //小獎次數
        int _big = 0; //大獎次數
        int _sumPrize = 0; //中獎金額

        for (int i = 0; i < lines.Count; i++)
        {
            _sumPrize += lines[i].sumPrize; //該線段的中獎獎金

            for (int j = 0; j < lines[i].PrizeItemList.Count; j++)
            {
                if (lines[i].PrizeItemList[j].prizeType == PrizeLineType.小獎) _little += 1; //累計小獎次數
                else if (lines[i].PrizeItemList[j].prizeType == PrizeLineType.大獎) _big += 1; //累計大獎次數
            }
        }

        if (_sumPrize == 0) _blank += 1; //若該等級所有線段中獎累計金額為0則累加至落空次數
        else simulationTotalPrize += _sumPrize; //累計測試程序中的中獎獎金

        //Debug.Log("等級 "+ unitLevel + " SumPrize = " + _sumPrize);

        System.Func<int, float> PercentageCalculate = (x) => //計算次數占總次數的百分比
        {
            if (x == 0) return 0f;

            float _p = Mathf.Clamp(( (float)x / s_totalTimes ) * 100, 0, 100);

            return _p;
        };

        //顯示結果
        totalTimesTxt.text = s_totalTimes.ToString(); //總次數

        blankTimes += _blank;
        blankProbabilityTxt.text = string.Format("{0}({1}%)", blankTimes, PercentageCalculate(blankTimes).ToString("0.0")); //落空次數&機率

        littlePrizeTimes += _little;
        littlePrizeProbabilityTxt.text = string.Format("{0}({1}%)", littlePrizeTimes, PercentageCalculate(littlePrizeTimes).ToString("0.0")); //小獎次數&機率

        bigPrizeTimes += _big;
        bigPrizeProbabilityTxt.text = string.Format("{0}({1}%)", bigPrizeTimes, PercentageCalculate(bigPrizeTimes).ToString("0.0")); //大獎次數&機率

        if (_sumPrize > betCost) //獲利>投注 次數&機率
        {
            gainTimes += 1;
            gainProbabilityTxt.text = string.Format("{0}({1}%)", gainTimes, PercentageCalculate(gainTimes).ToString("0.0"));
        }
        else //獲利≦投注 次數&機率
        {
            lossTimes += 1;
            lossProbabilityTxt.text = string.Format("{0}({1}%)", lossTimes, PercentageCalculate(lossTimes).ToString("0.0"));
        }

        float _averageGain = simulationTotalPrize == 0 ? 0 : (float)simulationTotalPrize / s_totalTimes;
        averageGainTxt.text = _averageGain.ToString("0"); //平均獲利

        float _ror = (float)simulationTotalPrize / ( betCost * s_totalTimes );
        rorTxt.text = _ror.ToString("0.0"); //投資報酬率
    }
}
