const MAX_SAVE_BYTES = 8 * 1024 * 1024;
const EXPECTED_ACHIEVEMENT_COUNT = 81;

const uploadForm = document.getElementById("uploadForm");
const saveFileInput = document.getElementById("saveFile");
const dropZone = document.getElementById("dropZone");
const selectedFilePanel = document.getElementById("selectedFile");
const analyzeBtn = document.getElementById("analyzeBtn");
const statusPanel = document.getElementById("statusPanel");
const results = document.getElementById("results");
const completionCeremony = document.getElementById("completionCeremony");
const completionPlayer = document.getElementById("completionPlayer");
const progressArc = document.getElementById("progressArc");
const progressPct = document.getElementById("progressPct");
const overviewNarrative = document.getElementById("overviewNarrative");
const nextStepsList = document.getElementById("nextStepsList");
const trackerCount = document.getElementById("trackerCount");
const trackerList = document.getElementById("trackerList");
const spoilerToggleBtn = document.getElementById("spoilerToggleBtn");
const searchInput = document.getElementById("searchInput");
const statusFilters = document.getElementById("statusFilters");
const categoryFilter = document.getElementById("categoryFilter");
const chapterFilter = document.getElementById("chapterFilter");
const libraryCount = document.getElementById("libraryCount");
const expandVisibleBtn = document.getElementById("expandVisibleBtn");
const achievementList = document.getElementById("achievementList");
const emptyState = document.getElementById("emptyState");

let selectedFile = null;
let currentReport = null;
let activeRequest = null;
let currentStatusFilter = "all";
let hideSpoilers = loadSpoilerPreference();
const revealedGuideIds = new Set();
const openGuideIds = new Set();

uploadForm.addEventListener("submit", analyzeSave);

saveFileInput.addEventListener("change", () => {
  chooseFile(saveFileInput.files?.[0] ?? null);
});

["dragenter", "dragover"].forEach((eventName) => {
  dropZone.addEventListener(eventName, (event) => {
    event.preventDefault();
    dropZone.classList.add("is-dragging");
  });
});

["dragleave", "drop"].forEach((eventName) => {
  dropZone.addEventListener(eventName, (event) => {
    event.preventDefault();
    dropZone.classList.remove("is-dragging");
  });
});

dropZone.addEventListener("drop", (event) => {
  const files = Array.from(event.dataTransfer?.files ?? []);
  if (files.length !== 1) {
    chooseFile(null);
    setStatus("Drop exactly one .sav file.", "error", true);
    return;
  }

  chooseFile(files[0]);
});

statusFilters.addEventListener("click", (event) => {
  const button = event.target.closest("[data-status-filter]");
  if (!button) return;

  currentStatusFilter = button.dataset.statusFilter;
  statusFilters.querySelectorAll("button").forEach((candidate) => {
    candidate.setAttribute(
      "aria-pressed",
      candidate === button ? "true" : "false"
    );
  });
  renderAchievementLibrary();
});

searchInput.addEventListener("input", renderAchievementLibrary);
categoryFilter.addEventListener("change", renderAchievementLibrary);
chapterFilter.addEventListener("change", renderAchievementLibrary);

spoilerToggleBtn.addEventListener("click", () => {
  rememberOpenGuides();
  hideSpoilers = !hideSpoilers;
  if (hideSpoilers) {
    revealedGuideIds.clear();
  }
  saveSpoilerPreference(hideSpoilers);
  syncSpoilerButton();
  renderSpoilerSensitiveViews();
});

achievementList.addEventListener("click", (event) => {
  const revealButton = event.target.closest("[data-reveal-guide]");
  if (!revealButton) return;

  const achievementId = Number(revealButton.dataset.revealGuide);
  if (!Number.isFinite(achievementId)) return;

  revealedGuideIds.add(achievementId);
  openGuideIds.add(achievementId);
  renderSpoilerSensitiveViews();
  document
    .getElementById(`achievement-${achievementId}`)
    ?.querySelector("summary")
    ?.focus();
});

