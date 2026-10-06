type Locale = 'ru' | 'en';

const INSTALL_KEY = '__echosphere_canvas_locale_installed__';

const RU_REPLACEMENTS: Array<[RegExp, string]> = [
  [/\bSPHERIST\b/g, 'СФЕРИСТ'],
  [/\bHUNTER\b/g, 'ОХОТНИК'],
  [/\bENGINEER\b/g, 'ИНЖЕНЕР'],
  [/\bBERSERKER\b/g, 'БЕРСЕРК'],
  [/\bALCHEMIST\b/g, 'АЛХИМИК'],
  [/\bARCHITECT\b/g, 'АРХИТЕКТОР'],
  [/\bRESONANCE\b/g, 'РЕЗОНАНС'],
  [/\bCHORUS\b/g, 'ХОР'],
  [/\bHUNT\b/g, 'ОХОТА'],
  [/\bMARK\b/g, 'МЕТКА'],
  [/\bTROPHY\b/g, 'ТРОФЕЙ'],
  [/\bLINKS\b/g, 'СВЯЗИ'],
  [/\bNETWORK\b/g, 'СЕТЬ'],
  [/\bRELAY\b/g, 'РЕТРАНСЛЯЦИЯ'],
  [/\bFURY\b/g, 'ЯРОСТЬ'],
  [/\bCLOSE\b/g, 'БЛИЗКИЙ БОЙ'],
  [/\bREADY\b/g, 'ГОТОВО'],
  [/\bBLOOD TRAIL\b/g, 'КРОВАВЫЙ СЛЕД'],
  [/\bREACTION\b/g, 'РЕАКЦИЯ'],
  [/\bCATALYST\b/g, 'КАТАЛИЗАТОР'],
  [/\bFORM\b/g, 'ФОРМА'],
  [/\bSTRENGTH\b/g, 'СИЛА'],
  [/\bDMG\b/g, 'УРН'],
  [/\bAS\b/g, 'СА'],
  [/\bBOSS DEFEATED!\b/g, 'БОСС ПОБЕЖДЁН!'],
  [/\bMUTATION!\b/g, 'МУТАЦИЯ!'],
  [/\bTIME STOP!\b/g, 'ОСТАНОВКА ВРЕМЕНИ!'],
  [/\bINVULNERABLE!\b/g, 'НЕУЯЗВИМОСТЬ!'],
  [/\bLINE\b/g, 'ЛИНИЯ'],
  [/\bTRIANGLE\b/g, 'ТРЕУГОЛЬНИК'],
  [/\bSQUARE\b/g, 'КВАДРАТ'],
  [/\bCLUSTER\b/g, 'КЛАСТЕР'],
  [/\bNONE\b/g, 'НЕТ'],
  [/\bSTELLA\b/g, 'СТЕЛЛА'],
  [/\bBREACHER\b/g, 'ПРОРЫВАТЕЛЬ'],
  [/\bVOID LANCER\b/g, 'ПИКОВИК ПУСТОТЫ'],
  [/\bBROOD MIND\b/g, 'РОЕВОЙ РАЗУМ'],
  [/\bAURA TITAN\b/g, 'ТИТАН АУРЫ'],
  [/\bLATTICE\b/g, 'РЕШЁТКА'],
  [/\bRING\b/g, 'КОЛЬЦО'],
  [/RESONANCE BASIN/g, 'РЕЗОНАНСНЫЙ БАССЕЙН'],
  [/REGION STABILIZED/g, 'РЕГИОН СТАБИЛИЗИРОВАН'],
  [/ORIENTATION/g, 'ОРИЕНТАЦИЯ'],
  [/FIRST RESONANCE/g, 'ПЕРВЫЙ РЕЗОНАНС'],
  [/SPECIALIZATION/g, 'СПЕЦИАЛИЗАЦИЯ'],
  [/ESCALATION/g, 'ЭСКАЛАЦИЯ'],
  [/CONVERGENCE/g, 'СХОДИМОСТЬ'],
  [/STABILIZATION/g, 'СТАБИЛИЗАЦИЯ'],
  [/AXIS NODE/g, 'ОСЕВОЙ УЗЕЛ'],
  [/GLASS FLOW/g, 'СТЕКЛЯННЫЙ ПОТОК'],
  [/FRACTURE/g, 'РАЗЛОМ'],
  [/HOLLOW CONTOUR/g, 'ПУСТОЙ КОНТУР'],
  [/PRESSURE FIELD/g, 'ЗОНА ДАВЛЕНИЯ'],
  [/RESONANCE CACHE/g, 'ТАЙНИК РЕЗОНАНСА'],
  [/BREACH NODE/g, 'УЗЕЛ ПРОБОЯ'],
  [/ECHO RELAY/g, 'РЕЛЕ ЭХА'],
  [/LOST SIGNAL/g, 'ПОТЕРЯННЫЙ СИГНАЛ'],
  [/ELITE NEST/g, 'ГНЕЗДО ЭЛИТЫ'],
  [/RUPTURE/g, 'РАЗРЫВ'],
  [/BOSS TRACE/g, 'СЛЕД БОССА'],
  [/NETWORK FRACTURE/g, 'РАЗЛОМ СЕТИ'],
  [/CONDUCTOR BREAK/g, 'РАЗРЫВ ПРОВОДНИКА'],
  [/CONDUCTOR SURGE/g, 'ИМПУЛЬС ПРОВОДНИКА'],
  [/GEOMETRY SHIFT/g, 'СДВИГ ГЕОМЕТРИИ'],
  [/ARCHITECT SEAL/g, 'ПЕЧАТЬ АРХИТЕКТОРА'],
  [/NULL FIELD/g, 'ПОЛЕ НУЛЯ'],
  [/NULL SHOCK/g, 'УДАР НУЛЯ'],
  [/STELLA GUARD/g, 'СТРАЖ СТЕЛЛЫ'],
  [/STELLA JUDGEMENT/g, 'ПРИГОВОР СТЕЛЛЫ'],
  [/CHORUS SYNC/g, 'СИНХРОНИЗАЦИЯ ХОРА'],
  [/CHORUS PULSE/g, 'ИМПУЛЬС ХОРА'],
  [/SPLITTER/g, 'РАСКОЛ'],
  [/ECHO FREEZE/g, 'ЭХО-ЗАМОРОЗКА'],
  [/CLOSED TIME NETWORK/g, 'ЗАМКНУТАЯ СЕТЬ ВРЕМЕНИ'],
  [/TEMPORAL CORE/g, 'ВРЕМЕННОЕ ЯДРО'],
];

