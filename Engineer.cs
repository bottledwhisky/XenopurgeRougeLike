using SpaceCommander;
using System;
using System.Collections.Generic;

namespace XenopurgeRougeLike
{
    // WIP: Engineer path not yet implemented
    // 工程师
    // 特色：强化地雷，手雷，炮台
    // 路径天赋：
    //     2：手雷伤害提升，闪光弹效果提升，炮台准确率提升
    //     4：手雷伤害提升，闪光弹效果提升，炮台准确率提升，单位拥有护甲时免疫敌方所有负面效果
    //     6：手雷伤害提升，闪光弹效果提升，炮台准确率提升，单位拥有护甲时受到的所有伤害减少50%
    // 普通：
    //     炮台弹药量+50%/100%
    //     地雷部署时间加快（可叠加3）
    //     地雷，手雷，闪光弹友军惩罚-50%（可叠加2）
    // 精锐：
    //     炮台免疫远程伤害
    //     炮台被摧毁后可以重新部署（修理是个分开的指令，花费时间更长）
    //     手雷，闪光弹使用次数+1
    //     地雷引爆次数+1
    // 专家：
    //     手雷，闪光弹范围加1
    //     炮台增加一次使用次数
    //     炮台升级为minigun
    public class Engineer
    {
        public static List<CompanyAffinity> _affinities;
        public static List<CompanyAffinity> Affinities => _affinities ??=
        [
            // TODO: Define engineer affinities
        ];

        public static Dictionary<Type, Reinforcement> _reinforcements;
        public static Dictionary<Type, Reinforcement> Reinforcements => _reinforcements ??= new()
        {
            // TODO: Register engineer reinforcements here
        };

        public static bool IsAvailable()
        {
            // TODO: Define availability conditions
            return true;
        }
    }
}
