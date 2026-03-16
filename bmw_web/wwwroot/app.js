// Elements
const savePathInput = document.getElementById("savePath");
const analyzeBtn = document.getElementById("analyzeBtn");
const statusPanel = document.getElementById("statusPanel");
const overviewPanel = document.getElementById("overviewPanel");
const progressArc = document.getElementById("progressArc");
const progressPct = document.getElementById("progressPct");
const itemTrackerPanel = document.getElementById("itemTrackerPanel");
const trackerCount = document.getElementById("trackerCount");
const trackerList = document.getElementById("trackerList");
const actionPlanPanel = document.getElementById("actionPlanPanel");
const actionCount = document.getElementById("actionCount");
const actionList = document.getElementById("actionList");
const fullPanel = document.getElementById("fullPanel");
const fullTableBody = document.getElementById("fullTableBody");
const searchInput = document.getElementById("searchInput");

let currentReport = null;
let currentFilter = "all";
let latestAnalysisRequestId = 0;
let lastAnalysisMeta = null;
const expandedAchievementIds = new Set();

// Events
analyzeBtn.addEventListener("click", analyze);
savePathInput.addEventListener("keydown", (e) => {
  if (e.key === "Enter") { e.preventDefault(); analyze(); }
});

document.querySelectorAll(".filter-tabs .tab").forEach((tab) => {
  tab.addEventListener("click", () => {
    document.querySelectorAll(".filter-tabs .tab").forEach((t) => t.classList.remove("active"));
    tab.classList.add("active");
    currentFilter = tab.dataset.filter;
    if (currentReport) renderFullTable(currentReport);
  });
});

searchInput.addEventListener("input", () => {
  if (currentReport) renderFullTable(currentReport);
});

fullTableBody.addEventListener("click", (event) => {
  const toggleBtn = event.target.closest("[data-achievement-toggle]");
  if (!toggleBtn) return;

  const achievementId = Number(toggleBtn.dataset.achievementToggle);
  if (!Number.isFinite(achievementId)) return;

  if (expandedAchievementIds.has(achievementId)) {
    expandedAchievementIds.delete(achievementId);
  } else {
    expandedAchievementIds.add(achievementId);
  }

  if (currentReport) renderFullTable(currentReport);
});

// Status
function setStatus(text, type = "ok") {
  statusPanel.classList.remove("hidden", "status-ok", "status-error");
  statusPanel.classList.add(type === "error" ? "status-error" : "status-ok");
  statusPanel.textContent = text;
}

function formatTimestamp(value) {
  if (!value) return "an unknown time";

  const parsed = new Date(value);
  if (Number.isNaN(parsed.getTime())) {
    return String(value);
  }

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "medium",
  }).format(parsed);
}

function hideResults() {
  overviewPanel.classList.add("hidden");
  itemTrackerPanel.classList.add("hidden");
  actionPlanPanel.classList.add("hidden");
  fullPanel.classList.add("hidden");
}

// Analyze
async function analyze() {
  const savePath = savePathInput.value.trim();
  if (!savePath) {
    setStatus("Enter a save path first.", "error");
    hideResults();
    return;
  }

  const requestId = ++latestAnalysisRequestId;
  const priorAnalysis = lastAnalysisMeta;
  const isRepeatAnalysis = currentReport !== null;
  currentReport = null;
  analyzeBtn.disabled = true;
  analyzeBtn.textContent = isRepeatAnalysis ? "Reanalyzing..." : "Analyzing...";
  setStatus(isRepeatAnalysis
    ? "Re-reading your save and refreshing the checklist..."
    : "Reading your save and updating the checklist...");
  hideResults();

  try {
    const response = await fetch(`/api/analyze?requestId=${requestId}`, {
      method: "POST",
      cache: "no-store",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ savePath }),
    });
    const data = await response.json();

    if (requestId !== latestAnalysisRequestId) {
      return;
    }

    if (!response.ok || !data.ok) {
      throw new Error(data.error || "Analysis failed.");
    }

    currentReport = data.report;
    lastAnalysisMeta = {
      savePath,
      analyzedAtUtc: data.analyzedAtUtc,
      saveFileLastWriteTimeUtc: data.saveFileLastWriteTimeUtc,
    };
    renderAll(data.report);

    const analyzedAtText = formatTimestamp(data.analyzedAtUtc);
    const saveUpdatedText = formatTimestamp(data.saveFileLastWriteTimeUtc);
    const repeatedSaveVersion =
      priorAnalysis &&
      priorAnalysis.savePath === savePath &&
      priorAnalysis.saveFileLastWriteTimeUtc === data.saveFileLastWriteTimeUtc;

    let statusText = `Updated on ${analyzedAtText}. Save file last modified on ${saveUpdatedText}. ${data.report.completedAchievements}/${data.report.totalAchievements} achievements complete.`;
    if (repeatedSaveVersion) {
      statusText += " The save file timestamp is unchanged since the previous analysis.";
    }

    setStatus(statusText);
  } catch (error) {
    if (requestId !== latestAnalysisRequestId) {
      return;
    }

    setStatus(error.message || "Analysis failed.", "error");
    hideResults();
  } finally {
    if (requestId === latestAnalysisRequestId) {
      analyzeBtn.disabled = false;
      analyzeBtn.textContent = "Analyze";
    }
  }
}