function getLocale(): Locale {
  return localStorage.getItem('echosphere_lang') === 'en' ? 'en' : 'ru';
}

function localizeCanvasText(text: string): string {
  if (getLocale() === 'en') return text;
  let result = text;
  const regionPhraseReplacements: Array<[string,string]> = [
    ['RESONANCE BASIN','РЕЗОНАНСНЫЙ БАССЕЙН'],['REGION STABILIZED','РЕГИОН СТАБИЛИЗИРОВАН'],
    ['FIRST RESONANCE','ПЕРВЫЙ РЕЗОНАНС'],['HOLLOW CONTOUR','ПУСТОЙ КОНТУР'],['PRESSURE FIELD','ЗОНА ДАВЛЕНИЯ'],
    ['RESONANCE CACHE','ТАЙНИК РЕЗОНАНСА'],['BREACH NODE','УЗЕЛ ПРОБОЯ'],['ECHO RELAY','РЕЛЕ ЭХА'],
    ['LOST SIGNAL','ПОТЕРЯННЫЙ СИГНАЛ'],['ELITE NEST','ГНЕЗДО ЭЛИТЫ'],['BOSS TRACE','СЛЕД БОССА'],
    ['FRACTURED NETWORK','РАЗЛОМ СЕТИ'],['NETWORK FRACTURE','РАЗЛОМ СЕТИ'],
    ['CONDUCTOR BREAK','РАЗРЫВ ПРОВОДНИКА'],['CONDUCTOR SURGE','ИМПУЛЬС ПРОВОДНИКА'],
    ['GEOMETRY SHIFT','СДВИГ ГЕОМЕТРИИ'],['ARCHITECT SEAL','ПЕЧАТЬ АРХИТЕКТОРА'],
    ['NULL FIELD','ПОЛЕ НУЛЯ'],['NULL SHOCK','УДАР НУЛЯ'],['STELLA GUARD','СТРАЖ СТЕЛЛЫ'],
    ['STELLA JUDGEMENT','ПРИГОВОР СТЕЛЛЫ'],['CHORUS SYNC','СИНХРОНИЗАЦИЯ ХОРА'],['CHORUS PULSE','ИМПУЛЬС ХОРА'],
    ['CLOSED TIME NETWORK','ЗАМКНУТАЯ СЕТЬ ВРЕМЕНИ'],['TEMPORAL CORE','ВРЕМЕННОЕ ЯДРО'],
  ];
  for (const [english, russian] of regionPhraseReplacements) result = result.split(english).join(russian);
  for (const [pattern, replacement] of RU_REPLACEMENTS) {
    result = result.replace(pattern, replacement);
  }
  return result;
}

export function installCanvasLocalization(): void {
  if (typeof window === 'undefined' || typeof CanvasRenderingContext2D === 'undefined') return;
  const prototype = CanvasRenderingContext2D.prototype as CanvasRenderingContext2D & Record<string, unknown>;
  if (prototype[INSTALL_KEY]) return;

  const originalFillText = prototype.fillText;
  prototype.fillText = function(this: CanvasRenderingContext2D, text: string, x: number, y: number, maxWidth?: number): void {
    originalFillText.call(this, localizeCanvasText(text), x, y, maxWidth);
  };

  prototype[INSTALL_KEY] = true;
}

installCanvasLocalization();
