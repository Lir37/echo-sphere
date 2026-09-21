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

  await page.goto(process.env.BASE_URL || 'http://127.0.0.1:4173', { waitUntil: 'networkidle' });

  const playButton = page.getByRole('button', { name: /Играть|Play|START RUN/i }).first();
  await expect(playButton).toBeVisible();
  await playButton.click();

  const canvas = page.locator('canvas').first();
  await expect(canvas).toBeVisible();

  // Place a real tower early enough that later enemy movement cannot make the
  // placement invalid. The point is deliberately away from the left joystick
  // and right-side action controls, and projects to ~77 world units from the
  // player, beyond the placement exclusion radius.
  await page.waitForTimeout(1_500);

  const box = await canvas.boundingBox();
  expect(box).not.toBeNull();
  await page.mouse.click(box.x + box.width * 0.50, box.y + box.height * 0.70);

  await page.waitForTimeout(13_500);

  const runtime = await canvas.evaluate((element) => ({
    width: element.width,
    height: element.height,
    stats: window.__ECHO3D_STATS || null,
    assetErrors: window.__ECHO3D_LOAD_ERRORS || [],
  }));

  const metricsPath = testInfo.outputPath('render-metrics.json');
  const screenshotPath = testInfo.outputPath('echo-sphere-render.png');
  await fs.mkdir(testInfo.outputDir, { recursive: true });
  await fs.writeFile(
    metricsPath,
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
    path: screenshotPath,
    fullPage: false,
  });

  await testInfo.attach('echo-sphere-render', {
    path: screenshotPath,
    contentType: 'image/png',
  });

  await testInfo.attach('render-metrics', {
    path: metricsPath,
    contentType: 'application/json',
  });

  expect(runtime.width).toBeGreaterThan(0);
  expect(runtime.height).toBeGreaterThan(0);
  expect(runtime.stats?.drawCalls || 0, 'no 3D draw calls').toBeGreaterThan(0);
  expect(runtime.stats?.triangles || 0, 'no rendered triangles').toBeGreaterThan(1000);
  expect(runtime.stats?.players || 0, 'player was not rendered').toBeGreaterThanOrEqual(1);
  expect(runtime.stats?.spheres || 0, 'tower was not rendered').toBeGreaterThanOrEqual(1);
  expect(runtime.stats?.enemies || 0, 'enemy was not rendered').toBeGreaterThanOrEqual(1);
  expect(runtime.assetErrors, '3D asset loading errors').toEqual([]);
  expect(consoleErrors, 'Browser console errors').toEqual([]);
  expect(pageErrors, 'Unhandled page errors').toEqual([]);
  expect(failedRequests, 'Failed network requests').toEqual([]);
});