achievementList.addEventListener(
  "toggle",
  (event) => {
    const details = event.target.closest("details[data-guide-id]");
    if (!details) return;

    const achievementId = Number(details.dataset.guideId);
    if (!Number.isFinite(achievementId)) return;

    if (details.open) {
      openGuideIds.add(achievementId);
    } else {
      openGuideIds.delete(achievementId);
    }
    syncExpandButton();
  },
  true
);

expandVisibleBtn.addEventListener("click", () => {
  const visibleGuides = Array.from(
    achievementList.querySelectorAll("details[data-guide-id]")
  );
  const shouldOpen = visibleGuides.some((details) => !details.open);

  visibleGuides.forEach((details) => {
    details.open = shouldOpen;
    const achievementId = Number(details.dataset.guideId);
    if (!Number.isFinite(achievementId)) return;

    if (shouldOpen) {
      openGuideIds.add(achievementId);
    } else {
      openGuideIds.delete(achievementId);
    }
  });
  syncExpandButton();
});

function chooseFile(file) {
  activeRequest?.abort();
  activeRequest = null;
  selectedFile = file;
  currentReport = null;
  renderCompletionState(null);
  results.classList.add("hidden");
  revealedGuideIds.clear();
  openGuideIds.clear();

  if (!file) {
    selectedFilePanel.classList.add("hidden");
    selectedFilePanel.replaceChildren();
    analyzeBtn.disabled = true;
    return;
  }

  const validationError = validateFile(file);
  if (validationError) {
    selectedFile = null;
    selectedFilePanel.classList.add("hidden");
    selectedFilePanel.replaceChildren();
    analyzeBtn.disabled = true;
    setStatus(validationError, "error", true);
    return;
  }

  selectedFilePanel.innerHTML = `
    <strong>${esc(file.name)}</strong>
    <span>${esc(formatBytes(file.size))}</span>`;
  selectedFilePanel.classList.remove("hidden");
  analyzeBtn.disabled = false;
  hideStatus();
}

function validateFile(file) {
  if (!file.name.toLowerCase().endsWith(".sav")) {
    return "Choose a Black Myth: Wukong file ending in .sav.";
  }

  if (file.size === 0) {
    return "That save file is empty.";
  }

  if (file.size > MAX_SAVE_BYTES) {
    return "That save is larger than the 8 MB upload limit.";
  }

  return null;
}

async function analyzeSave(event) {
  event.preventDefault();
  if (!selectedFile) {
    setStatus("Choose a .sav file first.", "error", true);
    return;
  }

  const validationError = validateFile(selectedFile);
  if (validationError) {
    setStatus(validationError, "error", true);
    return;
  }

  activeRequest?.abort();
  renderCompletionState(null);
  const controller = new AbortController();
  activeRequest = controller;
  const analyzedFile = selectedFile;

  analyzeBtn.disabled = true;
  analyzeBtn.innerHTML = "<span>Reading the save…</span><span aria-hidden=\"true\">◌</span>";
  setStatus("Uploading and decoding the save in memory…");

  try {
    const formData = new FormData();
    formData.append("saveFile", analyzedFile, analyzedFile.name);

    const response = await fetch("/api/analyze", {
      method: "POST",
      body: formData,
      cache: "no-store",
      signal: controller.signal,
    });
    const bodyText = await response.text();
    const payload = parseJsonResponse(bodyText);

    if (!response.ok || !payload?.ok) {
      throw new Error(
        payload?.error ||
          (response.status === 413
            ? "That upload exceeds the server's size limit."
            : "The server could not analyze this save.")
      );
    }

    if (controller !== activeRequest) return;

    currentReport = payload.report;
    populateFilters(currentReport.achievements ?? []);
    renderAll(currentReport);
    results.classList.remove("hidden");

    const countWarning = currentReport.totalAchievements === 81
      ? ""
      : ` The decoder returned ${currentReport.totalAchievements} guide rows instead of 81.`;
    setStatus(
      `Analyzed ${payload.saveFileName || analyzedFile.name}: ${currentReport.completedAchievements}/81 achievements complete.${countWarning}`
    );
  } catch (error) {
    if (error.name === "AbortError") return;

    currentReport = null;
    renderCompletionState(null);
    results.classList.add("hidden");
    setStatus(error.message || "The save could not be analyzed.", "error", true);
  } finally {
    if (controller === activeRequest) {
      activeRequest = null;
      analyzeBtn.disabled = selectedFile === null;
      analyzeBtn.innerHTML = "<span>Read my journey</span><span aria-hidden=\"true\">→</span>";
    }
  }
}

