import { useEffect, useMemo, useState } from 'react';
import type { Lang, TranslationKey } from './i18n';
import type { Handedness } from './MobileControls';

export type TutorialStep = 0 | 1 | 2 | 3 | 4 | 5;

interface TutorialOverlayProps {
  step: TutorialStep;
  lang: Lang;
  t: (key: TranslationKey) => string;
  handedness: Handedness;
  canAdvance: boolean;
  onNext: () => void;
  onSkip: () => void;
}

type TutorialTarget = 'movement' | 'field' | 'network' | 'upgrade' | null;

const STEP_CONFIG: Record<TutorialStep, { title: TranslationKey; body: TranslationKey; target: TutorialTarget; action?: TranslationKey }> = {
  0: { title: 'tutorialWelcomeTitle', body: 'tutorialWelcomeBody', target: null },
  1: { title: 'tutorialMoveTitle', body: 'tutorialMoveBody', target: 'movement', action: 'tutorialMoveDone' },
  2: { title: 'tutorialPlaceTitle', body: 'tutorialPlaceBody', target: 'field', action: 'tutorialPlaceDone' },
  3: { title: 'tutorialNetworkTitle', body: 'tutorialNetworkBody', target: 'network' },
  4: { title: 'tutorialUpgradeTitle', body: 'tutorialUpgradeBody', target: 'upgrade' },
  5: { title: 'tutorialDoneTitle', body: 'tutorialDoneBody', target: null },
};

function clamp(value: number, min: number, max: number): number {
  return Math.min(max, Math.max(min, value));
}

export default function TutorialOverlay({
  step,
  lang,
  t,
  handedness,
  canAdvance,
  onNext,
  onSkip,
}: TutorialOverlayProps) {
  const config = STEP_CONFIG[step];
  const [targetRect, setTargetRect] = useState<DOMRect | null>(null);

  useEffect(() => {
    const update = () => {
      if (!config.target) {
        setTargetRect(null);
        return;
      }
      const node = document.querySelector<HTMLElement>(`[data-tutorial-target="${config.target}"]`);
      setTargetRect(node?.getBoundingClientRect() ?? null);
    };
    update();
    const id = window.setInterval(update, 120);
    window.addEventListener('resize', update);
    window.addEventListener('scroll', update, true);
    return () => {
      window.clearInterval(id);
      window.removeEventListener('resize', update);
      window.removeEventListener('scroll', update, true);
    };
  }, [config.target, handedness, step]);

  const cardStyle = useMemo(() => {
    const width = Math.min(340, window.innerWidth - 24);
    if (!targetRect) {
      return {
        width: `${width}px`,
        left: `${Math.max(12, (window.innerWidth - width) / 2)}px`,
        top: '50%',
        transform: 'translateY(-50%)',
      } as const;
    }
    const above = targetRect.top > window.innerHeight * 0.48;
    const top = above ? targetRect.top - 170 : targetRect.bottom + 14;
    return {
      width: `${width}px`,
      left: `${clamp(targetRect.left + targetRect.width / 2 - width / 2, 12, window.innerWidth - width - 12)}px`,
      top: `${clamp(top, 12, window.innerHeight - 188)}px`,
    } as const;
  }, [targetRect]);

  const cutoutStyle = targetRect
    ? {
        left: Math.max(4, targetRect.left - 8),
        top: Math.max(4, targetRect.top - 8),
        width: targetRect.width + 16,
        height: targetRect.height + 16,
      }
    : null;

  const ru = lang === 'ru';

  return (
    <div className="absolute inset-0 z-[80] pointer-events-none" aria-live="polite">
      {cutoutStyle && (
        <>
          <div
            className="absolute rounded-xl border-2 border-[#63e6ff] shadow-[0_0_24px_rgba(57,216,255,.28)]"
            style={{
              left: cutoutStyle.left,
              top: cutoutStyle.top,
              width: cutoutStyle.width,
              height: cutoutStyle.height,
              boxShadow: '0 0 0 9999px rgba(0,0,0,.72), 0 0 24px rgba(57,216,255,.30)',
            }}
          />
          <div
            className="absolute rounded-xl border border-[#dcecff]/30 animate-pulse"
            style={{
              left: cutoutStyle.left - 3,
              top: cutoutStyle.top - 3,
              width: cutoutStyle.width + 6,
              height: cutoutStyle.height + 6,
            }}
          />
        </>
      )}

      <div
        className="absolute rounded-2xl border border-[#63e6ff]/30 bg-[#030a14]/95 px-4 py-4 text-[#dcecff] shadow-[0_16px_50px_rgba(0,0,0,.55)] pointer-events-auto"
        style={cardStyle}
      >
        <div className="text-[9px] uppercase tracking-[.22em] font-black text-[#63e6ff]">
          {ru ? `ОБУЧЕНИЕ · ${step + 1}/6` : `TUTORIAL · ${step + 1}/6`}
        </div>
        <div className="mt-1 text-lg font-bold">{t(config.title)}</div>
        <div className="mt-2 text-xs leading-relaxed text-[#a9c0d2]">{t(config.body)}</div>

        {config.action && step !== 4 && (
          <div className={`mt-3 rounded-lg border px-3 py-2 text-[10px] font-bold ${canAdvance ? 'border-[#55e6c1]/40 bg-[#55e6c1]/10 text-[#b9ffe8]' : 'border-[#243b55] bg-[#09121f] text-[#7089a0]'}`}>
            {canAdvance ? t(config.action) : (ru ? 'Выполни действие, чтобы продолжить.' : 'Perform the action to continue.')}
          </div>
        )}

        {step === 4 && (
          <div className="mt-3 rounded-lg border border-[#b68cff]/30 bg-[#b68cff]/10 px-3 py-2 text-[10px] font-bold text-[#dbcfff]">
            {ru ? 'Выбери одну карту. Обучение продолжится после выбора.' : 'Choose one card. The tutorial continues after your choice.'}
          </div>
        )}

        <div className="mt-4 flex items-center justify-between gap-2">
          <button
            type="button"
            onClick={onSkip}
            className="rounded-lg px-2 py-2 text-[9px] font-bold uppercase tracking-wider text-[#718aa1] pointer-events-auto"
          >
            {t('tutorialSkip')}
          </button>
          {step !== 1 && step !== 2 && step !== 4 ? (
            <button
              type="button"
              onClick={onNext}
              disabled={!canAdvance}
              className="rounded-xl border border-[#63e6ff]/40 bg-[#63e6ff]/10 px-4 py-2.5 text-[10px] font-black uppercase tracking-wider text-[#dffaff] disabled:opacity-30"
            >
              {t('tutorialNext')}
            </button>
          ) : (
            step === 1 || step === 2 ? (
              <button
                type="button"
                onClick={onNext}
                disabled={!canAdvance}
                className="rounded-xl border border-[#63e6ff]/40 bg-[#63e6ff]/10 px-4 py-2.5 text-[10px] font-black uppercase tracking-wider text-[#dffaff] disabled:opacity-30"
              >
                {t('tutorialNext')}
              </button>
            ) : null
          )}
        </div>
      </div>
    </div>
  );
}