// Render everything
function renderAll(report) {
  renderOverview(report);
  renderItemTracker(report);
  renderActionPlan(report);
  renderFullTable(report);
}

// Overview bar with progress ring
function renderOverview(report) {
  const pct = report.totalAchievements > 0
    ? Math.round((report.completedAchievements / report.totalAchievements) * 100)
    : 0;
  const circumference = 2 * Math.PI * 34; // r=34
  const offset = circumference - (pct / 100) * circumference;

  progressArc.style.strokeDasharray = `${circumference} ${circumference}`;
  progressArc.style.strokeDashoffset = String(offset);
  progressPct.textContent = `${pct}%`;

  document.getElementById("ovPlayer").textContent = report.playerName || "Unknown";
  document.getElementById("ovLevel").textContent = String(report.playerLevel);
  document.getElementById("ovNgPlus").textContent = String(report.newGamePlusCount);
  document.getElementById("ovAchievements").textContent = `${report.completedAchievements} / ${report.totalAchievements}`;
  document.getElementById("ovMissing").textContent = String(report.incompleteAchievements);

  overviewPanel.classList.remove("hidden");
}

// Action plan cards
function renderItemTracker(report) {
  const tracked = report.achievements
    .filter((item) => !item.isComplete)
    .filter((item) => Array.isArray(item.missingTargets) && item.missingTargets.length > 0)
    .sort((a, b) => {
      if (b.missingTargets.length !== a.missingTargets.length) return b.missingTargets.length - a.missingTargets.length;
      return a.achievementId - b.achievementId;
    });

  const totalMissing = tracked.reduce((sum, item) => sum + item.missingTargets.length, 0);
  trackerCount.textContent = `${totalMissing} items missing`;

  if (tracked.length === 0) {
    trackerList.innerHTML = `
      <div class="tracker-empty">
        <p>No tracked collection items are missing right now.</p>
      </div>`;
    itemTrackerPanel.classList.remove("hidden");
    return;
  }

  trackerList.innerHTML = tracked.map((item) => `
    <article class="tracker-card">
      <div class="tracker-card-head">
        <div>
          <h3 class="tracker-title">${esc(item.displayTitle)}</h3>
          <p class="tracker-meta">${item.missingTargets.length} still missing</p>
        </div>
      </div>
      <div class="tracker-route">${esc(item.routeHint)}</div>
      <div class="tracker-grid">
        ${item.missingTargets.map((target) => `
          <div class="tracker-item">
            <div class="tracker-item-top">
              <span class="target-status-dot dot-missing"></span>
              <strong>${esc(target.name)}</strong>
            </div>
            ${target.howToGet ? `<div class="tracker-how">${esc(target.howToGet)}</div>` : ""}
          </div>`).join("")}
      </div>
    </article>`).join("");

  itemTrackerPanel.classList.remove("hidden");
}

