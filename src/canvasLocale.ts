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
];

function getLocale(): Locale {
  return localStorage.getItem('echosphere_lang') === 'en' ? 'en' : 'ru';
}

function localizeCanvasText(text: string): string {
  if (getLocale() === 'en') return text;
  let result = text;
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
