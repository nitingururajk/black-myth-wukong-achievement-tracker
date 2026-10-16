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


test('language changes do not cancel an upload or clear a selected file', t => {
  const { d, run, switchLanguage } = setup(t);
  run('chooseFile(new File(["data"], "test.sav"));activeRequest=new AbortController();setAnalyzeButton(true);setStatus("Reading your save and checking all 81 achievements…")');
  switchLanguage('zh-CN');
  assert.equal(run('activeRequest.signal.aborted'), false);
  assert.equal(run('selectedFile.name'), 'test.sav');
  assert.equal(d.getElementById('analyzeBtn').disabled, true);
  assert.match(d.getElementById('statusPanel').textContent, /存档/);
});
test('upload validation, status round-trip and reset stay functional', t => {
  const { d, run, report, switchLanguage } = setup(t);
  run('chooseFile(new File([], "test.sav"))');
  switchLanguage('zh-CN');
  assert.match(d.getElementById('statusPanel').textContent, /空/);
  switchLanguage('en');
  assert.match(d.getElementById('statusPanel').textContent, /empty/);
  report(); switchLanguage('zh-CN'); run('resetReport()');
  assert.equal(d.getElementById('landing').hidden, false);
  assert.equal(d.getElementById('analyzeBtn').disabled, true);
  assert.match(d.getElementById('dropTitle').textContent, /存档/);
});
test('errors originating in Chinese also return to English', t => {
  const { d, run, switchLanguage } = setup(t, 'zh-CN');
  run('chooseFile(new File([], "test.sav"))');
  assert.match(d.getElementById('statusPanel').textContent, /空/);
  switchLanguage('en');
  assert.match(d.getElementById('statusPanel').textContent, /empty/);
});
test('example targets retain stable IDs and all known targets translate', t => {
  const { run } = setup(t, 'zh-CN');
  assert.equal(run('fixture.achievements.every(item => [...(item.requirementTargets ?? []), ...(item.missingTargets ?? [])].every(target => Number.isInteger(target.id) && zhTargets[`${item.achievementId}:${target.id}`]))'), true);
});
test('language changes keep the report data and New Game Plus count intact', t => {
  const { d, run, report, switchLanguage } = setup(t);
  run('fixture.newGamePlusCount=2');
  const original = run('JSON.stringify(fixture)');
  report(); switchLanguage('zh-CN'); switchLanguage('en');
  assert.equal(run('JSON.stringify(fixture)'), original);
  assert.equal(d.getElementById('ovNgPlus').textContent, 'NG+ 2');
});
test('checklist exports selected language while retaining spoiler restrictions', t => {
  const { w, run, report, switchLanguage } = setup(t);
  let blob;
  w.URL.createObjectURL = value => { blob = value; return 'blob:test'; };
  w.URL.revokeObjectURL = () => {};
  w.HTMLAnchorElement.prototype.click = () => {};
  w.Blob = class { constructor(parts) { this.text = parts.join(''); } };
  report(); switchLanguage('zh-CN'); run('downloadChecklist()');
  assert.match(blob.text, /示例/);
  assert.match(blob.text, /存档/);
  assert.doesNotMatch(blob.text, /EXAMPLE CHECKLIST/);
  run('hideSpoilers=true;downloadChecklist()');
  assert.match(blob.text, /剧透/);
});
