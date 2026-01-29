using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XenopurgeRougeLike
{
    public class I18nData
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

        // This dictionary holds [language_code] => [string_id] => [translation]
        public static Dictionary<string, Dictionary<string, string>> _translations = new Dictionary<string, Dictionary<string, string>>
        {
        };
    }
}