function parseJsonResponse(text) {
  if (!text) return null;

  try {
    return JSON.parse(text);
  } catch {
    return null;
  }
}

function setStatus(message, type = "ok", shouldFocus = false) {
  statusPanel.textContent = message;
  statusPanel.classList.remove("hidden", "status-error");
  statusPanel.classList.toggle("status-error", type === "error");
  statusPanel.setAttribute("role", type === "error" ? "alert" : "status");
  if (shouldFocus) statusPanel.focus();
}

function hideStatus() {
  statusPanel.classList.add("hidden");
  statusPanel.textContent = "";
}

function renderAll(report) {
  renderCompletionState(report);
  renderOverview(report);
  syncSpoilerButton();
  renderSpoilerSensitiveViews();
}

function renderCompletionState(report) {
  const journeyComplete = isJourneyComplete(report);

  completionCeremony.hidden = !journeyComplete;
  results.classList.toggle("is-journey-complete", journeyComplete);
  results.classList.toggle("is-celebrating", journeyComplete);

  if (!journeyComplete) return;

  completionPlayer.textContent = report.playerName || "The Destined One";
}

function isJourneyComplete(report) {
  const achievements = Array.isArray(report?.achievements) ? report.achievements : [];

  return (
    Number(report?.totalAchievements) === EXPECTED_ACHIEVEMENT_COUNT &&
    Number(report?.completedAchievements) === EXPECTED_ACHIEVEMENT_COUNT &&
    Number(report?.incompleteAchievements) === 0 &&
    achievements.length === EXPECTED_ACHIEVEMENT_COUNT &&
    achievements.every((item) => item?.isComplete === true)
  );
}

function renderSpoilerSensitiveViews() {
  if (!currentReport) return;

  renderNextSteps(currentReport);
  renderTracker(currentReport);
  renderAchievementLibrary();
}

function renderOverview(report) {
  const total = Math.max(Number(report.totalAchievements) || 0, 1);
  const completed = Math.max(Number(report.completedAchievements) || 0, 0);
  const percent = Math.round((completed / total) * 100);
  const circumference = 2 * Math.PI * 47;
  const offset = circumference - (percent / 100) * circumference;

  progressArc.style.strokeDasharray = String(circumference);
  progressArc.style.strokeDashoffset = String(offset);
  progressPct.textContent = `${percent}%`;
  document.getElementById("ovPlayer").textContent = report.playerName || "Unknown";
  document.getElementById("ovLevel").textContent = String(report.playerLevel ?? "—");
  document.getElementById("ovNgPlus").textContent = Number(report.newGamePlusCount) > 0
    ? `NG+${report.newGamePlusCount}`
    : "First";
  document.getElementById("ovAchievements").textContent = `${completed} / ${total}`;
  document.getElementById("ovMissing").textContent = String(report.incompleteAchievements ?? 0);

  const remaining = Number(report.incompleteAchievements) || 0;
  overviewNarrative.textContent = remaining === 0
    ? "Every recorded ordeal is complete. The ledger is whole."
    : `${remaining} ordeal${remaining === 1 ? "" : "s"} remain. Missable routes are marked in cinnabar.`;
}

