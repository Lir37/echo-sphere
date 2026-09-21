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

  // The menu/start transition consumes a few seconds; capture after roughly
  // five seconds of actual gameplay rather than five seconds after the Play click.
  await page.waitForTimeout(11_000);

  // Exercise the real mobile placement path: one tap creates a tower exactly at
  // the tapped world position. The follow-up tap on the same point must toggle it
  // back out instead of spawning/removing a random tower elsewhere.
  const box = await canvas.boundingBox();
  expect(box).not.toBeNull();
  const placementPoint = {
    x: box.x + box.width * 0.68,
    y: box.y + box.height * 0.52,
  };
  await page.mouse.click(placementPoint.x, placementPoint.y);
  await page.waitForTimeout(700);

  const placedMetrics = await canvas.evaluate((element) => ({
    width: element.width,
    height: element.height,
    stats: window.__ECHO3D_STATS || null,
  }));
  expect(placedMetrics.stats?.visibleEntities || 0, 'tower was not rendered after tap').toBeGreaterThanOrEqual(2);
  await page.screenshot({
    path: 'test-results/echo-sphere-render.png',
    fullPage: false,
  });
  await testInfo.attach('echo-sphere-render', {
    path: 'test-results/echo-sphere-render.png',
    contentType: 'image/png',
  });

  const placedVisibleEntities = placedMetrics.stats?.visibleEntities || 0;
  await page.mouse.click(placementPoint.x, placementPoint.y);
  await page.waitForTimeout(250);

  const removedMetrics = await canvas.evaluate(() => window.__ECHO3D_STATS || null);
  expect(removedMetrics?.visibleEntities || 0, 'tower did not disappear after tapping it again')
    .toBeLessThan(placedVisibleEntities);

  const renderMetrics = await canvas.evaluate((element) => ({
    width: element.width,
    height: element.height,
    renderActive: element.width > 0 && element.height > 0,
    stats: window.__ECHO3D_STATS || null,
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
    path: 'test-results/echo-sphere-after-removal.png',
    fullPage: false,
  });

  await testInfo.attach('echo-sphere-after-removal', {
    path: 'test-results/echo-sphere-after-removal.png',
    contentType: 'image/png',
  });

  expect(renderMetrics.renderActive).toBeTruthy();
  expect(renderMetrics.stats?.triangles || 0, '3D renderer produced no geometry').toBeGreaterThan(5000);
  expect(renderMetrics.stats?.visibleEntities || 0, '3D entities are not visible').toBeGreaterThan(0);
  expect(assetErrors, '3D asset loading errors').toEqual([]);
  expect(consoleErrors, 'Browser console errors').toEqual([]);
  expect(pageErrors, 'Unhandled page errors').toEqual([]);
  expect(failedRequests, 'Failed network requests').toEqual([]);
});
