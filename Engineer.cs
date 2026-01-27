using SpaceCommander;
using System;
using System.Collections.Generic;
using XenopurgeRougeLike.EngineerReinforcements;

namespace XenopurgeRougeLike
{
    // 工程师
    // 特色：强化地雷，手雷，炮台
    // 路径天赋：
    //     2：地雷，手雷伤害+25%，闪光弹效果持续时间+50%，指令商店出现地雷，手雷，闪光弹，炮台的概率提升
    //     4：地雷，手雷伤害+50%，闪光弹效果持续时间+100%，指令商店出现地雷，手雷，闪光弹，炮台的概率提升，同流派增援获得概率提升
    //     6：地雷，手雷伤害+100%，闪光弹效果持续时间+100%，指令商店出现地雷，手雷，闪光弹，炮台的概率提升，同流派增援获得概率提升，地雷，手雷，闪光弹使用次数+1
    // 普通：
    //     高级弹鼓：炮台弹药量+50%/100%（可叠加2），炮台准确率+15
    //     蜘蛛雷：地雷部署时间加快（可叠加2）
    //     塑形炸药：地雷，手雷，闪光弹友军惩罚-50%/-100%（可叠加2）
    // 精锐：
    //     重型炮台：炮台免疫远程伤害，炮台准确率+15
    //     模块化设计：炮台被摧毁后可以在原地重新部署（修理是个分开的指令，花费时间更长）
    //     战术腰带：地雷，手雷，闪光弹使用次数+1
    // 专家：
    //     大爆炸：地雷，手雷，闪光弹范围加1
    //     碳纤维支架：可额外携带一架炮台进入任务
    //     炮台升级为SIN转轮机枪，炮台准确率+15
    public class Engineer
    {
        public static List<CompanyAffinity> _affinities;
        public static List<CompanyAffinity> Affinities => _affinities ??=
        [
            EngineerAffinity2.Instance,
            // TODO: EngineerAffinity4.Instance,
            // TODO: EngineerAffinity6.Instance,
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