function renderNextSteps(report) {
  const newGamePlusCount = Number(report.newGamePlusCount) || 0;
  const candidates = (report.achievements ?? [])
    .filter((item) => !item.isComplete && item.achievementId !== 81081)
    .sort((left, right) => {
      const leftStage = chapterRouteRank(
        left,
        normalizeChapterNumber(report.currentChapterId),
        newGamePlusCount
      );
      const rightStage = chapterRouteRank(
        right,
        normalizeChapterNumber(report.currentChapterId),
        newGamePlusCount
      );
      if (leftStage !== rightStage) return leftStage - rightStage;
      if (left.isMissable !== right.isMissable) return left.isMissable ? -1 : 1;
      if (left.priorityOrder !== right.priorityOrder) {
        return (left.priorityOrder ?? 99) - (right.priorityOrder ?? 99);
      }
      return left.achievementId - right.achievementId;
    })
    .slice(0, 3);

  if (candidates.length === 0) {
    const journeyComplete = isJourneyComplete(report);
    nextStepsList.innerHTML = `
      <div class="completion-banner">
        <strong>${journeyComplete ? "No further route is needed." : "The final fulfillment remains."}</strong>
        <p>${
          journeyComplete
            ? "The final seal above marks every achievement complete."
            : "Open Ordeal 81 in the full ledger to review the last platform trigger."
        }</p>
      </div>`;
    return;
  }

  nextStepsList.innerHTML = candidates
    .map(
      (item, index) => {
        const spoilerHidden = isAchievementSpoilerHidden(item);
        return `
        <article class="next-card" data-order="${index + 1}">
          <span class="next-card-number">Move ${index + 1} · ${esc(item.chapter)}</span>
          <h3>${esc(item.displayTitle)}</h3>
          <p${spoilerHidden ? ' class="spoiler-placeholder"' : ""}>${
            spoilerHidden
              ? "Achievement description hidden while spoiler protection is on."
              : esc(item.requirementSummary || item.routeHint)
          }</p>
          <a href="#achievement-${item.achievementId}">Open this guide →</a>
        </article>`;
      }
    )
    .join("");
}

function chapterRouteRank(item, currentChapter, newGamePlusCount) {
  if (item.requiresNewGamePlus && newGamePlusCount === 0) return 5;

  const label = String(item.chapter || "");
  if (/new game/i.test(label)) return newGamePlusCount > 0 ? 1 : 5;
  if (/endgame/i.test(label)) return currentChapter >= 6 ? 0 : 4;
  if (/all chapters/i.test(label)) return 1;
  if (/prologue/i.test(label)) return currentChapter <= 1 ? 0 : 2;

  const chapterMatch = label.match(/Chapters?\s+(\d)(?:\s*[-–]\s*(\d))?/i);
  if (!chapterMatch) return 3;

  const firstChapter = Number(chapterMatch[1]);
  const lastChapter = Number(chapterMatch[2] || chapterMatch[1]);
  if (currentChapter >= firstChapter && currentChapter <= lastChapter) return 0;
  if (lastChapter < currentChapter) return 2;
  return 3;
}

function normalizeChapterNumber(rawChapterId) {
  const raw = Number(rawChapterId) || 0;
  if (raw >= 10) return Math.floor(raw / 10);
  return raw;
}

function renderTracker(report) {
  const trackedAchievements = (report.achievements ?? [])
    .filter((item) => !item.isComplete)
    .map((item) => ({
      item,
      missing: Array.isArray(item.missingTargets) ? item.missingTargets : [],
    }))
    .filter((entry) => entry.missing.length > 0)
    .sort((left, right) => {
      if (right.missing.length !== left.missing.length) {
        return right.missing.length - left.missing.length;
      }
      return left.item.achievementId - right.item.achievementId;
    });

  const totalMissing = trackedAchievements.reduce(
    (sum, entry) => sum + entry.missing.length,
    0
  );
  trackerCount.textContent = `${totalMissing} missing`;

  if (trackedAchievements.length === 0) {
    trackerList.innerHTML = `
      <div class="tracker-empty">
        No save-verified collection items are missing. Guide-only cleanup may still remain.
      </div>`;
    return;
  }

  trackerList.innerHTML = trackedAchievements
    .map(
      ({ item, missing }) => {
        const spoilerHidden = isAchievementSpoilerHidden(item);
        return `
        <details class="tracker-group">
          <summary>
            <span class="tracker-group-title">
              <strong>${esc(item.displayTitle)}</strong>
              <small>${missing.length} of ${(item.requirementTargets ?? []).length} tracked rows missing</small>
            </span>
          </summary>
          ${
            spoilerHidden
              ? `<div class="spoiler-gate spoiler-gate-compact">
                  <p>Missing item names and locations are hidden while spoiler protection is on.</p>
                </div>`
              : `<ul class="tracker-items">
                  ${missing
                    .map(
                      (target) => `
                        <li class="tracker-item">
                          <strong>${esc(target.name)}</strong>
                          ${target.howToGet ? `<p>${esc(target.howToGet)}</p>` : ""}
                        </li>`
                    )
                    .join("")}
                </ul>`
          }
        </details>`;
      }
    )
    .join("");
}

