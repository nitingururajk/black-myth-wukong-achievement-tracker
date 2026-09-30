const MAX_SAVE_BYTES = 4 * 1024 * 1024;
const EXPECTED_ACHIEVEMENT_COUNT = 81;

const landing = document.getElementById('landing');
const uploadForm = document.getElementById('uploadForm');
const saveFileInput = document.getElementById('saveFile');
const dropZone = document.getElementById('dropZone');
const dropTitle = document.getElementById('dropTitle');
const dropHint = document.getElementById('dropHint');
const selectedFilePanel = document.getElementById('selectedFile');
const analyzeBtn = document.getElementById('analyzeBtn');
const statusPanel = document.getElementById('statusPanel');
const results = document.getElementById('results');
const demoBtn = document.getElementById('demoBtn');
const demoBanner = document.getElementById('demoBanner');
const resetBtn = document.getElementById('resetBtn');
const downloadBtn = document.getElementById('downloadBtn');
const reportTitle = document.getElementById('reportTitle');
const reportFile = document.getElementById('reportFile');
const playerContext = document.getElementById('playerContext');
const completionCeremony = document.getElementById('completionCeremony');
const completionPlayer = document.getElementById('completionPlayer');
const progressArc = document.getElementById('progressArc');
const progressPct = document.getElementById('progressPct');
const overviewNarrative = document.getElementById('overviewNarrative');
const nextStepsList = document.getElementById('nextStepsList');
const missableSummary = document.getElementById('missableSummary');
const trackerCount = document.getElementById('trackerCount');
const trackerList = document.getElementById('trackerList');
const itemSearchInput = document.getElementById('itemSearchInput');
const spoilerToggleBtn = document.getElementById('spoilerToggleBtn');
const searchInput = document.getElementById('searchInput');
const statusFilters = document.getElementById('statusFilters');
const categoryFilter = document.getElementById('categoryFilter');
const chapterFilter = document.getElementById('chapterFilter');
const libraryCount = document.getElementById('libraryCount');
const expandVisibleBtn = document.getElementById('expandVisibleBtn');
const achievementList = document.getElementById('achievementList');
const emptyState = document.getElementById('emptyState');
const clearFiltersBtn = document.getElementById('clearFiltersBtn');
const resultTabs = document.getElementById('resultTabs');
const reportViews = [...document.querySelectorAll('[role="tabpanel"]')];
const saveHelpDialog = document.getElementById('saveHelpDialog');
const closeHelpBtn = document.getElementById('closeHelpBtn');
const helpDoneBtn = document.getElementById('helpDoneBtn');
const copyPathBtn = document.getElementById('copyPathBtn');

let selectedFile = null;
let currentReport = null;
let activeRequest = null;
let currentStatusFilter = 'all';
let hideSpoilers = loadSpoilerPreference();
let isExampleReport = false;
const revealedGuideIds = new Set();
const openGuideIds = new Set();

// Keep upload feedback next to the action that produced it.
uploadForm.append(statusPanel);

uploadForm.addEventListener('submit', analyzeSave);
saveFileInput.addEventListener('change', () => chooseFile(saveFileInput.files?.[0] ?? null));
['dragenter', 'dragover'].forEach((name) => dropZone.addEventListener(name, (event) => {
  event.preventDefault();
  dropZone.classList.add('is-dragging');
}));
['dragleave', 'drop'].forEach((name) => dropZone.addEventListener(name, (event) => {
  event.preventDefault();
  dropZone.classList.remove('is-dragging');
}));
dropZone.addEventListener('drop', (event) => {
  const files = [...(event.dataTransfer?.files ?? [])];
  if (files.length !== 1) {
    chooseFile(null);
    setStatus('Drop one .sav file at a time.', 'error', true);
    return;
  }
  chooseFile(files[0]);
});

