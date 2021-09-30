MessageEnum = {}
---@class BattleStatus   
MessageEnum.BattleStatus  = {
    NO_START = 1,                            --未开启
    BATTLE = 2,                              --战斗状态，允许玩家自由争夺位置
    SETTLEMENT = 3,                          --结算状态，做一系列结算操作，不允许玩家战斗
    RESET = 4,                               --重置状态
    SLEEP = 5,                               --休眠状态，只有控制器能唤醒
    CHANGE = 6,                              --切换状态
}