function populateFilters(achievements) {
  const categories = uniqueInOrder(
    achievements.map((item) => item.category).filter(Boolean)
  );
  const chapters = uniqueInOrder(
    achievements.map((item) => item.chapter).filter(Boolean)
  );

  replaceSelectOptions(categoryFilter, "Every category", categories);
  replaceSelectOptions(chapterFilter, "Every chapter", chapters);
}

function replaceSelectOptions(select, allLabel, values) {
  const allOption = new Option(allLabel, "all");
  select.replaceChildren(allOption, ...values.map((value) => new Option(value, value)));
}

function uniqueInOrder(values) {
  return [...new Set(values)];
}

function renderAchievementLibrary() {
  if (!currentReport) return;
  rememberOpenGuides();

  const query = searchInput.value.trim().toLocaleLowerCase();
  const category = categoryFilter.value;
  const chapter = chapterFilter.value;
  const allAchievements = currentReport.achievements ?? [];
  const visible = allAchievements.filter((item) => {
    if (currentStatusFilter === "complete" && !item.isComplete) return false;
    if (currentStatusFilter === "incomplete" && item.isComplete) return false;
    if (category !== "all" && item.category !== category) return false;
    if (chapter !== "all" && item.chapter !== chapter) return false;
    if (query && !buildSearchText(item).includes(query)) return false;
    return true;
  });

  libraryCount.textContent = `Showing ${visible.length} of ${allAchievements.length} achievements`;
  emptyState.classList.toggle("hidden", visible.length !== 0);
  achievementList.innerHTML = visible.map(renderAchievementCard).join("");
  syncExpandButton();
}

function buildSearchText(item) {
  const parts = [
    item.displayTitle,
    item.category,
    item.chapter,
  ];

  if (!isAchievementSpoilerHidden(item)) {
    parts.push(
      item.requirementSummary,
      item.routeHint,
      item.missableNote,
      ...(item.prerequisites ?? []),
      ...(item.guideSteps ?? []),
      ...(item.guideChecklist ?? [])
    );

    (item.requirementTargets ?? []).forEach((target) => {
      parts.push(target.name, target.howToGet);
    });
  }

  return parts.filter(Boolean).join(" ").toLocaleLowerCase();
}