resetBtn.addEventListener('click', resetReport);
document.querySelectorAll('[data-reset]').forEach((button) => button.addEventListener('click', resetReport));
demoBtn.addEventListener('click', loadExample);
downloadBtn.addEventListener('click', downloadChecklist);
resultTabs.addEventListener('click', (event) => {
  const tab = event.target.closest('[data-view]');
  if (tab) switchView(tab.dataset.view);
});
resultTabs.addEventListener('keydown', (event) => {
  if (!['ArrowLeft', 'ArrowRight', 'Home', 'End'].includes(event.key)) return;
  const tabs = [...resultTabs.querySelectorAll('[role="tab"]')];
  const index = tabs.indexOf(document.activeElement);
  if (index < 0) return;
  event.preventDefault();
  const nextIndex = event.key === 'Home' ? 0 : event.key === 'End' ? tabs.length - 1
    : (index + (event.key === 'ArrowRight' ? 1 : -1) + tabs.length) % tabs.length;
  switchView(tabs[nextIndex].dataset.view);
  tabs[nextIndex].focus();
});
document.addEventListener('click', (event) => {
  const viewButton = event.target.closest('[data-switch-view]');
  if (viewButton) {
    switchView(viewButton.dataset.switchView);
    document.getElementById(`tab-${viewButton.dataset.switchView}`).focus();
    resultTabs.scrollIntoView({ block: 'start', behavior: scrollBehavior() });
  }
  const guideButton = event.target.closest('[data-open-guide]');
  if (guideButton) openAchievement(Number(guideButton.dataset.openGuide));
});
statusFilters.addEventListener('click', (event) => {
  const button = event.target.closest('[data-status-filter]');
  if (!button) return;
  currentStatusFilter = button.dataset.statusFilter;
  syncStatusFilters();
  renderAchievementLibrary();
});
searchInput.addEventListener('input', renderAchievementLibrary);
categoryFilter.addEventListener('change', renderAchievementLibrary);
chapterFilter.addEventListener('change', renderAchievementLibrary);
itemSearchInput.addEventListener('input', () => renderTracker(currentReport));
clearFiltersBtn.addEventListener('click', () => {
  clearFilters();
  renderAchievementLibrary();
  searchInput.focus();
});
spoilerToggleBtn.addEventListener('click', () => {
  rememberOpenGuides();
  hideSpoilers = !hideSpoilers;
  if (hideSpoilers) revealedGuideIds.clear();
  saveSpoilerPreference(hideSpoilers);
  syncSpoilerButton();
  renderSpoilerSensitiveViews();
});
achievementList.addEventListener('click', (event) => {
  const button = event.target.closest('[data-reveal-guide]');
  if (!button) return;
  const id = Number(button.dataset.revealGuide);
  if (!Number.isFinite(id)) return;
  revealedGuideIds.add(id);
  openGuideIds.add(id);
  renderSpoilerSensitiveViews();
  document.getElementById(`achievement-${id}`)?.querySelector('summary')?.focus();
});
achievementList.addEventListener('toggle', (event) => {
  const details = event.target.closest('details[data-guide-id]');
  if (!details || !achievementList.contains(details)) return;
  const id = Number(details.dataset.guideId);
  if (details.open) openGuideIds.add(id);
  else openGuideIds.delete(id);
  syncExpandButton();
}, true);
expandVisibleBtn.addEventListener('click', () => {
  const guides = [...achievementList.querySelectorAll('details[data-guide-id]')];
  const shouldOpen = guides.some((guide) => !guide.open);
  guides.forEach((guide) => {
    guide.open = shouldOpen;
    const id = Number(guide.dataset.guideId);
    if (shouldOpen) openGuideIds.add(id);
    else openGuideIds.delete(id);
  });
  syncExpandButton();
});
document.querySelectorAll('[data-open-help]').forEach((button) => button.addEventListener('click', () => saveHelpDialog.showModal()));
closeHelpBtn.addEventListener('click', () => saveHelpDialog.close());
helpDoneBtn.addEventListener('click', () => saveHelpDialog.close());
saveHelpDialog.addEventListener('click', (event) => {
  if (event.target !== saveHelpDialog) return;
  const rect = saveHelpDialog.getBoundingClientRect();
  if (event.clientX < rect.left || event.clientX > rect.right || event.clientY < rect.top || event.clientY > rect.bottom) saveHelpDialog.close();
});
copyPathBtn.addEventListener('click', async () => {
  try {
    await navigator.clipboard.writeText('b1\\Saved\\SaveGames');
    copyPathBtn.textContent = 'Copied';
    setTimeout(() => { copyPathBtn.textContent = 'Copy'; }, 2000);
  } catch { copyPathBtn.textContent = 'Select the path to copy'; }
});

