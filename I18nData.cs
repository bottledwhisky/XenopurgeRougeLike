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

        // This dictionary holds [string_id] => [language_code] => [translation]
        public static Dictionary<string, Dictionary<string, string>> _translations = new Dictionary<string, Dictionary<string, string>>
        {
            // UI strings
            ["ui.coins_display"] = new Dictionary<string, string>
            {
                ["en"] = "<sprite name=\"CoinIcon\">: {0}",
                ["el"] = "<sprite name=\"CoinIcon\">: {0}",
                ["fr"] = "<sprite name=\"CoinIcon\">: {0}",
                ["zh"] = "<sprite name=\"CoinIcon\">: {0}",
                ["zh-TW"] = "<sprite name=\"CoinIcon\">: {0}",
                ["de"] = "<sprite name=\"CoinIcon\">: {0}",
                ["pl"] = "<sprite name=\"CoinIcon\">: {0}",
                ["pt"] = "<sprite name=\"CoinIcon\">: {0}",
                ["ru"] = "<sprite name=\"CoinIcon\">: {0}",
                ["es"] = "<sprite name=\"CoinIcon\">: {0}",
                ["uk"] = "<sprite name=\"CoinIcon\">: {0}",
                ["ja"] = "<sprite name=\"CoinIcon\">: {0}",
                ["ko"] = "<sprite name=\"CoinIcon\">: {0}",
            },
            ["ui.choose_reinforcement"] = new Dictionary<string, string>
            {
                ["en"] = "CHOOSE YOUR REINFORCEMENT",
                ["el"] = "ΕΠΙΛΕΞΤΕ ΤΗΝ ΕΝΙΣΧΥΣΗ ΣΑΣ",
                ["fr"] = "CHOISISSEZ VOTRE RENFORT",
                ["zh"] = "选择你的增援",
                ["zh-TW"] = "選擇你的增援",
                ["de"] = "WÄHLE DEINE VERSTÄRKUNG",
                ["pl"] = "WYBIERZ SWOJE WZMOCNIENIE",
                ["pt"] = "ESCOLHA SEU REFORÇO",
                ["ru"] = "ВЫБЕРИТЕ ПОДКРЕПЛЕНИЕ",
                ["es"] = "ELIGE TU REFUERZO",
                ["uk"] = "ВИБЕРІТЬ ПІДКРІПЛЕННЯ",
                ["ja"] = "増援を選択してください",
                ["ko"] = "증원 선택",
            },
            ["ui.hover_description"] = new Dictionary<string, string>
            {
                ["en"] = "Hover over an upgrade to see its description",
                ["el"] = "Τοποθετήστε το ποντίκι πάνω από μια αναβάθμιση για να δείτε την περιγραφή της",
                ["fr"] = "Survolez une amélioration pour voir sa description",
                ["zh"] = "将鼠标悬停在升级上以查看其描述",
                ["zh-TW"] = "將滑鼠懸停在升級上以查看其描述",
                ["de"] = "Fahre über ein Upgrade, um dessen Beschreibung zu sehen",
                ["pl"] = "Najedź na ulepszenie, aby zobaczyć jego opis",
                ["pt"] = "Passe o mouse sobre uma melhoria para ver sua descrição",
                ["ru"] = "Наведите курсор на улучшение, чтобы увидеть его описание",
                ["es"] = "Pasa el cursor sobre una mejora para ver su descripción",
                ["uk"] = "Наведіть курсор на покращення, щоб побачити його опис",
                ["ja"] = "アップグレードにカーソルを合わせると説明が表示されます",
                ["ko"] = "업그레이드 위에 마우스를 올려 설명을 확인하세요",
            },
            ["ui.reroll_button"] = new Dictionary<string, string>
            {
                ["en"] = "Reroll ({0} <sprite name=\"CoinIcon\">)",
                ["el"] = "Επανάληψη ({0} <sprite name=\"CoinIcon\">)",
                ["fr"] = "Relancer ({0} <sprite name=\"CoinIcon\">)",
                ["zh"] = "重新选择 ({0} <sprite name=\"CoinIcon\">)",
                ["zh-TW"] = "重新選擇 ({0} <sprite name=\"CoinIcon\">)",
                ["de"] = "Neu würfeln ({0} <sprite name=\"CoinIcon\">)",
                ["pl"] = "Losuj ponownie ({0} <sprite name=\"CoinIcon\">)",
                ["pt"] = "Rolar novamente ({0} <sprite name=\"CoinIcon\">)",
                ["ru"] = "Перебросить ({0} <sprite name=\"CoinIcon\">)",
                ["es"] = "Volver a tirar ({0} <sprite name=\"CoinIcon\">)",
                ["uk"] = "Перекинути ({0} <sprite name=\"CoinIcon\">)",
                ["ja"] = "リロール ({0} <sprite name=\"CoinIcon\">)",
                ["ko"] = "다시 굴리기 ({0} <sprite name=\"CoinIcon\">)",
            },
            ["ui.skip_button"] = new Dictionary<string, string>
            {
                ["en"] = "Skip (+3 <sprite name=\"CoinIcon\">)",
                ["el"] = "Παράλειψη (+3 <sprite name=\"CoinIcon\">)",
                ["fr"] = "Passer (+3 <sprite name=\"CoinIcon\">)",
                ["zh"] = "跳过 (+3 <sprite name=\"CoinIcon\">)",
                ["zh-TW"] = "跳過 (+3 <sprite name=\"CoinIcon\">)",
                ["de"] = "Überspringen (+3 <sprite name=\"CoinIcon\">)",
                ["pl"] = "Pomiń (+3 <sprite name=\"CoinIcon\">)",
                ["pt"] = "Pular (+3 <sprite name=\"CoinIcon\">)",
                ["ru"] = "Пропустить (+3 <sprite name=\"CoinIcon\">)",
                ["es"] = "Omitir (+3 <sprite name=\"CoinIcon\">)",
                ["uk"] = "Пропустити (+3 <sprite name=\"CoinIcon\">)",
                ["ja"] = "スキップ (+3 <sprite name=\"CoinIcon\">)",
                ["ko"] = "건너뛰기 (+3 <sprite name=\"CoinIcon\">)",
            },
            ["ui.coins_unit"] = new Dictionary<string, string>
            {
                ["en"] = "coins",
                ["el"] = "νομίσματα",
                ["fr"] = "pièces",
                ["zh"] = "金币",
                ["zh-TW"] = "金幣",
                ["de"] = "Münzen",
                ["pl"] = "monet",
                ["pt"] = "moedas",
                ["ru"] = "монет",
                ["es"] = "monedas",
                ["uk"] = "монет",
                ["ja"] = "コイン",
                ["ko"] = "코인",
            },
            ["ui.cost_format"] = new Dictionary<string, string>
            {
                ["en"] = "{0} <sprite name=\"CoinIcon\">",
                ["el"] = "{0} <sprite name=\"CoinIcon\">",
                ["fr"] = "{0} <sprite name=\"CoinIcon\">",
                ["zh"] = "{0} <sprite name=\"CoinIcon\">",
                ["zh-TW"] = "{0} <sprite name=\"CoinIcon\">",
                ["de"] = "{0} <sprite name=\"CoinIcon\">",
                ["pl"] = "{0} <sprite name=\"CoinIcon\">",
                ["pt"] = "{0} <sprite name=\"CoinIcon\">",
                ["ru"] = "{0} <sprite name=\"CoinIcon\">",
                ["es"] = "{0} <sprite name=\"CoinIcon\">",
                ["uk"] = "{0} <sprite name=\"CoinIcon\">",
                ["ja"] = "{0} <sprite name=\"CoinIcon\">",
                ["ko"] = "{0} <sprite name=\"CoinIcon\">",
            },
            ["ui.max_level_reached"] = new Dictionary<string, string>
            {
                ["en"] = "Max level reached",
                ["el"] = "Μέγιστο επίπεδο επιτεύχθηκε",
                ["fr"] = "Niveau maximum atteint",
                ["zh"] = "已达最大等级",
                ["zh-TW"] = "已達最大等級",
                ["de"] = "Maximales Level erreicht",
                ["pl"] = "Osiągnięto maksymalny poziom",
                ["pt"] = "Nível máximo alcançado",
                ["ru"] = "Достигнут максимальный уровень",
                ["es"] = "Nivel máximo alcanzado",
                ["uk"] = "Досягнуто максимального рівня",
                ["ja"] = "最大レベルに到達",
                ["ko"] = "최대 레벨 도달",
            },
            ["ui.next_unlock"] = new Dictionary<string, string>
            {
                ["en"] = "Next unlock",
                ["el"] = "Επόμενο ξεκλείδωμα",
                ["fr"] = "Prochain déverrouillage",
                ["zh"] = "下一个解锁",
                ["zh-TW"] = "下一個解鎖",
                ["de"] = "Nächste Freischaltung",
                ["pl"] = "Następne odblokowanie",
                ["pt"] = "Próximo desbloqueio",
                ["ru"] = "Следующая разблокировка",
                ["es"] = "Próximo desbloqueo",
                ["uk"] = "Наступне розблокування",
                ["ja"] = "次のアンロック",
                ["ko"] = "다음 잠금 해제",
            },
        };
    }
}