function renderAchievementCard(item) {
  const ordeal = String(item.achievementId - 81000).padStart(2, "0");
  const progress = getProgress(item);
  const guideHidden = isAchievementSpoilerHidden(item);
  const isOpen = openGuideIds.has(item.achievementId);
  const classes = [
    "achievement-card",
    item.isComplete ? "is-complete" : "is-incomplete",
    item.isMissable ? "is-missable" : "",
  ]
    .filter(Boolean)
    .join(" ");

  return `
    <article id="achievement-${item.achievementId}" class="${classes}">
      <div class="achievement-card-main">
        <div class="achievement-topline">
          <span class="ordeal-number">Ordeal ${ordeal}</span>
          <span class="status-label">${item.isComplete ? "Complete" : "Remaining"}</span>
        </div>
        <h3>${esc(item.displayTitle)}</h3>
        <p class="requirement-summary${guideHidden ? " spoiler-placeholder" : ""}">${
          guideHidden
            ? "Achievement description hidden while spoiler protection is on."
            : esc(item.requirementSummary || item.routeHint)
        }</p>
        <div class="tag-row">
          <span class="tag">${esc(item.category)}</span>
          <span class="tag">${esc(item.chapter)}</span>
          ${item.isMissable ? '<span class="tag tag-missable">Missable</span>' : ""}
          ${item.requiresNewGamePlus ? '<span class="tag tag-ng">New Game+</span>' : ""}
          ${!item.isPresentInSave ? '<span class="tag tag-unverified">Guide-only progress</span>' : ""}
        </div>
        <div class="achievement-progress" aria-label="${esc(progress.label)}">
          <div class="progress-track"><div class="progress-fill" style="width:${progress.percent}%"></div></div>
          <span class="progress-copy">${esc(progress.label)}</span>
        </div>
      </div>
      <details class="achievement-guide" data-guide-id="${item.achievementId}" ${isOpen ? "open" : ""}>
        <summary>Open full requirement guide</summary>
        <div class="guide-body">
          ${
            guideHidden
              ? renderSpoilerGate(item)
              : renderGuideContent(item)
          }
        </div>
      </details>
    </article>`;
}

function getProgress(item) {
  if (item.isComplete) return { percent: 100, label: "Complete" };

  if (item.requiredCount > 0) {
    const completed = Math.max(Number(item.completedCount) || 0, 0);
    const required = Math.max(Number(item.requiredCount) || 1, 1);
    return {
      percent: Math.min(100, Math.round((completed / required) * 100)),
      label: `${completed} / ${required}`,
    };
  }

  return {
    percent: 0,
    label: item.isPresentInSave ? "Trigger pending" : "Not exposed yet",
  };
}

function isAchievementSpoilerHidden(item) {
  return hideSpoilers && !revealedGuideIds.has(item.achievementId);
}

function renderSpoilerGate(item) {
  return `
    <div class="spoiler-gate">
      <div>
        <p>The achievement description, route details, and collectible locations are hidden.</p>
        <button class="reveal-button" type="button" data-reveal-guide="${item.achievementId}">
          Reveal ${esc(item.displayTitle)} guide
        </button>
      </div>
    </div>`;
}

function renderGuideContent(item) {
  const prerequisites = Array.isArray(item.prerequisites) ? item.prerequisites : [];
  const guideSteps = Array.isArray(item.guideSteps) ? item.guideSteps : [];
  const guideChecklist = Array.isArray(item.guideChecklist) ? item.guideChecklist : [];
  const targets = Array.isArray(item.requirementTargets) ? item.requirementTargets : [];
  const additionalMilestones = getAdditionalGuideMilestones(guideChecklist, targets);

  return `
    <div class="guide-layout">
      <div class="guide-column">
        <section class="guide-section">
          <h4 class="guide-label">Exact route</h4>
          <p>${esc(item.routeHint || item.requirementSummary)}</p>
        </section>
        ${
          item.isMissable && item.missableNote
            ? `<section class="guide-section warning-box">
                <h4 class="guide-label">Do this before moving on</h4>
                <p>${esc(item.missableNote)}</p>
              </section>`
            : ""
        }
        ${
          prerequisites.length
            ? `<section class="guide-section">
                <h4 class="guide-label">Prerequisites</h4>
                <ul class="guide-list">${prerequisites.map((step) => `<li>${esc(step)}</li>`).join("")}</ul>
              </section>`
            : ""
        }
        ${
          guideSteps.length
            ? `<section class="guide-section">
                <h4 class="guide-label">Walkthrough</h4>
                <ol class="guide-list">${guideSteps.map((step) => `<li>${esc(step)}</li>`).join("")}</ol>
              </section>`
            : ""
        }
      </div>
      <div class="guide-column">
        ${targets.length ? renderTargetChecklist(targets) : ""}
        ${additionalMilestones.length ? renderGuideMilestones(additionalMilestones, targets.length > 0) : ""}
        ${
          !additionalMilestones.length && !targets.length
            ? `<section class="guide-section">
                <h4 class="guide-label">Completion check</h4>
                <p>This is a single trigger achievement. Follow the route on the left; the uploaded save supplies the final complete / incomplete state.</p>
              </section>`
            : ""
        }
      </div>
    </div>`;
}

