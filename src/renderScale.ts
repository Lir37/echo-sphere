export const DPR_CAP = 1.75;

export interface CanvasRenderViewport {
  cssWidth: number;
  cssHeight: number;
  backingWidth: number;
  backingHeight: number;
  dpr: number;
}

export function clampDpr(value: number): number {
  return Math.min(DPR_CAP, Math.max(1, Number.isFinite(value) ? value : 1));
}

export function calculateBackingSize(cssWidth: number, cssHeight: number, dpr: number) {
  const safeDpr = clampDpr(dpr);
  return {
    width: Math.max(1, Math.round(cssWidth * safeDpr)),
    height: Math.max(1, Math.round(cssHeight * safeDpr)),
    dpr: safeDpr,
  };
}

export function configureCanvasResolution(canvas: HTMLCanvasElement): CanvasRenderViewport {
  const cssWidth = Math.max(1, canvas.clientWidth);
  const cssHeight = Math.max(1, canvas.clientHeight);
  const { width, height, dpr } = calculateBackingSize(cssWidth, cssHeight, window.devicePixelRatio || 1);
  if (canvas.width !== width) canvas.width = width;
  if (canvas.height !== height) canvas.height = height;
  return { cssWidth, cssHeight, backingWidth: width, backingHeight: height, dpr };
}

export function installCanvasResolutionPolicy(canvas: HTMLCanvasElement): () => void {
  const apply = () => configureCanvasResolution(canvas);
  apply();
  window.addEventListener('resize', apply, { passive: true });
  let media = window.matchMedia(`(resolution: ${window.devicePixelRatio || 1}dppx)`);
  const onResolutionChange = () => {
    media.removeEventListener('change', onResolutionChange);
    apply();
    media = window.matchMedia(`(resolution: ${window.devicePixelRatio || 1}dppx)`);
    media.addEventListener('change', onResolutionChange);
  };
  media.addEventListener('change', onResolutionChange);
  return () => {
    window.removeEventListener('resize', apply);
    media.removeEventListener('change', onResolutionChange);
  };
}

export function getRenderViewport(canvas: HTMLCanvasElement, backingWidth: number, backingHeight: number): CanvasRenderViewport {
  const cssWidth = Math.max(1, canvas.clientWidth || backingWidth);
  const cssHeight = Math.max(1, canvas.clientHeight || backingHeight);
  const dpr = cssWidth > 0 ? backingWidth / cssWidth : 1;
  return { cssWidth, cssHeight, backingWidth, backingHeight, dpr: clampDpr(dpr) };
}
