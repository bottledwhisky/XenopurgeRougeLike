using System.Collections.Generic;
using System.Linq;

namespace XenopurgeRougeLike
{
    public static partial class I18nData
    {
        //English (en)
        //Greek(el)
        //French(fr)
        //Chinese(Simplified) (zh)
        //Chinese(Traditional) (zh-TW)
        //German(de)
        //Polish(pl)
        //Portuguese(pt)
        //Russian(ru)
        //Spanish(es)
        //Ukrainian(uk)
        //Japanese(ja)
        //Korean(ko)

        // This dictionary holds [string_id] => [language_code] => [translation]
        public static Dictionary<string, Dictionary<string, string>> _translations =
            new Dictionary<string, Dictionary<string, string>>()
                .Concat(GetAffinityTranslations())
                .Concat(GetCommonTranslations())
                .Concat(GetCompanyTranslations())
                .Concat(GetEngineerTranslations())
                .Concat(GetGunslingerTranslations())
                .Concat(GetRockstarTranslations())
                .Concat(GetScavengerTranslations())
                .Concat(GetSupportTranslations())
                .Concat(GetSyntheticsTranslations())
                .Concat(GetUiTranslations())
                .Concat(GetWarriorTranslations())
                .Concat(GetXenoTranslations())
                .ToDictionary(x => x.Key, x => x.Value);
    }
}