function chooseFile(file) {
  activeRequest?.abort();
  activeRequest = null;
  selectedFile = null;
  setAnalyzeButton(false);
  selectedFilePanel.classList.add('hidden');
  selectedFilePanel.replaceChildren();
  dropTitle.textContent = 'Choose your save file';
  dropHint.textContent = 'or drag it here · .sav · up to 4 MB';
  hideStatus();
  if (!file) return;
  const error = validateFile(file);
  if (error) {
    saveFileInput.value = '';
    setStatus(error, 'error', true);
    return;
  }
  selectedFile = file;
  dropTitle.textContent = file.name;
  dropHint.textContent = `${formatBytes(file.size)} · Ready to analyze`;
  selectedFilePanel.innerHTML = '<strong>Save selected</strong><span>Read-only analysis</span>';
  selectedFilePanel.classList.remove('hidden');
  setAnalyzeButton(false);
}
function validateFile(file) {
  if (!file.name.toLowerCase().endsWith('.sav')) return 'Choose a Black Myth: Wukong file ending in .sav.';
  if (!file.size) return 'That file is empty. Choose your game’s .sav file.';
  if (file.size > MAX_SAVE_BYTES) return 'That save is larger than the 4 MB upload limit. Choose a smaller .sav file.';
  return null;
}
async function analyzeSave(event) {
  event.preventDefault();
  if (!selectedFile) return setStatus('Choose your .sav file first.', 'error', true);
  const error = validateFile(selectedFile);
  if (error) return setStatus(error, 'error', true);
  activeRequest?.abort();
  const controller = new AbortController();
  activeRequest = controller;
  const file = selectedFile;
  setAnalyzeButton(true);
  setStatus('Reading your save and checking all 81 achievements…');
  try {
    const form = new FormData();
    form.append('saveFile', file, file.name);
    const response = await fetch('/api/analyze', { method: 'POST', body: form, cache: 'no-store', signal: controller.signal });
    const payload = await readResponse(response);
    if (controller !== activeRequest) return;
    showReport(payload.report, { fileName: payload.saveFileName || file.name, example: false });
  } catch (error) {
    if (error.name === 'AbortError' || controller !== activeRequest) return;
    setStatus(error.message || 'Could not read your save. Please try again.', 'error', true);
  } finally {
    if (controller === activeRequest) {
      activeRequest = null;
      setAnalyzeButton(false);
    }
  }
}
async function readResponse(response) {
  let payload = null;
  try { payload = JSON.parse(await response.text()); } catch { /* Handle platform responses that are not JSON. */ }
  if (!response.ok || !payload?.ok || !Array.isArray(payload.report?.achievements)) {
    throw new Error(payload?.error || (response.status === 413 ? 'That upload exceeds the 4 MB limit.' : 'The save could not be analyzed. Please try again.'));
  }
  return payload;
}

