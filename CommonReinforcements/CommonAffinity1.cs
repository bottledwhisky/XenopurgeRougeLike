using static XenopurgeRougeLike.ModLocalization;

namespace XenopurgeRougeLike.CommonReinforcements
{
    // 通用路径分隔符，没有实际效果，只是为了显示
    public class CommonAffinity1 : CompanyAffinity
    {
        public CommonAffinity1()
        {
            unlockLevel = 1;
            company = Company.Common;
            description = L("common.affinity1.description");
        }

        public static CommonAffinity1 _instance;
        public static CommonAffinity1 Instance => _instance ??= new();
    }
}