// Action plan cards
function renderActionPlan(report) {
  const incomplete = report.achievements
    .filter((x) => !x.isComplete)
    .sort((a, b) => {
      if (a.priorityOrder !== b.priorityOrder) return a.priorityOrder - b.priorityOrder;
      if (a.remainingCount !== b.remainingCount) return b.remainingCount - a.remainingCount;
      return a.achievementId - b.achievementId;
    });

  actionCount.textContent = `${incomplete.length} remaining`;

  if (incomplete.length === 0) {
    actionList.innerHTML = `
      <div class="action-complete-banner">
        <span class="action-complete-icon">&#10003;</span>
        <p>All achievements complete.</p>
      </div>`;
    actionPlanPanel.classList.remove("hidden");
    return;
  }

  // Separate meta achievements to show last
  const direct = incomplete.filter((x) => x.priorityLabel !== "Meta");
  const meta = incomplete.filter((x) => x.priorityLabel === "Meta");
  const ordered = [...direct, ...meta];

  actionList.innerHTML = ordered
    .map((item, idx) => {
      const pctVal = item.requiredCount > 0
        ? Math.round((item.completedCount / item.requiredCount) * 100)
        : (item.isComplete ? 100 : 0);
      const isMeta = item.priorityLabel === "Meta";

      return `
      <article class="action-card ${isMeta ? 'action-card-meta' : ''}">
        <div class="action-header">
          <span class="action-number">${idx + 1}</span>
          <div class="action-title-block">
            <h3 class="action-title">${esc(item.displayTitle)}</h3>
            <div class="action-badges">
              ${item.resetOnNewGamePlus ? '<span class="badge badge-warn">Resets on NG+</span>' : ''}
            </div>
          </div>
        </div>

        <div class="action-progress-row">
          <div class="action-progress-bar">
            <div class="action-progress-fill" style="width: ${pctVal}%"></div>
          </div>
          <span class="action-progress-text">${item.completedCount}/${esc(item.requiredCountText)}</span>
        </div>

        <div class="action-route">
          <span class="route-icon">&#9873;</span>
          <span>${esc(item.routeHint)}</span>
        </div>

        ${renderTargetBlock(item)}

        <div class="action-steps">
          <div class="steps-label">Next checks</div>
          <ol class="steps-list">
            ${item.steps.map((s) => `<li>${esc(s)}</li>`).join("")}
          </ol>
        </div>
      </article>`;
    })
    .join("");

  actionPlanPanel.classList.remove("hidden");
}

// Target tracking block
function renderTargetBlock(item) {
  const missing = Array.isArray(item.missingTargets) ? item.missingTargets : [];
  const all = Array.isArray(item.requirementTargets) ? item.requirementTargets : [];

  if (missing.length === 0 && all.length === 0) return "";

  const missingHtml = missing.length > 0
    ? `<div class="targets-missing">
        <div class="targets-heading targets-heading-missing">Missing Items (${missing.length})</div>
        <ul class="target-items">
          ${missing.map((t) => `
            <li class="target-item target-item-missing">
              <span class="target-status-dot dot-missing"></span>
              <strong>${esc(t.name)}</strong>
              ${t.howToGet ? `<div class="target-how">${esc(t.howToGet)}</div>` : ""}
            </li>`).join("")}
        </ul>
      </div>`
    : "";

  const collected = all.filter((x) => x.isCollected).length;
  const trackedHtml = all.length > 0
    ? `<details class="targets-tracked">
        <summary class="targets-heading">Full checklist (${collected}/${all.length})</summary>
        <ul class="target-items target-items-compact">
          ${all.map((t) => `
            <li class="target-item ${t.isCollected ? 'target-item-done' : 'target-item-missing'}">
              <span class="target-status-dot ${t.isCollected ? 'dot-done' : 'dot-missing'}"></span>
              ${esc(t.name)}
            </li>`).join("")}
        </ul>
      </details>`
    : "";

  return `<div class="action-targets">${missingHtml}${trackedHtml}</div>`;
}

