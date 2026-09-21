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
  await page.waitForTimeout(1_200);

  const box = await canvas.boundingBox();
  expect(box).not.toBeNull();
  for (const [rx, ry] of [[0.50, 0.70], [0.62, 0.66], [0.38, 0.66]]) {
    await page.mouse.click(box.x + box.width * rx, box.y + box.height * ry);
    await page.waitForTimeout(450);
  }

  // Wait for an actually populated gameplay frame instead of guessing from wall-clock
  // time. This keeps the visual gate deterministic when CI startup speed varies.
  await page.waitForFunction(
    () => (window.__ECHO3D_STATS?.enemies || 0) >= 3,
    null,
    { timeout: 15_000 },
  );
  await page.waitForTimeout(900);

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
      gameplaySeconds: 7.8,
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
  expect(runtime.stats?.enemies || 0, 'not enough enemies rendered for gameplay QA').toBeGreaterThanOrEqual(3);
  expect(runtime.assetErrors, '3D asset loading errors').toEqual([]);
  expect(consoleErrors, 'Browser console errors').toEqual([]);
  expect(pageErrors, 'Unhandled page errors').toEqual([]);
  expect(failedRequests, 'Failed network requests').toEqual([]);
});
