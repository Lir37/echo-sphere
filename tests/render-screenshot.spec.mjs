import { test, expect } from '@playwright/test';
import fs from 'node:fs/promises';

test('capture the actual rendered menu, stage selector and game', async ({ page }, testInfo) => {
  const consoleErrors = [];
  page.on('console', (message) => {
    if (message.type() === 'error') consoleErrors.push(message.text());
  });
  const pageErrors = [];
  page.on('pageerror', (error) => pageErrors.push(error.message));

  await page.goto('/', { waitUntil: 'networkidle' });

  const menu = page.locator('.es-main-menu-legacy');
  await expect(menu).toBeVisible();
  const menuEntry = page.locator('button.es-main-map-entry').first();
  await expect(menuEntry).toBeVisible();

  await fs.mkdir('test-results', { recursive: true });
  await page.screenshot({
    path: 'test-results/echo-sphere-menu.png',
    fullPage: false,
  });
  const menuStat = await fs.stat('test-results/echo-sphere-menu.png');
  expect(menuStat.size).toBeGreaterThan(10_000);
  await testInfo.attach('echo-sphere-menu', {
    path: 'test-results/echo-sphere-menu.png',
    contentType: 'image/png',
  });

  await menuEntry.click();

  const stageSelector = page.locator('.es-stage-select');
  await expect(stageSelector).toBeVisible();
  await expect(page.locator('.es-region-card.is-available').first()).toBeVisible();
  await page.screenshot({
    path: 'test-results/echo-sphere-stage-select.png',
    fullPage: false,
  });
  const stageStat = await fs.stat('test-results/echo-sphere-stage-select.png');
  expect(stageStat.size).toBeGreaterThan(10_000);
  await testInfo.attach('echo-sphere-stage-select', {
    path: 'test-results/echo-sphere-stage-select.png',
    contentType: 'image/png',
  });

  await page.locator('.es-region-card.is-available .es-region-primary-run').first().click();

  const canvas = page.locator('canvas').first();
  await expect(canvas).toBeVisible();

  const followButton = page.locator('[data-game-control="formation-follow"]').first();
  await expect(followButton).toBeVisible();
  await expect(followButton).toHaveAttribute('aria-pressed', 'false');
  await followButton.dispatchEvent('pointerdown');
  await expect(followButton).toHaveAttribute('aria-pressed', 'true');

  await page.waitForTimeout(5_000);

  const renderMetrics = await canvas.evaluate((element) => {
    const canvas = element;
    const ctx = canvas.getContext('2d');
    const sampleWidth = Math.min(canvas.width, 16);
    const sampleHeight = Math.min(canvas.height, 16);
    const pixels = ctx && sampleWidth > 0 && sampleHeight > 0
      ? ctx.getImageData(0, 0, sampleWidth, sampleHeight).data
      : null;
    const hasRenderedPixels = Boolean(pixels && Array.from(pixels).some((value) => value !== 0));
    return {
      width: canvas.width,
      height: canvas.height,
      rendererMode: ctx ? '2d' : 'unknown',
      renderActive: Boolean(ctx && canvas.width > 0 && canvas.height > 0 && hasRenderedPixels),
    };
  });

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