function renderExpandedTargetList(item) {
  const all = Array.isArray(item.requirementTargets) ? item.requirementTargets : [];
  if (all.length === 0) return "";

  const collected = all.filter((target) => target.isCollected).length;
  const missing = all.length - collected;

  return `
    <div class="table-detail-panel">
      <div class="table-detail-head">
        <div class="table-detail-title">Tracked items</div>
        <div class="table-detail-meta">${collected}/${all.length} collected${missing > 0 ? `, ${missing} missing` : ""}</div>
      </div>
      <ul class="target-items target-items-compact">
        ${all.map((target) => `
          <li class="target-item ${target.isCollected ? "target-item-done" : "target-item-missing"}">
            <span class="target-status-dot ${target.isCollected ? "dot-done" : "dot-missing"}"></span>
            <strong>${esc(target.name)}</strong>
            ${target.howToGet ? `<div class="target-how">${esc(target.howToGet)}</div>` : ""}
          </li>`).join("")}
      </ul>
    </div>`;
}

// Full table with filtering
function renderFullTable(report) {
  const query = searchInput.value.trim().toLowerCase();
  const filtered = report.achievements
    .filter((item) => {
      if (currentFilter === "incomplete" && item.isComplete) return false;
      if (currentFilter === "complete" && !item.isComplete) return false;
       if (query && !item.displayTitle.toLowerCase().includes(query)) return false;
       return true;
     })
    .sort((a, b) => a.achievementId - b.achievementId);

  fullTableBody.innerHTML = filtered
    .map((item) => {
      const cls = item.isComplete ? "row-complete" : "row-incomplete";
      const statusCls = item.isComplete ? "status-complete" : "status-incomplete";
      const trackedTargets = Array.isArray(item.requirementTargets) ? item.requirementTargets : [];
      const hasTrackedTargets = trackedTargets.length > 0;
      const trackedCollected = trackedTargets.filter((target) => target.isCollected).length;
      const isExpanded = hasTrackedTargets && expandedAchievementIds.has(item.achievementId);

      return `
      <tr class="${cls}">
        <td class="achievement-name-cell">
          <div class="achievement-name-main">${esc(item.displayTitle)}</div>
          ${hasTrackedTargets ? `
            <div class="achievement-name-meta">
              <span class="achievement-checklist-meta">${trackedCollected}/${trackedTargets.length} tracked</span>
              <button
                type="button"
                class="row-toggle-btn"
                data-achievement-toggle="${item.achievementId}">
                ${isExpanded ? "Hide items" : "View items"}
              </button>
            </div>` : ""}
        </td>
        <td class="${statusCls}">${item.isComplete ? "&#10003; Done" : "&#10007; Missing"}</td>
        <td>${item.completedCount}/${esc(item.requiredCountText)}</td>
        <td>${item.remainingCount}</td>
      </tr>
      ${isExpanded ? `
      <tr class="row-details">
        <td colspan="4" class="row-details-cell">
          ${renderExpandedTargetList(item)}
        </td>
      </tr>` : ""}`;
    })
    .join("");

  fullPanel.classList.remove("hidden");
}

// Helpers
function friendlyType(rawType) {
  if (!rawType) return "Unknown";
  const t = rawType.toLowerCase();
  if (t.includes("killunit")) return "Boss";
  if (t.includes("killguid")) return "Elite";
  if (t.includes("entermap")) return "Explore";
  if (t.includes("finishtask") || t.includes("activatetask")) return "Quest";
  if (t.includes("gainitem")) return "Item";
  if (t.includes("gainequip")) return "Gear";
  if (t.includes("gainspell")) return "Spell";
  if (t.includes("gainwine")) return "Wine";
  if (t.includes("gainsoulskill")) return "Spirit";
  if (t.includes("gainlegacy")) return "Legacy";
  if (t.includes("gainallattr")) return "Attribute";
  if (t.includes("buildarmor")) return "Forge Armor";
  if (t.includes("buildweapon")) return "Forge Weapon";
  if (t.includes("alchemy")) return "Alchemy";
  if (t.includes("achievementcomplete")) return "Meta";
  if (t.includes("unlockmeditation")) return "Meditation";
  if (t.includes("pass")) return "Progress";
  return rawType.replace(/^(NoProgress|Progress)/i, "").replace(/([A-Z])/g, " $1").trim();
}

function esc(value) {
  return String(value)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#39;");
}
