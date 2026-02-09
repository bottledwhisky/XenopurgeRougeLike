using System.Collections.Generic;

namespace XenopurgeRougeLike
{
    public static partial class I18nData
    {
        // Gunslinger path strings
        private static Dictionary<string, Dictionary<string, string>> GetGunslingerTranslations()
        {
            return new Dictionary<string, Dictionary<string, string>>
            {
                // Gunslinger Affinity 2
                ["gunslinger.affinity2.description"] = new Dictionary<string, string>
                {
                    ["en"] = "+{0} <sprite name=\"AccuracyIcon\">. Unlock critical hit mechanic: {1}% base chance, +{2}% base extra damage. Unlock expert reinforcements.",
                    ["el"] = "+{0} <sprite name=\"AccuracyIcon\">. Ξεκλείδωμα μηχανισμού κρίσιμου χτυπήματος: {1}% βασική πιθανότητα, +{2}% βασική επιπλέον ζημιά. Ξεκλείδωμα ενισχύσεων εμπειρογνωμόνων.",
                    ["fr"] = "+{0} <sprite name=\"AccuracyIcon\">. Déverrouille le mécanisme de coup critique : {1}% de chance de base, +{2}% de dégâts supplémentaires de base. Déverrouille les renforts experts.",
                    ["zh"] = "+{0} <sprite name=\"AccuracyIcon\">。解锁暴击机制：{1}%基础概率，+{2}%基础额外伤害。解锁专家增援。",
                    ["zh-TW"] = "+{0} <sprite name=\"AccuracyIcon\">。解鎖暴擊機制：{1}%基礎機率，+{2}%基礎額外傷害。解鎖專家增援。",
                    ["de"] = "+{0} <sprite name=\"AccuracyIcon\">. Kritischer Treffer-Mechanismus freigeschaltet: {1}% Basiswahrscheinlichkeit, +{2}% Basis-Zusatzschaden. Schalte Experten-Verstärkungen frei.",
                    ["pl"] = "+{0} <sprite name=\"AccuracyIcon\">. Odblokuj mechanikę trafienia krytycznego: {1}% bazowa szansa, +{2}% bazowe dodatkowe obrażenia. Odblokuj wzmocnienia eksperckie.",
                    ["pt"] = "+{0} <sprite name=\"AccuracyIcon\">. Desbloqueia mecânica de acerto crítico: {1}% de chance base, +{2}% de dano extra base. Desbloqueia reforços especialistas.",
                    ["ru"] = "+{0} <sprite name=\"AccuracyIcon\">. Разблокировка механики критического удара: {1}% базовый шанс, +{2}% базовый дополнительный урон. Разблокировка экспертных подкреплений.",
                    ["es"] = "+{0} <sprite name=\"AccuracyIcon\">. Desbloquea mecánica de golpe crítico: {1}% de probabilidad base, +{2}% de daño extra base. Desbloquea refuerzos expertos.",
                    ["uk"] = "+{0} <sprite name=\"AccuracyIcon\">. Розблокування механіки критичного удару: {1}% базовий шанс, +{2}% базовий додатковий урон. Розблокування експертних підкріплень.",
                    ["ja"] = "+{0} <sprite name=\"AccuracyIcon\">。クリティカルヒットメカニズムをアンロック：基本確率{1}%、基本追加ダメージ+{2}%。エキスパート増援を解除。",
                    ["ko"] = "+{0} <sprite name=\"AccuracyIcon\">. 치명타 메커니즘 잠금 해제: 기본 확률 {1}%, 기본 추가 피해 +{2}%. 전문가 지원 잠금 해제.",
                },

                // Gunslinger Affinity 4
                ["gunslinger.affinity4.description"] = new Dictionary<string, string>
                {
                    ["en"] = "+{0} <sprite name=\"AccuracyIcon\">. Unlock critical hit mechanic: {1}% base chance, +{2}% base extra damage.",
                    ["el"] = "+{0} <sprite name=\"AccuracyIcon\">. Ξεκλείδωμα μηχανισμού κρίσιμου χτυπήματος: {1}% βασική πιθανότητα, +{2}% βασική επιπλέον ζημιά.",
                    ["fr"] = "+{0} <sprite name=\"AccuracyIcon\">. Déverrouille le mécanisme de coup critique : {1}% de chance de base, +{2}% de dégâts supplémentaires de base.",
                    ["zh"] = "+{0} <sprite name=\"AccuracyIcon\">。解锁暴击机制：{1}%基础概率，+{2}%基础额外伤害。",
                    ["zh-TW"] = "+{0} <sprite name=\"AccuracyIcon\">。解鎖暴擊機制：{1}%基礎機率，+{2}%基礎額外傷害。",
                    ["de"] = "+{0} <sprite name=\"AccuracyIcon\">. Kritischer Treffer-Mechanismus freigeschaltet: {1}% Basiswahrscheinlichkeit, +{2}% Basis-Zusatzschaden.",
                    ["pl"] = "+{0} <sprite name=\"AccuracyIcon\">. Odblokuj mechanikę trafienia krytycznego: {1}% bazowa szansa, +{2}% bazowe dodatkowe obrażenia.",
                    ["pt"] = "+{0} <sprite name=\"AccuracyIcon\">. Desbloqueia mecânica de acerto crítico: {1}% de chance base, +{2}% de dano extra base.",
                    ["ru"] = "+{0} <sprite name=\"AccuracyIcon\">. Разблокировка механики критического удара: {1}% базовый шанс, +{2}% базовый дополнительный урон.",
                    ["es"] = "+{0} <sprite name=\"AccuracyIcon\">. Desbloquea mecánica de golpe crítico: {1}% de probabilidad base, +{2}% de daño extra base.",
                    ["uk"] = "+{0} <sprite name=\"AccuracyIcon\">. Розблокування механіки критичного удару: {1}% базовий шанс, +{2}% базовий додатковий урон.",
                    ["ja"] = "+{0} <sprite name=\"AccuracyIcon\">。クリティカルヒットメカニズムをアンロック：基本確率{1}%、基本追加ダメージ+{2}%。",
                    ["ko"] = "+{0} <sprite name=\"AccuracyIcon\">. 치명타 메커니즘 잠금 해제: 기본 확률 {1}%, 기본 추가 피해 +{2}%.",
                },

                // Gunslinger Affinity 6
                ["gunslinger.affinity6.description"] = new Dictionary<string, string>
                {
                    ["en"] = "+{0} <sprite name=\"AccuracyIcon\">. Unlock critical hit mechanic: {1}% base chance, +{2}% base extra damage. Each <sprite name=\"AccuracyIcon\"> above 120 is converted to crit chance.",
                    ["el"] = "+{0} <sprite name=\"AccuracyIcon\">. Ξεκλείδωμα μηχανισμού κρίσιμου χτυπήματος: {1}% βασική πιθανότητα, +{2}% βασική επιπλέον ζημιά. Κάθε <sprite name=\"AccuracyIcon\"> πάνω από 120 μετατρέπεται σε πιθανότητα κρίσιμου χτυπήματος.",
                    ["fr"] = "+{0} <sprite name=\"AccuracyIcon\">. Déverrouille le mécanisme de coup critique : {1}% de chance de base, +{2}% de dégâts supplémentaires de base. Chaque <sprite name=\"AccuracyIcon\"> au-dessus de 120 est converti en chance de coup critique.",
                    ["zh"] = "+{0} <sprite name=\"AccuracyIcon\">。解锁暴击机制：{1}%基础概率，+{2}%基础额外伤害。超过120的每点<sprite name=\"AccuracyIcon\">转化为暴击率。",
                    ["zh-TW"] = "+{0} <sprite name=\"AccuracyIcon\">。解鎖暴擊機制：{1}%基礎機率，+{2}%基礎額外傷害。超過120的每點<sprite name=\"AccuracyIcon\">轉化為暴擊率。",
                    ["de"] = "+{0} <sprite name=\"AccuracyIcon\">. Kritischer Treffer-Mechanismus freigeschaltet: {1}% Basiswahrscheinlichkeit, +{2}% Basis-Zusatzschaden. Jeder <sprite name=\"AccuracyIcon\"> über 120 wird in kritische Trefferchance umgewandelt.",
                    ["pl"] = "+{0} <sprite name=\"AccuracyIcon\">. Odblokuj mechanikę trafienia krytycznego: {1}% bazowa szansa, +{2}% bazowe dodatkowe obrażenia. Każdy <sprite name=\"AccuracyIcon\"> powyżej 120 jest konwertowany na szansę na trafienie krytyczne.",
                    ["pt"] = "+{0} <sprite name=\"AccuracyIcon\">. Desbloqueia mecânica de acerto crítico: {1}% de chance base, +{2}% de dano extra base. Cada <sprite name=\"AccuracyIcon\"> acima de 120 é convertido em chance de acerto crítico.",
                    ["ru"] = "+{0} <sprite name=\"AccuracyIcon\">. Разблокировка механики критического удара: {1}% базовый шанс, +{2}% базовый дополнительный урон. Каждая <sprite name=\"AccuracyIcon\"> свыше 120 конвертируется в шанс критического удара.",
                    ["es"] = "+{0} <sprite name=\"AccuracyIcon\">. Desbloquea mecánica de golpe crítico: {1}% de probabilidad base, +{2}% de daño extra base. Cada <sprite name=\"AccuracyIcon\"> por encima de 120 se convierte en probabilidad de golpe crítico.",
                    ["uk"] = "+{0} <sprite name=\"AccuracyIcon\">. Розблокування механіки критичного удару: {1}% базовий шанс, +{2}% базовий додатковий урон. Кожна <sprite name=\"AccuracyIcon\"> понад 120 конвертується в шанс критичного удару.",
                    ["ja"] = "+{0} <sprite name=\"AccuracyIcon\">。クリティカルヒットメカニズムをアンロック：基本確率{1}%、基本追加ダメージ+{2}%。120を超える各<sprite name=\"AccuracyIcon\">はクリティカル率に変換されます。",
                    ["ko"] = "+{0} <sprite name=\"AccuracyIcon\">. 치명타 메커니즘 잠금 해제: 기본 확률 {1}%, 기본 추가 피해 +{2}%. 120을 초과하는 각 <sprite name=\"AccuracyIcon\">은 치명타 확률로 변환됩니다.",
                },

                // Targeting Weakspots - Name
                ["gunslinger.targeting_weakspots.name"] = new Dictionary<string, string>
                {
                    ["en"] = "Targeting Weakspots",
                    ["el"] = "Στόχευση Αδύναμων Σημείων",
                    ["fr"] = "Cibler les Points Faibles",
                    ["zh"] = "瞄准弱点",
                    ["zh-TW"] = "瞄準弱點",
                    ["de"] = "Schwachstellen anvisieren",
                    ["pl"] = "Celowanie w Słabe Punkty",
                    ["pt"] = "Mirar em Pontos Fracos",
                    ["ru"] = "Поражение слабых мест",
                    ["es"] = "Apuntar a Puntos Débiles",
                    ["uk"] = "Націлювання на слабкі місця",
                    ["ja"] = "弱点を狙う",
                    ["ko"] = "약점 조준",
                },

                // Targeting Weakspots - Flavour
                ["gunslinger.targeting_weakspots.flavour"] = new Dictionary<string, string>
                {
                    ["en"] = "\"Every target has a weak spot. You just need to find it.\"",
                    ["el"] = "\"Κάθε στόχος έχει ένα αδύναμο σημείο. Απλά πρέπει να το βρείτε.\"",
                    ["fr"] = "\"Chaque cible a un point faible. Il suffit de le trouver.\"",
                    ["zh"] = "「每个目标都有弱点。你只需要找到它。」",
                    ["zh-TW"] = "「每個目標都有弱點。你只需要找到它。」",
                    ["de"] = "\"Jedes Ziel hat eine Schwachstelle. Man muss sie nur finden.\"",
                    ["pl"] = "\"Każdy cel ma słaby punkt. Musisz go tylko znaleźć.\"",
                    ["pt"] = "\"Todo alvo tem um ponto fraco. Você só precisa encontrá-lo.\"",
                    ["ru"] = "\"У каждой цели есть слабое место. Нужно просто его найти.\"",
                    ["es"] = "\"Todo objetivo tiene un punto débil. Solo necesitas encontrarlo.\"",
                    ["uk"] = "\"У кожної цілі є слабке місце. Просто потрібно його знайти.\"",
                    ["ja"] = "「すべてのターゲットには弱点がある。それを見つけるだけだ。」",
                    ["ko"] = "\"모든 목표에는 약점이 있습니다. 찾기만 하면 됩니다.\"",
                },

                // Targeting Weakspots - Description
                ["gunslinger.targeting_weakspots.description"] = new Dictionary<string, string>
                {
                    ["en"] = "+{0}% critical hit chance.",
                    ["el"] = "+{0}% πιθανότητα κρίσιμου χτυπήματος.",
                    ["fr"] = "+{0}% de chance de coup critique.",
                    ["zh"] = "+{0}%暴击率。",
                    ["zh-TW"] = "+{0}%暴擊率。",
                    ["de"] = "+{0}% kritische Trefferchance.",
                    ["pl"] = "+{0}% szansy na trafienie krytyczne.",
                    ["pt"] = "+{0}% de chance de acerto crítico.",
                    ["ru"] = "+{0}% шанс критического удара.",
                    ["es"] = "+{0}% de probabilidad de golpe crítico.",
                    ["uk"] = "+{0}% шанс критичного удару.",
                    ["ja"] = "+{0}%クリティカルヒット率。",
                    ["ko"] = "+{0}% 치명타 확률.",
                },

                // Steady Shot - Name
                ["gunslinger.steady_shot.name"] = new Dictionary<string, string>
                {
                    ["en"] = "Steady Shot",
                    ["el"] = "Σταθερή Βολή",
                    ["fr"] = "Tir Stable",
                    ["zh"] = "稳固射击",
                    ["zh-TW"] = "穩固射擊",
                    ["de"] = "Stabiler Schuss",
                    ["pl"] = "Stabilny Strzał",
                    ["pt"] = "Tiro Firme",
                    ["ru"] = "Устойчивая стрельба",
                    ["es"] = "Tiro Estable",
                    ["uk"] = "Стабільний постріл",
                    ["ja"] = "安定射撃",
                    ["ko"] = "안정적인 사격",
                },

                // Steady Shot - Flavour
                ["gunslinger.steady_shot.flavour"] = new Dictionary<string, string>
                {
                    ["en"] = "\"Stand your ground and make every shot count.\"",
                    ["el"] = "\"Σταθείτε γερά και κάντε κάθε βολή να μετράει.\"",
                    ["fr"] = "\"Tenez votre position et faites compter chaque tir.\"",
                    ["zh"] = "「站稳脚跟，让每一发子弹都算数。」",
                    ["zh-TW"] = "「站穩腳跟，讓每一發子彈都算數。」",
                    ["de"] = "\"Halte Stand und lass jeden Schuss zählen.\"",
                    ["pl"] = "\"Stań mocno i niech każdy strzał się liczy.\"",
                    ["pt"] = "\"Mantenha sua posição e faça cada tiro valer.\"",
                    ["ru"] = "\"Держите позицию и цените каждый выстрел.\"",
                    ["es"] = "\"Mantén tu posición y haz que cada disparo cuente.\"",
                    ["uk"] = "\"Тримайте позицію і робіть так, щоб кожен постріл був важливим.\"",
                    ["ja"] = "「立ち位置を守り、すべての射撃を確実に。」",
                    ["ko"] = "\"자리를 지키고 모든 사격을 중요하게 만드세요.\"",
                },

                // Steady Shot - Description
                ["gunslinger.steady_shot.description"] = new Dictionary<string, string>
                {
                    ["en"] = "Stand and Fight and Suppressive Fire commands get +{0} <sprite name=\"AccuracyIcon\">.",
                    ["el"] = "Οι εντολές Σταθείτε και Μάχεστε και Καταπιεστικός Πυροβολισμός λαμβάνουν +{0} <sprite name=\"AccuracyIcon\">.",
                    ["fr"] = "Les commandes Tenir et Combattre et Tir de Suppression obtiennent +{0} <sprite name=\"AccuracyIcon\">.",
                    ["zh"] = "原地迎敌和压制射击指令获得+{0} <sprite name=\"AccuracyIcon\">。",
                    ["zh-TW"] = "原地迎敵和壓制射擊指令獲得+{0} <sprite name=\"AccuracyIcon\">。",
                    ["de"] = "Standhalten und Kämpfen und Unterdrückungsfeuer-Befehle erhalten +{0} <sprite name=\"AccuracyIcon\">.",
                    ["pl"] = "Polecenia Stać i Walczyć oraz Ogień Zaporowy otrzymują +{0} <sprite name=\"AccuracyIcon\">.",
                    ["pt"] = "Os comandos Ficar e Lutar e Fogo Supressivo recebem +{0} <sprite name=\"AccuracyIcon\">.",
                    ["ru"] = "Команды Держать позицию и Подавляющий огонь получают +{0} <sprite name=\"AccuracyIcon\">.",
                    ["es"] = "Los comandos Mantener Posición y Fuego de Supresión obtienen +{0} <sprite name=\"AccuracyIcon\">.",
                    ["uk"] = "Команди Триматися і Битися та Придушувальний вогонь отримують +{0} <sprite name=\"AccuracyIcon\">.",
                    ["ja"] = "踏みとどまって戦うと制圧射撃コマンドが+{0} <sprite name=\"AccuracyIcon\">を獲得します。",
                    ["ko"] = "서서 싸우기와 제압 사격 명령이 +{0} <sprite name=\"AccuracyIcon\">을 얻습니다.",
                },

                // Area Suppression - Name
                ["gunslinger.area_suppression.name"] = new Dictionary<string, string>
                {
                    ["en"] = "Area Suppression",
                    ["el"] = "Περιοχική Καταστολή",
                    ["fr"] = "Suppression de Zone",
                    ["zh"] = "范围压制",
                    ["zh-TW"] = "範圍壓制",
                    ["de"] = "Flächenunterdrückung",
                    ["pl"] = "Obszarowa Supresja",
                    ["pt"] = "Supressão em Área",
                    ["ru"] = "Подавление области",
                    ["es"] = "Supresión de Área",
                    ["uk"] = "Придушення зони",
                    ["ja"] = "範囲制圧",
                    ["ko"] = "지역 제압",
                },

                // Area Suppression - Flavour
                ["gunslinger.area_suppression.flavour"] = new Dictionary<string, string>
                {
                    ["en"] = "\"When you pin one down, pin them all.\"",
                    ["el"] = "\"Όταν καθηλώνεις έναν, καθήλωσε τους όλους.\"",
                    ["fr"] = "\"Quand vous en clouez un, clouez-les tous.\"",
                    ["zh"] = "「压制一个，就压制所有。」",
                    ["zh-TW"] = "「壓制一個，就壓制所有。」",
                    ["de"] = "\"Wenn du einen festhältst, halte sie alle fest.\"",
                    ["pl"] = "\"Gdy przygniatasz jednego, przygniataj wszystkich.\"",
                    ["pt"] = "\"Quando você imobiliza um, imobilize todos.\"",
                    ["ru"] = "\"Когда прижимаешь одного, прижми всех.\"",
                    ["es"] = "\"Cuando inmovilizas a uno, inmoviliza a todos.\"",
                    ["uk"] = "\"Коли придавлюєш одного, придуши всіх.\"",
                    ["ja"] = "「一人を釘付けにするとき、全員を釘付けにしろ。」",
                    ["ko"] = "\"하나를 억제하면 모두를 억제하라.\"",
                },

                // Area Suppression - Description
                ["gunslinger.area_suppression.description"] = new Dictionary<string, string>
                {
                    ["en"] = "When an enemy is suppressed, all other enemies in the same tile also receive the same suppression effect.",
                    ["el"] = "Όταν ένας εχθρός καταστέλλεται, όλοι οι άλλοι εχθροί στο ίδιο πλακίδιο λαμβάνουν επίσης το ίδιο εφέ καταστολής.",
                    ["fr"] = "Lorsqu'un ennemi est supprimé, tous les autres ennemis dans la même case reçoivent également le même effet de suppression.",
                    ["zh"] = "当一个敌人被压制时，同格内所有其他敌人也会受到同样的压制效果。",
                    ["zh-TW"] = "當一個敵人被壓制時，同格內所有其他敵人也會受到同樣的壓制效果。",
                    ["de"] = "Wenn ein Feind unterdrückt wird, erhalten alle anderen Feinde im selben Feld ebenfalls denselben Unterdrückungseffekt.",
                    ["pl"] = "Gdy wróg jest tłumiony, wszyscy inni wrogowie na tym samym polu również otrzymują ten sam efekt tłumienia.",
                    ["pt"] = "Quando um inimigo é suprimido, todos os outros inimigos no mesmo tile também recebem o mesmo efeito de supressão.",
                    ["ru"] = "Когда враг подавлен, все остальные враги на той же клетке также получают тот же эффект подавления.",
                    ["es"] = "Cuando un enemigo es suprimido, todos los demás enemigos en la misma casilla también reciben el mismo efecto de supresión.",
                    ["uk"] = "Коли ворог пригнічений, всі інші вороги на тій самій клітинці також отримують той самий ефект придушення.",
                    ["ja"] = "敵が制圧されると、同じタイルにいる他のすべての敵も同じ制圧効果を受けます。",
                    ["ko"] = "적이 제압되면 같은 타일에 있는 다른 모든 적도 동일한 제압 효과를 받습니다.",
                },

                // Enhanced Suppression - Name
                ["gunslinger.enhanced_suppression.name"] = new Dictionary<string, string>
                {
                    ["en"] = "Enhanced Suppression",
                    ["el"] = "Ενισχυμένη Καταστολή",
                    ["fr"] = "Suppression Améliorée",
                    ["zh"] = "压制强化",
                    ["zh-TW"] = "壓制強化",
                    ["de"] = "Verstärkte Unterdrückung",
                    ["pl"] = "Wzmocnione Tłumienie",
                    ["pt"] = "Supressão Aprimorada",
                    ["ru"] = "Усиленное подавление",
                    ["es"] = "Supresión Mejorada",
                    ["uk"] = "Посилене придушення",
                    ["ja"] = "強化された制圧",
                    ["ko"] = "강화된 제압",
                },

                // Enhanced Suppression - Flavour
                ["gunslinger.enhanced_suppression.flavour"] = new Dictionary<string, string>
                {
                    ["en"] = "\"Keep their heads down. Way down.\"",
                    ["el"] = "\"Κρατήστε τα κεφάλια τους χαμηλά. Πολύ χαμηλά.\"",
                    ["fr"] = "\"Gardez leurs têtes baissées. Très baissées.\"",
                    ["zh"] = "「让他们抬不起头来。完全抬不起来。」",
                    ["zh-TW"] = "「讓他們抬不起頭來。完全抬不起來。」",
                    ["de"] = "\"Halte ihre Köpfe unten. Weit unten.\"",
                    ["pl"] = "\"Trzymaj ich głowy nisko. Bardzo nisko.\"",
                    ["pt"] = "\"Mantenha suas cabeças baixas. Bem baixas.\"",
                    ["ru"] = "\"Прижми их к земле. К самой земле.\"",
                    ["es"] = "\"Mantén sus cabezas agachadas. Muy agachadas.\"",
                    ["uk"] = "\"Притисни їх до землі. Щільно до землі.\"",
                    ["ja"] = "\"彼らの頭を下げ続けろ。ずっと下に。\"",
                    ["ko"] = "\"그들의 머리를 숙이게 하세요. 완전히 숙이게.\"",
                },

                // Enhanced Suppression - Description
                ["gunslinger.enhanced_suppression.description"] = new Dictionary<string, string>
                {
                    ["en"] = "Suppressive Fire speed debuff increased by {0}%.",
                    ["el"] = "Η μείωση ταχύτητας του Καταπιεστικού Πυροβολισμού αυξάνεται κατά {0}%.",
                    ["fr"] = "Le malus de vitesse du Tir de Suppression est augmenté de {0}%.",
                    ["zh"] = "压制射击的速度减益效果提高{0}%。",
                    ["zh-TW"] = "壓制射擊的速度減益效果提高{0}%。",
                    ["de"] = "Geschwindigkeitsabzug des Unterdrückungsfeuers um {0}% erhöht.",
                    ["pl"] = "Kara do prędkości Ognia Zaporowego zwiększona o {0}%.",
                    ["pt"] = "Debuff de velocidade do Fogo Supressivo aumentado em {0}%.",
                    ["ru"] = "Штраф к скорости от Подавляющего огня увеличен на {0}%.",
                    ["es"] = "El debuff de velocidad del Fuego de Supresión aumenta un {0}%.",
                    ["uk"] = "Штраф до швидкості від Придушувального вогню збільшено на {0}%.",
                    ["ja"] = "制圧射撃の速度デバフが{0}%増加します。",
                    ["ko"] = "제압 사격의 속도 디버프가 {0}% 증가합니다.",
                },

                // Ricochet - Name
                ["gunslinger.ricochet.name"] = new Dictionary<string, string>
                {
                    ["en"] = "Ricochet",
                    ["el"] = "Αναπήδηση",
                    ["fr"] = "Ricochet",
                    ["zh"] = "跳弹",
                    ["zh-TW"] = "跳彈",
                    ["de"] = "Querschläger",
                    ["pl"] = "Rykoszet",
                    ["pt"] = "Ricochete",
                    ["ru"] = "Рикошет",
                    ["es"] = "Rebote",
                    ["uk"] = "Рикошет",
                    ["ja"] = "跳弾",
                    ["ko"] = "도탄",
                },

                // Ricochet - Flavour
                ["gunslinger.ricochet.flavour"] = new Dictionary<string, string>
                {
                    ["en"] = "\"A miss is just a bullet looking for another target.\"",
                    ["el"] = "\"Μια αποτυχία είναι απλά μια σφαίρα που ψάχνει για άλλο στόχο.\"",
                    ["fr"] = "\"Un raté n'est qu'une balle cherchant une autre cible.\"",
                    ["zh"] = "「没有打偏，只是子弹在寻找下一个目标。」",
                    ["zh-TW"] = "「沒有打偏，只是子彈在尋找下一個目標。」",
                    ["de"] = "\"Ein Fehlschuss ist nur eine Kugel auf der Suche nach einem anderen Ziel.\"",
                    ["pl"] = "\"Chybienie to tylko kula szukająca innego celu.\"",
                    ["pt"] = "\"Um erro é apenas uma bala procurando outro alvo.\"",
                    ["ru"] = "\"Промах - это просто пуля, ищущая другую цель.\"",
                    ["es"] = "\"Un fallo es solo una bala buscando otro objetivo.\"",
                    ["uk"] = "\"Промах - це просто куля, що шукає іншу ціль.\"",
                    ["ja"] = "「外れは次の標的を探している弾丸に過ぎない。」",
                    ["ko"] = "\"빗나간 건 다른 목표를 찾는 총알일 뿐입니다.\"",
                },

                // Ricochet - Description
                ["gunslinger.ricochet.description"] = new Dictionary<string, string>
                {
                    ["en"] = "When a bullet misses, {0}% chance to ricochet to another enemy on the target's tile or behind it.",
                    ["el"] = "Όταν μια σφαίρα αστοχεί, {0}% πιθανότητα να αναπηδήσει σε άλλον εχθρό στο πλακίδιο του στόχου ή πίσω από αυτό.",
                    ["fr"] = "Lorsqu'une balle rate, {0}% de chance de ricocher sur un autre ennemi sur la case de la cible ou derrière.",
                    ["zh"] = "子弹未命中时，有{0}%概率跳弹到目标当前格或身后的另一个敌人。",
                    ["zh-TW"] = "子彈未命中時，有{0}%機率跳彈到目標當前格或身後的另一個敵人。",
                    ["de"] = "Wenn eine Kugel verfehlt, {0}% Chance auf Querschläger zu einem anderen Feind auf dem Feld des Ziels oder dahinter.",
                    ["pl"] = "Gdy kula chybia, {0}% szansy na rykoszet do innego wroga na polu celu lub za nim.",
                    ["pt"] = "Quando uma bala erra, {0}% de chance de ricochetear para outro inimigo no tile do alvo ou atrás dele.",
                    ["ru"] = "Когда пуля промахивается, {0}% шанс рикошета в другого врага на клетке цели или позади неё.",
                    ["es"] = "Cuando una bala falla, {0}% de probabilidad de rebotar hacia otro enemigo en la casilla del objetivo o detrás.",
                    ["uk"] = "Коли куля промахується, {0}% шанс рикошету в іншого ворога на клітинці цілі або позаду неї.",
                    ["ja"] = "弾丸が外れたとき、{0}%の確率で目標のタイルまたはその後方の別の敵に跳弾します。",
                    ["ko"] = "총알이 빗나갔을 때, {0}% 확률로 목표의 타일이나 그 뒤에 있는 다른 적에게 도탄합니다.",
                },

                // Penetrating Rounds - Name
                ["gunslinger.penetrating_rounds.name"] = new Dictionary<string, string>
                {
                    ["en"] = "Penetrating Rounds",
                    ["el"] = "Διαπερνούσες Σφαίρες",
                    ["fr"] = "Balles Perforantes",
                    ["zh"] = "穿透弹",
                    ["zh-TW"] = "穿透彈",
                    ["de"] = "Durchschlagsmunition",
                    ["pl"] = "Pociski Penetrujące",
                    ["pt"] = "Munição Perfurante",
                    ["ru"] = "Бронебойные патроны",
                    ["es"] = "Munición Perforante",
                    ["uk"] = "Бронебійні патрони",
                    ["ja"] = "貫通弾",
                    ["ko"] = "관통탄",
                },

                // Penetrating Rounds - Flavour
                ["gunslinger.penetrating_rounds.flavour"] = new Dictionary<string, string>
                {
                    ["en"] = "\"One bullet, multiple targets. Now that's efficiency.\"",
                    ["el"] = "\"Μία σφαίρα, πολλαπλοί στόχοι. Αυτή είναι αποδοτικότητα.\"",
                    ["fr"] = "\"Une balle, plusieurs cibles. Voilà ce qu'on appelle l'efficacité.\"",
                    ["zh"] = "「一发子弹，多个目标。这才叫效率。」",
                    ["zh-TW"] = "「一發子彈，多個目標。這才叫效率。」",
                    ["de"] = "\"Eine Kugel, mehrere Ziele. Das ist Effizienz.\"",
                    ["pl"] = "\"Jeden pocisk, wiele celów. To jest wydajność.\"",
                    ["pt"] = "\"Uma bala, vários alvos. Isso é eficiência.\"",
                    ["ru"] = "\"Одна пуля, несколько целей. Вот что такое эффективность.\"",
                    ["es"] = "\"Una bala, múltiples objetivos. Eso es eficiencia.\"",
                    ["uk"] = "\"Одна куля, кілька цілей. Ось що таке ефективність.\"",
                    ["ja"] = "「一発の弾丸、複数の標的。これが効率というものだ。」",
                    ["ko"] = "\"하나의 총알, 여러 목표. 그것이 효율성입니다.\"",
                },

                // Penetrating Rounds - Description
                ["gunslinger.penetrating_rounds.description"] = new Dictionary<string, string>
                {
                    ["en"] = "On critical hit, bullets penetrate through the target, dealing additional base damage to all enemies on the target's tile and behind it.",
                    ["el"] = "Σε κρίσιμο χτύπημα, οι σφαίρες διαπερνούν το στόχο, προκαλώντας επιπλέον βασική ζημιά σε όλους τους εχθρούς στο πλακίδιο του στόχου και πίσω από αυτό.",
                    ["fr"] = "Sur un coup critique, les balles traversent la cible, infligeant des dégâts de base supplémentaires à tous les ennemis sur la case de la cible et derrière.",
                    ["zh"] = "暴击时子弹穿透目标，对目标当前格及其后方的所有敌人造成等同于非暴击伤害的额外伤害。",
                    ["zh-TW"] = "暴擊時子彈穿透目標，對目標當前格及其後方的所有敵人造成等同於非暴擊傷害的額外傷害。",
                    ["de"] = "Bei kritischem Treffer durchdringen Kugeln das Ziel und verursachen zusätzlichen Basisschaden bei allen Feinden auf dem Feld des Ziels und dahinter.",
                    ["pl"] = "Przy trafieniu krytycznym, pociski przebijają cel, zadając dodatkowe podstawowe obrażenia wszystkim wrogom na polu celu i za nim.",
                    ["pt"] = "Em acerto crítico, as balas penetram através do alvo, causando dano base adicional a todos os inimigos no tile do alvo e atrás dele.",
                    ["ru"] = "При критическом ударе пули пробивают цель, нанося дополнительный базовый урон всем врагам на клетке цели и позади неё.",
                    ["es"] = "En golpe crítico, las balas penetran a través del objetivo, causando daño base adicional a todos los enemigos en la casilla del objetivo y detrás.",
                    ["uk"] = "При критичному ударі кулі пробивають ціль, завдаючи додаткову базову шкоду всімворогам на клітинці цілі та позаду неї.",
                    ["ja"] = "クリティカルヒット時、弾丸は目標を貫通し、目標のタイルとその後方のすべての敵に追加の基本ダメージを与えます。",
                    ["ko"] = "치명타 시, 총알이 목표를 관통하여 목표의 타일과 그 뒤에 있는 모든 적에게 추가 기본 피해를 입힙니다.",
                },

                // Quick Draw - Name
                ["gunslinger.quick_draw.name"] = new Dictionary<string, string>
                {
                    ["en"] = "Quick Draw",
                    ["el"] = "Γρήγορη Κλήρωση",
                    ["fr"] = "Tir Rapide",
                    ["zh"] = "快速拔枪",
                    ["zh-TW"] = "快速拔槍",
                    ["de"] = "Schnellziehen",
                    ["pl"] = "Szybkie Wyciąganie",
                    ["pt"] = "Saque Rápido",
                    ["ru"] = "Быстрая стрельба",
                    ["es"] = "Desenfunde Rápido",
                    ["uk"] = "Швидке витягування",
                    ["ja"] = "早撃ち",
                    ["ko"] = "빠른 발사",
                },

                // Quick Draw - Flavour
                ["gunslinger.quick_draw.flavour"] = new Dictionary<string, string>
                {
                    ["en"] = "\"Speed kills. Literally.\"",
                    ["el"] = "\"Η ταχύτητα σκοτώνει. Κυριολεκτικά.\"",
                    ["fr"] = "\"La vitesse tue. Littéralement.\"",
                    ["zh"] = "「速度致命。字面意义上。」",
                    ["zh-TW"] = "「速度致命。字面意義上。」",
                    ["de"] = "\"Geschwindigkeit tötet. Buchstäblich.\"",
                    ["pl"] = "\"Szybkość zabija. Dosłownie.\"",
                    ["pt"] = "\"Velocidade mata. Literalmente.\"",
                    ["ru"] = "\"Скорость убивает. Буквально.\"",
                    ["es"] = "\"La velocidad mata. Literalmente.\"",
                    ["uk"] = "\"Швидкість вбиває. Буквально.\"",
                    ["ja"] = "「速度が殺す。文字通り。」",
                    ["ko"] = "\"속도가 죽입니다. 말 그대로.\"",
                },

                // Quick Draw - Description
                ["gunslinger.quick_draw.description"] = new Dictionary<string, string>
                {
                    ["en"] = "All ranged weapons fire all bullets in magazine within the first {0}% of their normal firing duration.",
                    ["el"] = "Όλα τα όπλα εξ αποστάσεως ρίχνουν όλες τις σφαίρες του γεμιστήρα μέσα στο πρώτο {0}% της κανονικής τους διάρκειας πυροβολισμού.",
                    ["fr"] = "Toutes les armes à distance tirent toutes les balles du chargeur dans les premiers {0}% de leur durée de tir normale.",
                    ["zh"] = "所有远程武器在正常射击时间的前{0}%内射出弹匣中的所有子弹。",
                    ["zh-TW"] = "所有遠程武器在正常射擊時間的前{0}%內射出彈匣中的所有子彈。",
                    ["de"] = "Alle Fernkampfwaffen feuern alle Kugeln im Magazin innerhalb der ersten {0}% ihrer normalen Feuerdauer ab.",
                    ["pl"] = "Wszystkie bronie dystansowe wystrzeliwują wszystkie pociski z magazynka w pierwszych {0}% ich normalnego czasu strzału.",
                    ["pt"] = "Todas as armas de longo alcance disparam todas as balas no magazine dentro dos primeiros {0}% de sua duração normal de disparo.",
                    ["ru"] = "Все дальнобойные оружия выпускают все пули в магазине в первые {0}% от их обычной продолжительности стрельбы.",
                    ["es"] = "Todas las armas a distancia disparan todas las balas del cargador en el primer {0}% de su duración normal de disparo.",
                    ["uk"] = "Вся зброя дальнього бою випускає всі кулі в магазині протягом перших {0}% від їх звичайної тривалості стрільби.",
                    ["ja"] = "すべての遠距離武器は、通常の発射時間の最初の{0}%以内にマガジン内のすべての弾丸を発射します。",
                    ["ko"] = "모든 원거리 무기가 정상 발사 시간의 처음 {0}% 이내에 탄창의 모든 총알을 발사합니다.",
                },
                
                // Assault Training (突击训练)
                ["gunslinger.assault_training.name"] = new Dictionary<string, string>
                {
                    ["en"] = "Assault Training",
                    ["el"] = "Εκπαίδευση Επίθεσης",
                    ["fr"] = "Entraînement d'Assaut",
                    ["zh"] = "突击训练",
                    ["zh-TW"] = "突擊訓練",
                    ["de"] = "Sturmtraining",
                    ["pl"] = "Trening Szturmowy",
                    ["pt"] = "Treinamento de Assalto",
                    ["ru"] = "Штурмовая подготовка",
                    ["es"] = "Entrenamiento de Asalto",
                    ["uk"] = "Штурмове тренування",
                    ["ja"] = "突撃訓練",
                    ["ko"] = "돌격 훈련",
                },
                ["gunslinger.assault_training.description"] = new Dictionary<string, string>
                {
                    ["en"] = "Remove <sprite name=\"AccuracyIcon\"> penalties from ranged weapons at non-optimal distances.",
                    ["el"] = "Αφαιρέστε ποινές <sprite name=\"AccuracyIcon\"> από όπλα εξ αποστάσεως σε μη βέλτιστες αποστάσεις.",
                    ["fr"] = "Supprime les pénalités de <sprite name=\"AccuracyIcon\"> des armes à distance à des distances non optimales.",
                    ["zh"] = "移除远程武器在非最优距离上的<sprite name=\"AccuracyIcon\">惩罚。",
                    ["zh-TW"] = "移除遠程武器在非最優距離上的<sprite name=\"AccuracyIcon\">懲罰。",
                    ["de"] = "Entfernt <sprite name=\"AccuracyIcon\">-Strafen von Fernkampfwaffen bei nicht optimalen Entfernungen.",
                    ["pl"] = "Usuwa kary <sprite name=\"AccuracyIcon\"> z broni dystansowej na nieoptymalnych dystansach.",
                    ["pt"] = "Remove penalidades de <sprite name=\"AccuracyIcon\"> de armas de longo alcance em distâncias não ideais.",
                    ["ru"] = "Убирает штрафы <sprite name=\"AccuracyIcon\"> дальнобойного оружия на неоптимальных дистанциях.",
                    ["es"] = "Elimina penalizaciones de <sprite name=\"AccuracyIcon\"> de armas a distancia en distancias no óptimas.",
                    ["uk"] = "Прибирає штрафи <sprite name=\"AccuracyIcon\"> далекобійної зброї на неоптимальних дистанціях.",
                    ["ja"] = "最適でない距離での遠距離武器の<sprite name=\"AccuracyIcon\">ペナルティを削除する。",
                    ["ko"] = "최적이 아닌 거리에서 원거리 무기의 <sprite name=\"AccuracyIcon\"> 페널티를 제거합니다.",
                },
                ["gunslinger.assault_training.flavour"] = new Dictionary<string, string>
                {
                    ["en"] = "Relentless advance. No distance is wrong when every shot counts.",
                    ["el"] = "Ακατάπαυστη προέλαση. Καμία απόσταση δεν είναι λάθος όταν κάθε βολή μετράει.",
                    ["fr"] = "Avancée implacable. Aucune distance n'est mauvaise quand chaque tir compte.",
                    ["zh"] = "无情推进。当每一枪都至关重要时，没有距离是错的。",
                    ["zh-TW"] = "無情推進。當每一槍都至關重要時，沒有距離是錯的。",
                    ["de"] = "Unerbittlicher Vormarsch. Keine Entfernung ist falsch, wenn jeder Schuss zählt.",
                    ["pl"] = "Nieustępliwy atak. Żaden dystans nie jest zły, gdy każdy strzał się liczy.",
                    ["pt"] = "Avanço implacável. Nenhuma distância está errada quando cada tiro conta.",
                    ["ru"] = "Неумолимое наступление. Нет неправильной дистанции, когда важен каждый выстрел.",
                    ["es"] = "Avance implacable. Ninguna distancia es incorrecta cuando cada disparo cuenta.",
                    ["uk"] = "Невблаганний наступ. Немає неправильної дистанції, коли важливий кожен постріл.",
                    ["ja"] = "容赦ない前進。すべての射撃が重要なとき、間違った距離などない。",
                    ["ko"] = "무자비한 진격. 모든 사격이 중요할 때 잘못된 거리는 없습니다.",
                },

                // Death's Eye (死神之眼)
                ["gunslinger.deaths_eye.name"] = new Dictionary<string, string>
                {
                    ["en"] = "Death's Eye",
                    ["el"] = "Μάτι του Θανάτου",
                    ["fr"] = "Œil de la Mort",
                    ["zh"] = "死神之眼",
                    ["zh-TW"] = "死神之眼",
                    ["de"] = "Auge des Todes",
                    ["pl"] = "Oko Śmierci",
                    ["pt"] = "Olho da Morte",
                    ["ru"] = "Око Смерти",
                    ["es"] = "Ojo de la Muerte",
                    ["uk"] = "Око Смерті",
                    ["ja"] = "死神の目",
                    ["ko"] = "죽음의 눈",
                },
                ["gunslinger.deaths_eye.description"] = new Dictionary<string, string>
                {
                    ["en"] = "Gain \"Death's Eye\" command, usable once. All friendly units' ranged attacks are guaranteed to critically hit for {0} <sprite name=\"time icon\">.",
                    ["el"] = "Κερδίστε την εντολή \"Μάτι του Θανάτου\", χρησιμοποιείται μία φορά. Όλες οι επιθέσεις εξ αποστάσεως των φίλιων μονάδων είναι εγγυημένες να κάνουν κρίσιμο χτύπημα για {0} <sprite name=\"time icon\">.",
                    ["fr"] = "Obtenez la commande \"Œil de la Mort\", utilisable une fois. Toutes les attaques à distance des unités alliées sont garanties d'infliger un coup critique pendant {0} <sprite name=\"time icon\">.",
                    ["zh"] = "获得\"死神之眼\"指令，限一次。所有友方单位的远程攻击在 {0} <sprite name=\"time icon\">内必定暴击。",
                    ["zh-TW"] = "獲得\"死神之眼\"指令，限一次。所有友方單位的遠程攻擊在 {0} <sprite name=\"time icon\">內必定爆擊。",
                    ["de"] = "Erhalte den Befehl \"Auge des Todes\", einmal verwendbar. Alle Fernkampfangriffe verbündeter Einheiten sind garantiert kritische Treffer für {0} <sprite name=\"time icon\">.",
                    ["pl"] = "Zdobądź komendę \"Oko Śmierci\", do jednorazowego użycia. Wszystkie ataki dystansowe sojuszniczych jednostek gwarantują trafienie krytyczne przez {0} <sprite name=\"time icon\">.",
                    ["pt"] = "Ganhe o comando \"Olho da Morte\", utilizável uma vez. Todos os ataques à distância de unidades aliadas acertam criticamente garantidamente por {0} <sprite name=\"time icon\">.",
                    ["ru"] = "Получите команду \"Око Смерти\", используется один раз. Все дальние атаки союзных юнитов гарантированно наносят критический урон в течение {0} <sprite name=\"time icon\">.",
                    ["es"] = "Obtén el comando \"Ojo de la Muerte\", utilizable una vez. Todos los ataques a distancia de las unidades aliadas están garantizados de impactar críticamente durante {0} <sprite name=\"time icon\">.",
                    ["uk"] = "Отримайте команду \"Око Смерті\", використовується один раз. Усі далекі атаки союзних юнітів гарантовано завдають критичної шкоди протягом {0} <sprite name=\"time icon\">.",
                    ["ja"] = "\"死神の目\"コマンドを獲得、一度のみ使用可能。すべての味方ユニットの遠距離攻撃が{0} <sprite name=\"time icon\">の間必ずクリティカルヒットする。",
                    ["ko"] = "\"죽음의 눈\" 명령 획득, 한 번만 사용 가능. 모든 아군 유닛의 원거리 공격이 {0} <sprite name=\"time icon\"> 동안 반드시 치명타로 명중합니다.",
                },
                ["gunslinger.deaths_eye.flavour"] = new Dictionary<string, string>
                {
                    ["en"] = "Time slows. Every target is marked. Death is inevitable.",
                    ["el"] = "Ο χρόνος επιβραδύνεται. Κάθε στόχος είναι σημειωμένος. Ο θάνατος είναι αναπόφευκτος.",
                    ["fr"] = "Le temps ralentit. Chaque cible est marquée. La mort est inévitable.",
                    ["zh"] = "时间放缓。每个目标都被标记。死亡不可避免。",
                    ["zh-TW"] = "時間放緩。每個目標都被標記。死亡不可避免。",
                    ["de"] = "Die Zeit verlangsamt sich. Jedes Ziel ist markiert. Der Tod ist unvermeidlich.",
                    ["pl"] = "Czas zwalnia. Każdy cel jest oznaczony. Śmierć jest nieunikniona.",
                    ["pt"] = "O tempo desacelera. Cada alvo está marcado. A morte é inevitável.",
                    ["ru"] = "Время замедляется. Каждая цель помечена. Смерть неизбежна.",
                    ["es"] = "El tiempo se ralentiza. Cada objetivo está marcado. La muerte es inevitable.",
                    ["uk"] = "Час сповільнюється. Кожна ціль позначена. Смерть неминуча.",
                    ["ja"] = "時間が遅くなる。すべてのターゲットがマークされる。死は避けられない。",
                    ["ko"] = "시간이 느려집니다. 모든 표적이 표시됩니다. 죽음은 피할 수 없습니다.",
                },
                ["gunslinger.deaths_eye.card_name"] = new Dictionary<string, string>
                {
                    ["en"] = "Death's Eye",
                    ["el"] = "Μάτι του Θανάτου",
                    ["fr"] = "Œil de la Mort",
                    ["zh"] = "死神之眼",
                    ["zh-TW"] = "死神之眼",
                    ["de"] = "Auge des Todes",
                    ["pl"] = "Oko Śmierci",
                    ["pt"] = "Olho da Morte",
                    ["ru"] = "Око Смерти",
                    ["es"] = "Ojo de la Muerte",
                    ["uk"] = "Око Смерті",
                    ["ja"] = "死神の目",
                    ["ko"] = "죽음의 눈",
                },
                ["gunslinger.deaths_eye.card_description"] = new Dictionary<string, string>
                {
                    ["en"] = "All friendly units' ranged attacks are guaranteed to critically hit for {0} <sprite name=\"time icon\">.",
                    ["el"] = "Όλες οι επιθέσεις εξ αποστάσεως των φίλιων μονάδων είναι εγγυημένες να κάνουν κρίσιμο χτύπημα για {0} <sprite name=\"time icon\">.",
                    ["fr"] = "Toutes les attaques à distance des unités alliées sont garanties d'infliger un coup critique pendant {0} <sprite name=\"time icon\">.",
                    ["zh"] = "所有友方单位的远程攻击在 {0} <sprite name=\"time icon\">内必定暴击。",
                    ["zh-TW"] = "所有友方單位的遠程攻擊在 {0} <sprite name=\"time icon\">內必定爆擊。",
                    ["de"] = "Alle Fernkampfangriffe verbündeter Einheiten sind garantiert kritische Treffer für {0} <sprite name=\"time icon\">.",
                    ["pl"] = "Wszystkie ataki dystansowe sojuszniczych jednostek gwarantują trafienie krytyczne przez {0} <sprite name=\"time icon\">.",
                    ["pt"] = "Todos os ataques à distância de unidades aliadas acertam criticamente garantidamente por {0} <sprite name=\"time icon\">.",
                    ["ru"] = "Все дальние атаки союзных юнитов гарантированно наносят критический урон в течение {0} <sprite name=\"time icon\">.",
                    ["es"] = "Todos los ataques a distancia de las unidades aliadas están garantizados de impactar críticamente durante {0} <sprite name=\"time icon\">.",
                    ["uk"] = "Усі далекі атаки союзних юнітів гарантовано завдають критичної шкоди протягом {0} <sprite name=\"time icon\">.",
                    ["ja"] = "すべての味方ユニットの遠距離攻撃が{0} <sprite name=\"time icon\">の間必ずクリティカルヒットする。",
                    ["ko"] = "모든 아군 유닛의 원거리 공격이 {0} <sprite name=\"time icon\"> 동안 반드시 치명타로 명중합니다.",
                },
            };
        }
    }
}