function getAdditionalGuideMilestones(entries, targets) {
  if (!targets.length) return entries;

  const targetNames = new Set(targets.map((target) => normalizeRequirementName(target.name)));
  return entries.filter((entry) => !targetNames.has(normalizeRequirementName(entry)));
}

function normalizeRequirementName(value) {
  return String(value ?? "")
    .toLowerCase()
    .replace(/^(?:soak|spirit(?: skill)?):\s*/, "")
    .replace(/\s*\([^)]*\)\s*$/, "")
    .replace(/[^a-z0-9]+/g, "");
}

function renderGuideMilestones(entries, hasTrackedTargets) {
  const heading = hasTrackedTargets ? "Additional route milestones" : "Route milestones";
  const note = hasTrackedTargets
    ? "These notes add route context; automatically checked requirements are shown above."
    : "These are route notes. The uploaded save exposes the overall achievement result, not a separate state for each step.";

  return `
    <section class="guide-section">
      <h4 class="guide-label">${heading}</h4>
      <p class="tracking-note">${note}</p>
      <ul class="guide-list">${entries.map((entry) => `<li>${esc(entry)}</li>`).join("")}</ul>
    </section>`;
}

function renderTargetChecklist(targets) {
  const collected = targets.filter((target) => target.isCollected).length;
  return `
    <section class="guide-section">
      <h4 class="guide-label">Requirements from your save · ${collected}/${targets.length} complete</h4>
      <p class="tracking-note">The uploaded save automatically marks each requirement collected or missing.</p>
      <ul class="target-list">
        ${targets
          .map(
            (target) => `
              <li class="target-row ${target.isCollected ? "is-owned" : "is-missing"}">
                <span class="target-mark" aria-hidden="true">${target.isCollected ? "✓" : "×"}</span>
                <span>
                  <strong>${esc(target.name)}</strong>
                  <span class="target-state">${target.isCollected ? "Collected" : "Missing"}</span>
                  ${target.howToGet ? `<small>${esc(target.howToGet)}</small>` : ""}
                </span>
              </li>`
          )
          .join("")}
      </ul>
    </section>`;
}

function rememberOpenGuides() {
  achievementList.querySelectorAll("details[data-guide-id]").forEach((details) => {
    const achievementId = Number(details.dataset.guideId);
    if (!Number.isFinite(achievementId)) return;
    if (details.open) openGuideIds.add(achievementId);
    else openGuideIds.delete(achievementId);
  });
}

function syncExpandButton() {
  const guides = Array.from(achievementList.querySelectorAll("details[data-guide-id]"));
  const allOpen = guides.length > 0 && guides.every((details) => details.open);
  expandVisibleBtn.textContent = allOpen ? "Collapse visible guides" : "Expand visible guides";
  expandVisibleBtn.disabled = guides.length === 0;
}

function syncSpoilerButton() {
  spoilerToggleBtn.setAttribute("aria-pressed", hideSpoilers ? "true" : "false");
  spoilerToggleBtn.textContent = hideSpoilers
    ? "Show achievement spoilers"
    : "Hide achievement spoilers";
}

function loadSpoilerPreference() {
  try {
    return window.localStorage.getItem("journey-ledger.hide-spoilers") === "true";
  } catch {
    return false;
  }
}

function saveSpoilerPreference(value) {
  try {
    window.localStorage.setItem(
      "journey-ledger.hide-spoilers",
      value ? "true" : "false"
    );
  } catch {
    // Local storage is optional; the current session still works without it.
  }
}

function formatBytes(bytes) {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(2)} MB`;
}

function esc(value) {
  return String(value ?? "")
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#39;");
}

syncSpoilerButton();
