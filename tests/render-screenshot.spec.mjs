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
    const gl = canvas.getContext('webgl');
    return {
      width: canvas.width,
      height: canvas.height,
      renderActive: Boolean(gl && gl.getError() === gl.NO_ERROR && canvas.width > 0 && canvas.height > 0),
      webgl: Boolean(gl),
      glError: gl ? gl.getError() : null,
    };
  });

  await fs.mkdir('test-results', { recursive: true });
  await page.screenshot({
    path: 'test-results/echo-sphere-render.png',
    fullPage: false,
  });

  const screenshotStat = await fs.stat('test-results/echo-sphere-render.png');
  renderMetrics.screenshotBytes = screenshotStat.size;
  renderMetrics.renderActive = renderMetrics.renderActive && screenshotStat.size > 10_000;

  await fs.writeFile(
    'test-results/render-metrics.json',
    JSON.stringify({ renderMetrics, consoleErrors, pageErrors }, null, 2),
    'utf8',
  );

  await testInfo.attach('echo-sphere-render', {
    path: 'test-results/echo-sphere-render.png',
    contentType: 'image/png',
  });

  expect(renderMetrics.renderActive).toBeTruthy();
  expect(consoleErrors, 'Browser console errors').toEqual([]);
  expect(pageErrors, 'Unhandled page errors').toEqual([]);
});
