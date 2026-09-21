import { test, expect } from '@playwright/test';
import fs from 'node:fs/promises';

test.setTimeout(120_000);

test('capture the actual rendered game after Play', async ({ page }, testInfo) => {
  const consoleErrors = [];
  const pageErrors = [];
  const failedRequests = [];

  page.on('console', (message) => {
    if (message.type() === 'error') consoleErrors.push(message.text());
  });
  page.on('pageerror', (error) => pageErrors.push(error.message));
  page.on('requestfailed', (request) => {
    failedRequests.push(
      request.method() + ' ' + request.url() + ' :: ' + (request.failure()?.errorText || 'failed'),
    );
  });

  await page.goto('/', { waitUntil: 'networkidle' });

  const playButton = page.getByRole('button', { name: /Играть|Play|START RUN/i }).first();
  await expect(playButton).toBeVisible();
  await playButton.click();

  const canvas = page.locator('canvas').first();
  await expect(canvas).toBeVisible();

  // Gameplay must run before the QA capture. Place one real tower shortly before
  // the 15-second mark so the screenshot contains player + enemies + tower.
  await page.waitForTimeout(13_000);

  const box = await canvas.boundingBox();
  expect(box).not.toBeNull();
  await page.mouse.click(box.x + box.width * 0.68, box.y + box.height * 0.52);

  await page.waitForTimeout(2_000);

  const runtime = await canvas.evaluate((element) => ({
    width: element.width,
    height: element.height,
    stats: window.__ECHO3D_STATS || null,
    assetErrors: window.__ECHO3D_LOAD_ERRORS || [],
  }));

  await fs.mkdir('test-results', { recursive: true });
  await fs.writeFile(
    'test-results/render-metrics.json',
    JSON.stringify({
      runtime,
      consoleErrors,
      pageErrors,
      failedRequests,
      gameplaySeconds: 15,
    }, null, 2),
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

  expect(runtime.width).toBeGreaterThan(0);
  expect(runtime.height).toBeGreaterThan(0);
  expect(runtime.stats?.players || 0, 'player was not rendered').toBeGreaterThanOrEqual(1);
  expect(runtime.stats?.spheres || 0, 'tower was not rendered').toBeGreaterThanOrEqual(1);
  expect(runtime.stats?.enemies || 0, 'enemy was not rendered').toBeGreaterThanOrEqual(1);
  expect(runtime.assetErrors, '3D asset loading errors').toEqual([]);
  expect(consoleErrors, 'Browser console errors').toEqual([]);
  expect(pageErrors, 'Unhandled page errors').toEqual([]);
  expect(failedRequests, 'Failed network requests').toEqual([]);
});
