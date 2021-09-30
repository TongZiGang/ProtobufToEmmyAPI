---@class ActivityModule 
---@field type number 类型
---@field activityId number 
---@field task TaskActivityModule 
ActivityModule  = { }

---@class ActivityData 
---@field tasks TaskActivityData 任务活动
ActivityData  = { }

---@class TaskActivity 
---@field id number 条目ID
---@field curProgress number 当前进度
---@field totalProgress number 总进度
TaskActivity  = { }

---@class TaskActivityModule 
---@field receivedIds number[] 已领取的奖励ID集合
---@field tasks TaskActivity[] 任务进度
---@field receivedTaskIds number[] 已接取任务ID集合
TaskActivityModule  = { }

---@class TaskActivityItem 
---@field id number 
---@field goalId number 目标ID
---@field rewards PbReward[] 常规奖励
---@field vipLimit number vip等级限制
---@field jump string 跳转数据
---@field isSelectReward boolean 是否选择奖励
---@field resetFrequency number 重置频率，1每日，2每周，3每月
TaskActivityItem  = { }

---@class TaskActivityData 
---@field items TaskActivityItem[] 
TaskActivityData  = { }

local data = {}
