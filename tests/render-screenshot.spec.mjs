import { test, expect } from '@playwright/test';
import fs from 'node:fs/promises';

test('capture the actual rendered game after pressing Play', async ({ page }, testInfo) => {
  const consoleErrors = [];
  page.on('console', (message) => {
    if (message.type() === 'error') consoleErrors.push(message.text());
  });

  const pageErrors = [];
  page.on('pageerror', (error) => pageErrors.push(error.message));

  await page.goto('/', { waitUntil: 'networkidle' });

  // CI runs with an English browser locale, but support both translations so
  // the test follows the real UI rather than bypassing the menu.
  const playButton = page.locator('button.es-main-play').first();
  await expect(playButton).toBeVisible();
  await playButton.click();

  const canvas = page.locator('canvas').first();
  await expect(canvas).toBeVisible();

  // Give the game loop five seconds to spawn/render actual gameplay.
  await page.waitForTimeout(5_000);

  const renderMetrics = await canvas.evaluate((element) => {
    const canvas = element;
    const gl = canvas.getContext('webgl', { preserveDrawingBuffer: true });
    if (!gl) return { width: canvas.width, height: canvas.height, nonBlackPixels: 0, renderActive: false, context: 'missing' };

    const width = canvas.width;
    const height = canvas.height;
    const sampleSize = Math.min(width * height, 320_000);
    const readWidth = Math.min(width, Math.max(1, Math.floor(Math.sqrt(sampleSize * width / Math.max(1, height)))));
    const readHeight = Math.min(height, Math.max(1, Math.floor(sampleSize / readWidth)));
    const buffer = new Uint8Array(readWidth * readHeight * 4);
    gl.readPixels(0, 0, readWidth, readHeight, gl.RGBA, gl.UNSIGNED_BYTE, buffer);

    let nonBlackPixels = 0;
    for (let i = 0; i < buffer.length; i += 4 * 8) {
      const r = buffer[i];
      const g = buffer[i + 1];
      const b = buffer[i + 2];
      if (r + g + b > 12) nonBlackPixels++;
    }

    return {
      width,
      height,
      nonBlackPixels,
      renderActive: nonBlackPixels > 100,
      context: 'webgl',
    };
  });

  await fs.mkdir('test-results', { recursive: true });
  await fs.writeFile(
    'test-results/render-metrics.json',
    JSON.stringify({ renderMetrics, consoleErrors, pageErrors }, null, 2),
    'utf8',
  );

  await page.screenshot({
    path: 'test-results/echo-sphere-render.png',
    fullPage: false,
  });

  await testInfo.attach('echo-sphere-render', {
    path: 'test-results/echo-sphere-render.png',
    contentType: 'image/png',
  });

  expect(renderMetrics.renderActive).toBeTruthy();
  expect(consoleErrors, 'Browser console errors').toEqual([]);
  expect(pageErrors, 'Unhandled page errors').toEqual([]);
});
