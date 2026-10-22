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

// Controlled fetches exercise real request handlers; responses use the checked-in
// example, not a claimed successful decode of a real game save.
function controlledFetch(w) {
  const requests = [];
  w.fetch = (url, options) => new Promise((resolve, reject) => requests.push({ url, options, resolve, reject }));
  return requests;
}
function reportResponse() {
  return { ok: true, status: 200, text: async () => JSON.stringify({ ...example, saveFileName: 'test.sav' }) };
}
test('cancelling an example immediately resets its controls without waiting for fetch', async t => {
  const { w, d, run } = setup(t);
  const requests = controlledFetch(w);
  const first = run('loadExample()');
  assert.equal(d.getElementById('demoBtn').disabled, true);
  run('chooseFile(new File(["test"], "test.sav"))');
  assert.equal(requests[0].options.signal.aborted, true);
  assert.equal(d.getElementById('demoBtn').disabled, false);
  assert.doesNotMatch(d.getElementById('demoBtn').textContent, /Loading/);
  requests[0].resolve(reportResponse()); await first;
  assert.equal(d.getElementById('landing').hidden, false);
});
test('a stale example response cannot clear the loading state of a newer request', async t => {
  const { w, d, run, switchLanguage } = setup(t);
  const requests = controlledFetch(w);
  const first = run('loadExample()');
  run('chooseFile(new File(["test"], "test.sav"))');
  const second = run('loadExample()');
  requests[0].resolve(reportResponse()); await first;
  assert.equal(d.getElementById('demoBtn').disabled, true);
  switchLanguage('zh-CN');
  assert.match(d.getElementById('demoBtn').textContent, /加载/);
  requests[1].resolve(reportResponse()); await second;
  assert.match(d.getElementById('reportFile').textContent, /示例/);
});
test('starting an example clears obsolete upload feedback', async t => {
  const { w, d, run } = setup(t);
  const requests = controlledFetch(w);
  run('setStatus("That file is empty. Choose your game’s .sav file.", "error")');
  const pending = run('loadExample()');
  assert.equal(d.getElementById('statusPanel').classList.contains('hidden'), true);
  requests[0].resolve(reportResponse()); await pending;
});
test('late upload success cannot overwrite a subsequently selected file', async t => {
  const { w, d, run } = setup(t);
  const requests = controlledFetch(w);
  run('chooseFile(new File(["test"], "old.sav"))');
  const pending = run('analyzeSave({preventDefault(){}})');
  run('chooseFile(new File(["new"], "new.sav"))');
  requests[0].resolve(reportResponse()); await pending;
  assert.equal(d.getElementById('landing').hidden, false);
  assert.equal(d.getElementById('dropTitle').textContent, 'new.sav');
  assert.equal(d.getElementById('analyzeBtn').disabled, false);
});
test('upload completes correctly after a mid-request language switch', async t => {
  const { w, d, run, switchLanguage } = setup(t);
  const requests = controlledFetch(w);
  run('chooseFile(new File(["test"], "test.sav"))');
  const pending = run('analyzeSave({preventDefault(){}})');
  switchLanguage('zh-CN');
  requests[0].resolve(reportResponse()); await pending;
  assert.equal(d.getElementById('landing').hidden, true);
  assert.equal(d.getElementById('demoBanner').hidden, true);
  assert.match(d.getElementById('reportFile').textContent, /存档报告.*test.sav/);
  assert.equal(d.getElementById('uploadForm').getAttribute('aria-busy'), 'false');
});
test('unknown dictionary keys use literal fallback, not Object.prototype members', t => {
  const { run } = setup(t, 'zh-CN');
  for (const key of ['constructor', '__proto__', 'toString', 'unknown message']) {
    assert.equal(run(`t(${JSON.stringify(key)})`), key);
    assert.equal(run(`chapterLabel(${JSON.stringify(key)})`), key);
  }
});
test('blocked localStorage does not prevent language changes or spoiler toggles', t => {
  const { d, report, switchLanguage } = setup(t, 'en', { storageBlocked: true });
  switchLanguage('zh-CN'); report();
  d.getElementById('spoilerToggleBtn').click();
  assert.equal(d.documentElement.lang, 'zh-CN');
  assert.equal(d.getElementById('spoilerToggleBtn').getAttribute('aria-pressed'), 'true');
});
test('completion ceremony and zero-missing state survive repeated language switches', t => {
  const { d, run, report, switchLanguage } = setup(t);
  run('fixture.achievements.forEach(item=>{item.isComplete=true;item.missingTargets=[]});fixture.completedAchievements=81;fixture.incompleteAchievements=0');
  report(); switchLanguage('zh-CN'); switchLanguage('en'); switchLanguage('zh-CN');
  assert.equal(d.getElementById('completionCeremony').hidden, false);
  assert.match(d.getElementById('trackerList').textContent, /完成/);
  assert.equal(d.querySelectorAll('.achievement-card.is-complete').length, 81);
});
