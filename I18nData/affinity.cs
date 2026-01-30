using System.Collections.Generic;

namespace XenopurgeRougeLike
{
    public static partial class I18nData
    {
        // affinity strings
        private static Dictionary<string, Dictionary<string, string>> GetAffinityTranslations()
        {
            return new Dictionary<string, Dictionary<string, string>>
            {
                ["affinity.help"] = new Dictionary<string, string>
                {
                    ["en"] = "As a token of trust, each company can provide unique support to your missions based on the number of distinct reinforcements you accepted from them.",
                    ["el"] = "Ως ένδειξη εμπιστοσύνης, κάθε εταιρεία μπορεί να παρέχει μοναδική υποστήριξη στις αποστολές σας με βάση τον αριθμό των διαφορετικών ενισχύσεων που δεχθήκατε από αυτές.",
                    ["fr"] = "En signe de confiance, chaque entreprise peut fournir un soutien unique à vos missions en fonction du nombre de renforts distincts que vous avez acceptés d'eux.",
                    ["zh"] = "作为信任的象征，每家公司可以根据您接受的不同增援数量为您的任务提供独特的支持。",
                    ["zh-TW"] = "作為信任的象徵，每家公司可以根據您接受的不同增援數量為您的任務提供獨特的支持。",
                    ["de"] = "Als Zeichen des Vertrauens kann jedes Unternehmen einzigartige Unterstützung für Ihre Missionen bieten, basierend auf der Anzahl der unterschiedlichen Verstärkungen, die Sie von ihnen akzeptiert haben.",
                    ["pl"] = "W geście zaufania każda firma może zapewnić unikalne wsparcie dla twoich misji na podstawie liczby różnych wzmocnień, które od nich przyjąłeś.",
                    ["pt"] = "Como sinal de confiança, cada empresa pode fornecer suporte único às suas missões com base no número de reforços distintos que você aceitou delas.",
                    ["ru"] = "В знак доверия каждая компания может предоставить уникальную поддержку вашим миссиям на основе количества различных подкреплений, которые вы приняли от них.",
                    ["es"] = "Como señal de confianza, cada empresa puede brindar apoyo único a tus misiones según la cantidad de refuerzos distintos que hayas aceptado de ellos.",
                    ["uk"] = "Як знак довіри, кожна компанія може надати унікальну підтримку вашим місіям на основі кількості різних підкріплень, які ви прийняли від них.",
                    ["ja"] = "信頼の証として、各企業はあなたが受け入れた異なる増援の数に基づいて、ミッションに独自のサポートを提供できます。",
                    ["ko"] = "신뢰의 표시로, 각 회사는 당신이 받아들인 서로 다른 증원 수에 따라 임무에 고유한 지원을 제공할 수 있습니다.",
                },
                ["affinity.unlock_description"] = new Dictionary<string, string>
                {
                    ["en"] = "Unlocked after acquiring {0} reinforcements:\n{1}",
                    ["el"] = "Ξεκλειδώνεται αφού αποκτήσετε {0} ενισχύσεις:\n{1}",
                    ["fr"] = "Débloqué après avoir acquis {0} renforts :\n{1}",
                    ["zh"] = "获得 {0} 个增援后解锁：\n{1}",
                    ["zh-TW"] = "獲得 {0} 個增援後解鎖：\n{1}",
                    ["de"] = "Freigeschaltet nach Erhalt von {0} Verstärkungen:\n{1}",
                    ["pl"] = "Odblokowane po zdobyciu {0} wzmocnień:\n{1}",
                    ["pt"] = "Desbloqueado após adquirir {0} reforços:\n{1}",
                    ["ru"] = "Разблокировано после получения {0} подкреплений:\n{1}",
                    ["es"] = "Desbloqueado después de adquirir {0} refuerzos:\n{1}",
                    ["uk"] = "Розблоковано після отримання {0} підкріплень:\n{1}",
                    ["ja"] = "{0}個の増援を獲得後にアンロック：\n{1}",
                    ["ko"] = "{0}개의 증원 획득 후 잠금 해제:\n{1}",
                },
                // Wayland-Yutani (Synthetics)
            };
        }
    }
}