async function loadExample() {
  activeRequest?.abort();
  const controller = new AbortController();
  activeRequest = controller;
  setAnalyzeButton(false);
  demoBtn.disabled = true;
  demoBtn.textContent = 'Loading the example…';
  try {
    const response = await fetch('/example-report.json', { signal: controller.signal });
    const payload = await readResponse(response);
    if (controller !== activeRequest) return;
    showReport(payload.report, { fileName: 'Example save', example: true });
  } catch (error) {
    if (error.name !== 'AbortError' && controller === activeRequest) setStatus(error.message, 'error', true);
  } finally {
    if (controller === activeRequest) activeRequest = null;
    demoBtn.disabled = false;
    demoBtn.innerHTML = 'Just looking? <span>Explore an example</span> <span aria-hidden="true">→</span>';
  }
}
function showReport(report, { fileName, example }) {
  currentReport = report;
  isExampleReport = example;
  revealedGuideIds.clear();
  openGuideIds.clear();
  achievementList.replaceChildren();
  trackerList.replaceChildren();
  populateFilters(report.achievements);
  clearFilters();
  itemSearchInput.value = '';
  reportFile.textContent = example ? 'EXAMPLE SAVE REPORT' : `SAVE REPORT · ${fileName}`;
  demoBanner.hidden = !example;
  renderAll(report);
  switchView('overview');
  hideStatus();
  landing.hidden = true;
  results.classList.remove('hidden');
  resetBtn.classList.remove('hidden');
  document.body.classList.add('has-report');
  window.scrollTo({ top: 0, behavior: 'instant' });
  reportTitle.focus({ preventScroll: true });
}
function resetReport() {
  activeRequest?.abort();
  activeRequest = null;
  currentReport = null;
  isExampleReport = false;
  selectedFile = null;
  saveFileInput.value = '';
  revealedGuideIds.clear();
  openGuideIds.clear();
  achievementList.replaceChildren();
  trackerList.replaceChildren();
  chooseFile(null);
  results.classList.add('hidden');
  landing.hidden = false;
  resetBtn.classList.add('hidden');
  document.body.classList.remove('has-report');
  window.scrollTo({ top: 0, behavior: 'instant' });
  saveFileInput.focus({ preventScroll: true });
}
function switchView(view) {
  const tab = document.getElementById(`tab-${view}`);
  if (!tab) return;
  resultTabs.querySelectorAll('[role="tab"]').forEach((button) => {
    const selected = button === tab;
    button.setAttribute('aria-selected', String(selected));
    button.tabIndex = selected ? 0 : -1;
  });
  reportViews.forEach((panel) => { panel.hidden = panel.id !== tab.getAttribute('aria-controls'); });
}
function openAchievement(id) {
  if (!currentReport?.achievements.some((item) => item.achievementId === id)) return;
  rememberOpenGuides();
  clearFilters();
  renderAchievementLibrary();
  switchView('achievements');
  const card = document.getElementById(`achievement-${id}`);
  card.querySelector('details').open = true;
  openGuideIds.add(id);
  card.querySelector('summary').focus({ preventScroll: true });
  card.scrollIntoView({ block: 'start', behavior: scrollBehavior() });
}
function setAnalyzeButton(busy) {
  analyzeBtn.disabled = busy || !selectedFile;
  analyzeBtn.innerHTML = busy ? '<span>Analyzing your save…</span><span aria-hidden="true">◌</span>' : '<span>Analyze my save</span><span aria-hidden="true">→</span>';
  uploadForm.setAttribute('aria-busy', String(busy));
}
function setStatus(message, type = 'ok', focus = false) {
  statusPanel.textContent = message;
  statusPanel.classList.remove('hidden');
  statusPanel.classList.toggle('status-error', type === 'error');
  statusPanel.setAttribute('role', type === 'error' ? 'alert' : 'status');
  if (focus) statusPanel.focus();
}
function hideStatus() { statusPanel.classList.add('hidden'); statusPanel.textContent = ''; }
function renderAll(report) {
  renderCompletionState(report);
  renderOverview(report);
  syncSpoilerButton();
  renderSpoilerSensitiveViews();
}
function renderCompletionState(report) {
  const complete = Number(report?.totalAchievements) === EXPECTED_ACHIEVEMENT_COUNT
    && Number(report?.completedAchievements) === EXPECTED_ACHIEVEMENT_COUNT
    && Number(report?.incompleteAchievements) === 0
    && report?.achievements.length === EXPECTED_ACHIEVEMENT_COUNT
    && report.achievements.every((item) => item.isComplete === true);
  completionCeremony.hidden = !complete;
  if (complete) completionPlayer.textContent = report.playerName || 'The Destined One';
}
function renderSpoilerSensitiveViews() {
  if (!currentReport) return;
  renderNextSteps(currentReport);
  renderMissables(currentReport);
  renderTracker(currentReport);
  renderAchievementLibrary();
}
function renderOverview(report) {
  const total = Math.max(Number(report.totalAchievements) || 81, 1);
  const completed = Math.max(Number(report.completedAchievements) || 0, 0);
  const remaining = Number(report.incompleteAchievements) || 0;
  const missing = getTrackedCollections(report).reduce((sum, entry) => sum + entry.missing.length, 0);
  const percent = Math.min(100, Math.round(completed / total * 100));
  const circumference = 2 * Math.PI * 47;
  progressArc.style.strokeDasharray = String(circumference);
  progressArc.style.strokeDashoffset = String(circumference * (1 - percent / 100));
  progressPct.textContent = `${percent}%`;
  document.getElementById('ovAchievements').innerHTML = `${completed} <small>/ ${total}</small>`;
  document.getElementById('ovMissing').textContent = String(remaining);
  document.getElementById('ovCollectibles').textContent = String(missing);
  const chapter = normalizeChapterNumber(report.currentChapterId);
  document.getElementById('ovChapter').textContent = chapter > 0 ? String(chapter) : 'Prologue';
  document.getElementById('ovNgPlus').textContent = Number(report.newGamePlusCount) > 0 ? `NG+ ${report.newGamePlusCount}` : 'First journey';
  overviewNarrative.textContent = remaining === 0 ? 'All achievements completed.' : 'One save. A clear picture.';
  playerContext.textContent = `${report.playerName || 'The Destined One'} · Level ${report.playerLevel ?? '—'} · ${chapter > 0 ? `Chapter ${chapter}` : 'Prologue'}`;
  document.getElementById('itemsTabCount').textContent = String(missing);
  document.getElementById('collectionTeaser').textContent = missing ? `${missing} missing requirements, with locations and tips.` : 'Your tracked collections are complete.';
  document.getElementById('allFilterCount').textContent = String(total);
  document.getElementById('remainingFilterCount').textContent = String(remaining);
  document.getElementById('completeFilterCount').textContent = String(completed);
}
function getNextSteps(report) {
  const chapter = normalizeChapterNumber(report.currentChapterId);
  const ng = Number(report.newGamePlusCount) || 0;
  return report.achievements.filter((item) => !item.isComplete && item.achievementId !== 81081).sort((left, right) => {
    const stage = chapterRouteRank(left, chapter, ng) - chapterRouteRank(right, chapter, ng);
    if (stage) return stage;
    if (left.isMissable !== right.isMissable) return left.isMissable ? -1 : 1;
    return (left.priorityOrder ?? 99) - (right.priorityOrder ?? 99) || left.achievementId - right.achievementId;
  }).slice(0, 3);
}
function renderNextSteps(report) {
  const items = getNextSteps(report);
  if (!items.length) {
    const complete = report.completedAchievements === 81;
    nextStepsList.innerHTML = `<div class="completion-banner"><strong>${complete ? 'Your achievement checklist is complete.' : 'Just the final achievement remains.'}</strong><p>${complete ? 'Enjoy the view. There are no unfinished achievements in this save.' : 'Open Final Fulfillment to check the last requirement.'}</p>${complete ? '' : '<button class="text-button" type="button" data-open-guide="81081">Open Final Fulfillment →</button>'}</div>`;
    return;
  }
  nextStepsList.innerHTML = items.map((item, index) => `<article class="next-card"><span class="next-card-number">${String(index + 1).padStart(2, '0')}</span><div class="next-card-content"><div class="next-card-meta"><span>${esc(item.chapter)}</span>${item.isMissable ? '<span class="next-missable">Can be missed</span>' : ''}${item.requiresNewGamePlus ? '<span>New Game+</span>' : ''}</div><h3>${esc(item.displayTitle)}</h3><p${isAchievementSpoilerHidden(item) ? ' class="spoiler-placeholder"' : ''}>${isAchievementSpoilerHidden(item) ? 'Details hidden while spoiler protection is on.' : esc(item.requirementSummary || item.routeHint)}</p></div><button class="next-card-link" type="button" data-open-guide="${item.achievementId}" aria-label="Open ${esc(item.displayTitle)} guide">→</button></article>`).join('');
}
function chapterRouteRank(item, chapter, ng) {
  if (item.requiresNewGamePlus && !ng) return 5;
  const label = String(item.chapter || '');
  if (/new game/i.test(label)) return ng > 0 ? 1 : 5;
  if (/endgame/i.test(label)) return chapter >= 6 ? 0 : 4;
  if (/all chapters/i.test(label)) return 1;
  if (/prologue/i.test(label)) return chapter <= 1 ? 0 : 2;
  const match = label.match(/Chapters?\s+(\d)(?:\s*[-–]\s*(\d))?/i);
  if (!match) return 3;
  const first = Number(match[1]);
  const last = Number(match[2] || match[1]);
  if (chapter >= first && chapter <= last) return first === last ? 0 : 1;
  return last < chapter ? 2 : 3;
}
function normalizeChapterNumber(value) { const chapter = Number(value) || 0; return chapter >= 10 ? Math.floor(chapter / 10) : chapter; }
function renderMissables(report) {
  const items = report.achievements.filter((item) => !item.isComplete && item.isMissable);
  const chapter = normalizeChapterNumber(report.currentChapterId);
  const near = items.filter((item) => chapterRouteRank(item, chapter, Number(report.newGamePlusCount) || 0) <= 1).slice(0, 3);
  missableSummary.innerHTML = items.length ? `<p class="missable-count">${items.length} missable${items.length === 1 ? '' : 's'} still open</p><p>Some achievements depend on a choice or a quest order. Check their guides before finishing a chapter.</p>${near.length ? `<div class="missable-list">${near.map((item) => `<button class="missable-link" type="button" data-open-guide="${item.achievementId}">${esc(item.displayTitle)} →</button>`).join('')}</div>` : ''}` : '<p>All missable achievements are complete in this save. You can focus on the remaining checklist.</p>';
}
function getTrackedCollections(report) {
  return (report?.achievements ?? []).filter((item) => !item.isComplete).map((item) => ({ item, missing: Array.isArray(item.missingTargets) ? item.missingTargets : [] })).filter((entry) => entry.missing.length > 0).sort((a, b) => b.missing.length - a.missing.length || a.item.achievementId - b.item.achievementId);
}
function renderTracker(report) {
  if (!report) return;
  const collections = getTrackedCollections(report);
  const total = collections.reduce((sum, entry) => sum + entry.missing.length, 0);
  trackerCount.textContent = `${total} missing`;
  const query = itemSearchInput.value.trim().toLocaleLowerCase();
  const openIds = new Set([...trackerList.querySelectorAll('details[open]')].map((node) => Number(node.dataset.collectionId)));
  const visible = collections.map(({ item, missing }) => {
    const titleMatch = item.displayTitle.toLocaleLowerCase().includes(query);
    const matches = !query || titleMatch ? missing : isAchievementSpoilerHidden(item) ? [] : missing.filter((target) => `${target.name} ${target.howToGet || ''}`.toLocaleLowerCase().includes(query));
    return { item, missing: matches, totalMissing: missing.length };
  }).filter((entry) => entry.missing.length);
  if (!visible.length) {
    trackerList.innerHTML = `<div class="tracker-empty">${query ? 'No missing items match. Try another search or collection name.' : 'All tracked collection requirements are complete. Other achievement steps may still remain.'}</div>`;
    return;
  }
  trackerList.innerHTML = visible.map(({ item, missing, totalMissing }) => `<details class="tracker-group" data-collection-id="${item.achievementId}" ${query || openIds.has(item.achievementId) ? 'open' : ''}><summary><span class="tracker-symbol" aria-hidden="true">◇</span><span class="tracker-group-title"><strong>${esc(item.displayTitle)}</strong><small>${(item.requirementTargets ?? []).filter((target) => target.isCollected).length} of ${(item.requirementTargets ?? []).length} requirements collected</small></span><span class="tracker-missing-badge">${totalMissing} missing</span><span class="chevron" aria-hidden="true"></span></summary>${isAchievementSpoilerHidden(item) ? '<div class="spoiler-gate spoiler-gate-compact"><p>Item names and locations are hidden. Reveal this achievement’s guide to see them.</p></div>' : `<ul class="tracker-items">${missing.map((target) => `<li class="tracker-item"><strong>${esc(target.name)}</strong>${target.howToGet ? `<p>${esc(target.howToGet)}</p>` : ''}</li>`).join('')}</ul>`}<div class="tracker-guide-link"><button class="text-button" type="button" data-open-guide="${item.achievementId}">Open achievement guide →</button></div></details>`).join('');
}

