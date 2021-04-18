//列舉值類別集合

//圖格圖片種類
public enum ElementImageType
{
    雞蛋, 羽毛, 小雞, 告示牌, 卡通雞, 公雞, 彩虹雞, 烤雞大餐
}

//圖格性質
public enum ElementProperty
{
    一般符號, 狂野符號
}

//中獎線等級方塊顏色變化
public enum LineLevelBlockStyle
{
    激活顏色, 未激活顏色, 未解鎖顏色
}

//窗格方向
public enum WindowSide
{
    左側, 右側
}

//支付表組合分類方式
public enum CombinationSortType
{
    純元素與否, 同花色與否, 元素數量, 總獎金, 排序相似與否
}

//圖格動畫
public enum ElementAnimationType
{
    全部動畫停止, 中獎
}

//連線獎項性質
public enum PrizeLineType
{
    小獎, 大獎
}

//粒子特效種類
public enum ParticleEffectType
{
    金幣收集, 選擇等級, 中獎圖格, 金雞蛋, 滑鼠點擊
}