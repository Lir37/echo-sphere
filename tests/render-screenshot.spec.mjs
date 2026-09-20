import { test, expect } from '@playwright/test';
import fs from 'node:fs/promises';

test.setTimeout(120_000);

test('capture the actual rendered game after pressing Play', async ({ page }, testInfo) => {
  const consoleErrors = [];
  page.on('console', (message) => {
    if (message.type() === 'error') consoleErrors.push(message.text());
  });

  const pageErrors = [];
  page.on('pageerror', (error) => pageErrors.push(error.message));

  const failedRequests = [];
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

  await page.waitForTimeout(15_000);

  const renderMetrics = await canvas.evaluate((element) => ({
    width: element.width,
    height: element.height,
    renderActive: element.width > 0 && element.height > 0,
  }));

  const assetErrors = await page.evaluate(
    () => window.__ECHO3D_LOAD_ERRORS || [],
  );

  await fs.mkdir('test-results', { recursive: true });
  await fs.writeFile(
    'test-results/render-metrics.json',
    JSON.stringify({
      renderMetrics,
      assetErrors,
      consoleErrors,
      pageErrors,
      failedRequests,
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

  expect(renderMetrics.renderActive).toBeTruthy();
  expect(assetErrors, '3D asset loading errors').toEqual([]);
  expect(consoleErrors, 'Browser console errors').toEqual([]);
  expect(pageErrors, 'Unhandled page errors').toEqual([]);
  expect(failedRequests, 'Failed network requests').toEqual([]);
});
