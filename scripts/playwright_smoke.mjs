import { chromium } from 'playwright';

const browser = await chromium.launch({ headless: true });
const page = await browser.newPage({ viewport: { width: 1440, height: 900 }, deviceScaleFactor: 1 });
await page.goto('http://127.0.0.1:4173', { waitUntil: 'networkidle' });

const buttons = page.getByRole('button');
const texts = ['Играть', 'Play', 'Начать', 'Start'];
let clicked = false;
for (const text of texts) {
  const b = buttons.filter({ hasText: text }).first();
  if (await b.count() && await b.isVisible().catch(() => false)) {
    await b.click();
    clicked = true;
    break;
  }
}
if (!clicked) {
  const candidates = page.locator('button');
  const count = await candidates.count();
  for (let i = 0; i < count; i++) {
    const b = candidates.nth(i);
    if (await b.isVisible().catch(() => false)) {
      const label = (await b.innerText().catch(() => '')).trim().toLowerCase();
      if (label.includes('игр') || label.includes('play') || label.includes('start') || label.includes('нач')) {
        await b.click();
        clicked = true;
        break;
      }
    }
  }
}

if (!clicked) {
  throw new Error('Play button was not found on the built app');
}

await page.waitForTimeout(5000);
await page.screenshot({ path: 'artifacts/echo-sphere-play.png', fullPage: true });
await browser.close();
