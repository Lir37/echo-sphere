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
  const playButton = page.getByRole('button', { name: /Играть|Play|START RUN/i }).first();
  await expect(playButton).toBeVisible();
  await playButton.click();

  const canvas = page.locator('canvas').first();
  await expect(canvas).toBeVisible();

  // Give the game loop five seconds to spawn/render actual gameplay.
  await page.waitForTimeout(5_000);

  const renderMetrics = await canvas.evaluate((element) => {
    const canvas = element;
    const ctx = canvas.getContext('2d', { willReadFrequently: true });
    if (!ctx) return { width: canvas.width, height: canvas.height, nonBlackPixels: 0, renderActive: false };

    const width = canvas.width;
    const height = canvas.height;
    const sample = ctx.getImageData(0, 0, width, height);
    let nonBlackPixels = 0;

    // Sample every 8th pixel. This is intentionally lightweight but proves
    // that the canvas contains rendered frame data, not just an empty surface.
    for (let i = 0; i < sample.data.length; i += 4 * 8) {
      const r = sample.data[i];
      const g = sample.data[i + 1];
      const b = sample.data[i + 2];
      if (r + g + b > 12) nonBlackPixels++;
    }

    return {
      width,
      height,
      nonBlackPixels,
      renderActive: nonBlackPixels > 100,
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
