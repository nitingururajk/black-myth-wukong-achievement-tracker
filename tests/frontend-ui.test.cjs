const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const { JSDOM } = require('jsdom');
const root = path.join(__dirname, '../bmw_web/wwwroot');
const example = JSON.parse(fs.readFileSync(path.join(root, 'example-report.json'), 'utf8'));
function setup(t, storedLanguage = 'en', { storageBlocked = false } = {}) {
  const dom = new JSDOM(fs.readFileSync(path.join(root, 'index.html'), 'utf8'), { url: 'https://test.invalid', runScripts: 'outside-only' });
  t.after(() => { dom.window.close(); global.gc?.(); });
  const w = dom.window;
  w.scrollTo = () => {};
  w.matchMedia = () => ({ matches: true });
  w.HTMLElement.prototype.scrollIntoView = () => {};
  if (storageBlocked) {
    Object.defineProperty(w, 'localStorage', { get() { throw new w.DOMException('Storage blocked', 'SecurityError'); } });
  } else {
    w.localStorage.setItem('journey-ledger.language', storedLanguage);
  }
  const context = dom.getInternalVMContext();
  const run = source => vm.runInContext(source, context);
  for (const script of w.document.querySelectorAll('script[src]')) {
    vm.runInContext(fs.readFileSync(path.join(root, new URL(script.src).pathname), 'utf8'), context);
  }
  w.fixture = structuredClone(example.report);
  const switchLanguage = value => {
    const select = w.document.getElementById('languageSelect');
    select.value = value;
    select.dispatchEvent(new w.Event('change', { bubbles: true }));
  };
  const report = () => run('showReport(fixture, {fileName: "example.sav", example: true})');
  return { w, d: w.document, run, switchLanguage, report };
}


test('initial language selector is native, labeled, visible and keyboard-focusable', t => {
  const { d } = setup(t);
  const select = d.getElementById('languageSelect');
  assert.ok(select);
  assert.equal(select.tagName, 'SELECT');
  assert.equal(select.tabIndex, 0);
  assert.notEqual(select.getAttribute('aria-hidden'), 'true');
  assert.ok(d.querySelector('label[for="languageSelect"]'));
  assert.equal(d.getElementById('languageMenu'), null);
});
test('stored language translates new landing UI and can return to English', t => {
  const { d, switchLanguage } = setup(t, 'zh-CN');
  assert.equal(d.documentElement.lang, 'zh-CN');
  assert.match(d.getElementById('pageTitle').textContent, /成就/);
  assert.match(d.getElementById('demoBtn').textContent, /示例/);
  switchLanguage('en');
  assert.equal(d.documentElement.lang, 'en');
  assert.match(d.getElementById('pageTitle').textContent, /Achievement Tracker/);
});
test('real repository example renders all 81 achievements; translations cover catalog', t => {
  const { d, report, switchLanguage, run } = setup(t);
  report(); switchLanguage('zh-CN');
  assert.equal(d.querySelectorAll('.achievement-card').length, 81);
  assert.equal(run('fixture.achievements.every(item => zhGuides[item.achievementId])'), true);
  assert.equal(d.getElementById('demoBanner').hidden, false);
  assert.match(d.getElementById('reportFile').textContent, /示例/);
  assert.match(d.getElementById('demoBanner').textContent, /示例/);
});
test('language round-trip preserves active tab, filters, search, guides and missing-item groups', t => {
  const { d, report, run, switchLanguage } = setup(t);
  report();
  run('switchView("achievements"); categoryFilter.value=fixture.achievements[0].category; renderAchievementLibrary();');
  const category = d.getElementById('categoryFilter').value;
  const guide = d.querySelector('details[data-guide-id]');
  guide.open = true;
  const id = guide.dataset.guideId;
  const collection = d.querySelector('details[data-collection-id]');
  collection.open = true;
  const collectionId = collection.dataset.collectionId;
  switchLanguage('zh-CN'); switchLanguage('en');
  assert.equal(d.getElementById('tab-achievements').getAttribute('aria-selected'), 'true');
  assert.equal(d.getElementById('categoryFilter').value, category);
  assert.equal(d.querySelector(`[data-guide-id="${id}"]`).open, true);
  assert.equal(d.querySelector(`[data-collection-id="${collectionId}"]`).open, true);
});
test('Chinese and English achievement searches work without exposing spoiler content', t => {
  const { d, report, run, switchLanguage } = setup(t);
  report(); switchLanguage('zh-CN');
  run('searchInput.value=zhGuides[81002].displayTitle;renderAchievementLibrary()');
  assert.equal(d.querySelectorAll('.achievement-card').length, 1);
  run('searchInput.value=fixture.achievements.find(x=>x.achievementId===81002).displayTitle;renderAchievementLibrary()');
  assert.equal(d.querySelectorAll('.achievement-card').length, 1);
  run('hideSpoilers=true;revealedGuideIds.clear()');
  assert.equal(run('buildSearchText(fixture.achievements.find(x=>x.achievementId===81002)).includes(zhGuides[81002].guideSteps[0])'), false);
});
test('missing-item search supports translated target names and respects spoiler protection', t => {
  const { d, report, run, switchLanguage } = setup(t);
  report(); switchLanguage('zh-CN');
  run('var entry=getTrackedCollections(currentReport).find(({item,missing})=>missing.some(target=>zhTargets[`${item.achievementId}:${target.id}`])); var target=entry.missing.find(target=>zhTargets[`${entry.item.achievementId}:${target.id}`]); itemSearchInput.value=targetText(entry.item,target,"name");renderTracker(currentReport)');
  assert.ok(d.querySelectorAll('.tracker-item').length > 0);
  run('hideSpoilers=true;revealedGuideIds.clear();renderTracker(currentReport)');
  assert.equal(d.querySelectorAll('.tracker-item').length, 0);
});
test('filename and player-name text remain safe and untranslated', t => {
  const { d, run, switchLanguage } = setup(t);
  run('fixture.playerName="<img src=x onerror=alert(1)>";showReport(fixture,{fileName:"<b>save.sav</b>",example:false})');
  switchLanguage('zh-CN');
  assert.equal(d.querySelector('#playerContext img'), null);
  assert.equal(d.querySelector('#reportFile b'), null);
  assert.match(d.getElementById('reportFile').textContent, /<b>save.sav<\/b>/);
});