function populateFilters(achievements) {
  const categories = [...new Set(achievements.map((item) => item.category).filter(Boolean))];
  const chapters = [...new Set(achievements.map((item) => item.chapter).filter(Boolean))];
  categoryFilter.replaceChildren(new Option('All categories', 'all'), ...categories.map((value) => new Option(value, value)));
  chapterFilter.replaceChildren(new Option('All chapters', 'all'), ...chapters.map((value) => new Option(value, value)));
}
function clearFilters() {
  searchInput.value = '';
  categoryFilter.value = 'all';
  chapterFilter.value = 'all';
  currentStatusFilter = 'all';
  syncStatusFilters();
}
function syncStatusFilters() { statusFilters.querySelectorAll('button').forEach((button) => button.setAttribute('aria-pressed', String(button.dataset.statusFilter === currentStatusFilter))); }
function renderAchievementLibrary() {
  if (!currentReport) return;
  rememberOpenGuides();
  const query = searchInput.value.trim().toLocaleLowerCase();
  const visible = currentReport.achievements.filter((item) => {
    if (currentStatusFilter === 'complete' && !item.isComplete) return false;
    if (currentStatusFilter === 'incomplete' && item.isComplete) return false;
    if (categoryFilter.value !== 'all' && item.category !== categoryFilter.value) return false;
    if (chapterFilter.value !== 'all' && item.chapter !== chapterFilter.value) return false;
    return !query || buildSearchText(item).includes(query);
  });
  libraryCount.textContent = `${visible.length} of ${currentReport.achievements.length} achievements`;
  emptyState.classList.toggle('hidden', visible.length > 0);
  achievementList.innerHTML = visible.map(renderAchievementCard).join('');
  syncExpandButton();
}
function buildSearchText(item) {
  const parts = [item.displayTitle, item.category, item.chapter];
  if (!isAchievementSpoilerHidden(item)) {
    parts.push(item.requirementSummary, item.routeHint, item.missableNote, ...(item.prerequisites ?? []), ...(item.guideSteps ?? []), ...(item.guideChecklist ?? []));
    (item.requirementTargets ?? []).forEach((target) => parts.push(target.name, target.howToGet));
  }
  return parts.filter(Boolean).join(' ').toLocaleLowerCase();
}
function renderAchievementCard(item) {
  const number = String(item.achievementId - 81000).padStart(2, '0');
  const progress = getProgress(item);
  const hidden = isAchievementSpoilerHidden(item);
  return `<article id="achievement-${item.achievementId}" class="achievement-card ${item.isComplete ? 'is-complete' : 'is-incomplete'}"><details class="achievement-guide" data-guide-id="${item.achievementId}" ${openGuideIds.has(item.achievementId) ? 'open' : ''}><summary><span class="achievement-number" aria-hidden="true">${item.isComplete ? '✓' : number}</span><div class="achievement-summary-copy"><h3>${esc(item.displayTitle)}</h3><p class="requirement-summary${hidden ? ' spoiler-placeholder' : ''}">${hidden ? 'Details hidden while spoiler protection is on.' : esc(item.requirementSummary || item.routeHint)}</p><div class="tag-row"><span class="tag">${esc(item.chapter)}</span><span class="tag">${esc(item.category)}</span>${item.isMissable ? '<span class="tag tag-missable">Missable</span>' : ''}${item.requiresNewGamePlus ? '<span class="tag tag-ng">New Game+</span>' : ''}${!item.isPresentInSave ? '<span class="tag tag-unverified">Not recorded in save</span>' : ''}</div></div><span class="achievement-status"><span class="status-label">${item.isComplete ? 'Complete' : 'Remaining'}</span>${!item.isComplete ? `<span class="progress-copy">${esc(progress.label)}</span><span class="achievement-progress" aria-label="${esc(progress.label)}"><span class="progress-track" style="display:block"><span class="progress-fill" style="display:block;width:${progress.percent}%"></span></span></span>` : ''}</span><span class="chevron" aria-hidden="true"></span></summary><div class="guide-body">${hidden ? renderSpoilerGate(item) : renderGuideContent(item)}</div></details></article>`;
}
function getProgress(item) {
  if (item.isComplete) return { percent: 100, label: 'Complete' };
  if (item.requiredCount > 0) {
    const completed = Math.max(Number(item.completedCount) || 0, 0);
    const required = Math.max(Number(item.requiredCount) || 1, 1);
    return { percent: Math.min(100, Math.round(completed / required * 100)), label: `${completed} / ${required}` };
  }
  return { percent: 0, label: item.isPresentInSave ? 'Not unlocked' : 'Not recorded' };
}
function isAchievementSpoilerHidden(item) { return hideSpoilers && !revealedGuideIds.has(item.achievementId); }
function renderSpoilerGate(item) {
  return `<div class="spoiler-gate"><div><p>The walkthrough and collectible locations are hidden.</p><button class="reveal-button" type="button" data-reveal-guide="${item.achievementId}">Reveal this guide</button></div></div>`;
}
function renderGuideContent(item) {
  const prerequisites = item.prerequisites ?? [];
  const steps = item.guideSteps ?? [];
  const targets = item.requirementTargets ?? [];
  const milestones = getAdditionalGuideMilestones(item.guideChecklist ?? [], targets);
  return `<div class="guide-layout"><div class="guide-column"><section class="guide-section"><h4 class="guide-label">How to unlock it</h4><p>${esc(item.routeHint || item.requirementSummary)}</p></section>${item.isMissable && item.missableNote ? `<section class="guide-section warning-box"><h4 class="guide-label">Do this before moving on</h4><p>${esc(item.missableNote)}</p></section>` : ''}${prerequisites.length ? `<section class="guide-section"><h4 class="guide-label">Before you start</h4><ul class="guide-list">${prerequisites.map((step) => `<li>${esc(step)}</li>`).join('')}</ul></section>` : ''}${steps.length ? `<section class="guide-section"><h4 class="guide-label">Step by step</h4><ol class="guide-list">${steps.map((step) => `<li>${esc(step)}</li>`).join('')}</ol></section>` : ''}</div><div class="guide-column">${targets.length ? renderTargetChecklist(targets) : ''}${milestones.length ? renderGuideMilestones(milestones, targets.length > 0) : ''}${!milestones.length && !targets.length ? '<section class="guide-section"><h4 class="guide-label">Completion check</h4><p>Your save shows whether this achievement is complete. Follow the walkthrough, then upload a fresh save to check your progress.</p></section>' : ''}</div></div>`;
}
function getAdditionalGuideMilestones(entries, targets) {
  if (!targets.length) return entries;
  const names = new Set(targets.map((target) => normalizeRequirementName(target.name)));
  return entries.filter((entry) => !names.has(normalizeRequirementName(entry)));
}
function normalizeRequirementName(value) {
  return String(value ?? '').toLowerCase().replace(/^(?:soak|spirit(?: skill)?):\s*/, '').replace(/\s*\([^)]*\)\s*$/, '').replace(/[^a-z0-9]+/g, '');
}
function renderGuideMilestones(entries, tracked) {
  return `<section class="guide-section"><h4 class="guide-label">${tracked ? 'More things to check in-game' : 'Check these in-game'}</h4><p class="tracking-note">${tracked ? 'These route steps are not individually checked by your save.' : 'Your save records the overall result, so use these notes to check each step yourself.'}</p><ul class="guide-list">${entries.map((entry) => `<li>${esc(entry)}</li>`).join('')}</ul></section>`;
}
function renderTargetChecklist(targets) {
  const collected = targets.filter((target) => target.isCollected).length;
  return `<section class="guide-section"><h4 class="guide-label">Your collection · ${collected} / ${targets.length}</h4><p class="tracking-note">Collected and missing requirements are checked against your save.</p><ul class="target-list">${targets.map((target) => `<li class="target-row ${target.isCollected ? 'is-owned' : 'is-missing'}"><span class="target-mark" aria-hidden="true">${target.isCollected ? '✓' : '×'}</span><span><strong>${esc(target.name)}</strong><span class="target-state">${target.isCollected ? 'Collected' : 'Missing'}</span>${target.howToGet ? `<small>${esc(target.howToGet)}</small>` : ''}</span></li>`).join('')}</ul></section>`;
}
function rememberOpenGuides() {
  achievementList.querySelectorAll('details[data-guide-id]').forEach((details) => {
    const id = Number(details.dataset.guideId);
    if (details.open) openGuideIds.add(id);
    else openGuideIds.delete(id);
  });
}
function syncExpandButton() {
  const guides = [...achievementList.querySelectorAll('details[data-guide-id]')];
  expandVisibleBtn.textContent = guides.length && guides.every((guide) => guide.open) ? 'Collapse guides' : 'Expand guides';
  expandVisibleBtn.disabled = !guides.length;
}
function syncSpoilerButton() {
  spoilerToggleBtn.setAttribute('aria-pressed', String(hideSpoilers));
  spoilerToggleBtn.textContent = hideSpoilers ? 'Spoilers hidden' : 'Hide spoilers';
  spoilerToggleBtn.setAttribute('aria-label', hideSpoilers ? 'Show spoilers in all guides' : 'Hide spoilers in all guides');
}
function loadSpoilerPreference() {
  try { return localStorage.getItem('journey-ledger.hide-spoilers') === 'true'; } catch { return false; }
}
function saveSpoilerPreference(value) {
  try { localStorage.setItem('journey-ledger.hide-spoilers', String(value)); } catch { /* Local storage is optional. */ }
}
function downloadChecklist() {
  if (!currentReport) return;
  const report = currentReport;
  const lines = ['JOURNEY LEDGER — BLACK MYTH: WUKONG', isExampleReport ? 'EXAMPLE CHECKLIST — sample progress, not your save.' : 'Your save checklist', `${report.completedAchievements} / ${report.totalAchievements} achievements complete`, ''];
  report.achievements.filter((item) => !item.isComplete).forEach((item) => {
    lines.push(`[ ] ${item.displayTitle} (${item.chapter})${item.isMissable ? ' — MISSABLE' : ''}`);
    if (isAchievementSpoilerHidden(item)) lines.push('    Details hidden by your spoiler preference.');
    else {
      if (item.requirementSummary) lines.push(`    ${item.requirementSummary}`);
      if (item.isMissable && item.missableNote) lines.push(`    Before moving on: ${item.missableNote}`);
      (item.guideSteps ?? []).forEach((step, index) => lines.push(`    ${index + 1}. ${step}`));
      (item.missingTargets ?? []).forEach((target) => {
        lines.push(`    [ ] ${target.name}`);
        if (target.howToGet) lines.push(`        ${target.howToGet}`);
      });
    }
    lines.push('');
  });
  if (report.completedAchievements === report.totalAchievements) lines.push('All achievements are complete.');
  lines.push('This is a snapshot. Upload a fresh save after playing to update your checklist.');
  const url = URL.createObjectURL(new Blob([lines.join('\n')], { type: 'text/plain;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = isExampleReport ? 'journey-ledger-example.txt' : 'journey-ledger-checklist.txt';
  link.click();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}
function scrollBehavior() { return matchMedia('(prefers-reduced-motion: reduce)').matches ? 'instant' : 'smooth'; }
function formatBytes(bytes) { return bytes < 1024 ? `${bytes} B` : bytes < 1024 * 1024 ? `${(bytes / 1024).toFixed(0)} KB` : `${(bytes / (1024 * 1024)).toFixed(2)} MB`; }
function esc(value) { return String(value ?? '').replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;').replaceAll('"', '&quot;').replaceAll("'", '&#39;'); }
syncSpoilerButton();
