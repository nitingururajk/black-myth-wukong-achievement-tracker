// English remains the source language and the API contract.
let language = "en";
try { language = localStorage.getItem("journey-ledger.language") === "zh-CN" ? "zh-CN" : "en"; } catch { /* Storage is optional. */ }
const uiZh = {
  "Skip to achievement guide": "跳转到成就攻略",
  "The Destined One’s Journey Ledger": "天命人的取经行记",
  "Every ordeal.": "八十一难。",
  "One clear path.": "一条明路。",
  "Upload a save to turn all 81 achievements into a personal route—complete with missables, New Game+ gates, exact collection checklists, and practical next steps.": "上传存档，规划属于你的八十一难之路：易错过提醒、再入轮回要求、精确收集清单与下一步攻略，一目了然。",
  "81 guided achievements": "81项成就攻略",
  "Save-aware checklists": "按存档核对收集进度",
  "Beginner-friendly routes": "新手也能轻松跟进",
  "Tracker features": "追踪器功能",
  "Begin your reading": "展开行记",
  "Choose your save": "选择存档",
  "Drop a": "拖入",
  "here": "文件",
  "or choose one from your device · 8 MB maximum": "或从设备选择文件 · 最大 8 MB",
  "Read my journey": "查看我的行记",
  "Reading the save…": "正在读取存档…",
  "Your save is decoded in memory for this request and is never written to disk.": "存档仅在本次请求的内存中解析，不会写入磁盘。",
  "Where is my save?": "存档在哪里？",
  "Back up the file before uploading it. The common Windows location is:": "上传前请备份存档。Windows 上的常见位置为：",
  "Pick the slot you actually play, not a backup or screenshot file.": "选择你正在游玩的存档槽位，不要选择备份或截图。",
  "The final seal is set": "功成印落",
  "All eighty-one ordeals fulfilled": "九九归真，难满功成",
  "The Destined One": "天命人",
  "’s journey is written whole. The ledger bears no unfinished path.": "的行记已圆满，八十一难尽数完成。",
  "81 / 81 · Nothing remains unwritten": "81 / 81 · 全始全终",
  "Your current pilgrimage": "此刻的取经之路",
  "’s journey": "的行记",
  "Unknown": "未知",
  "Level": "等级",
  "Cycle": "轮回",
  "First": "首轮",
  "Complete": "已完成",
  "Remaining": "未完成",
  "Next steps": "下一步",
  "Missing items": "缺失物品",
  "All 81": "全部81难",
  "Result sections": "结果导航",
  "Recommended route": "推荐路线",
  "The next three moves": "接下来做这三件事",
  "Start here, then use the full ledger for chapter-by-chapter cleanup.": "从这里开始，再对照完整行记逐回查漏补缺。",
  "Save-verified collection gaps": "根据存档核对缺项",
  "Missing collectibles": "尚未收集的物品",
  "These rows come from decoded requirement IDs or inventory ownership. Guide-only steps are labeled separately in each achievement card.": "清单根据存档中的成就要求或物品持有情况核对。仅供攻略参考的步骤，会在成就卡片内单独标明。",
  "Complete achievement compendium": "完整成就图鉴",
  "All 81 ordeals": "八十一难",
  "Hide achievement spoilers": "隐藏成就剧透",
  "Show achievement spoilers": "显示成就剧透",
  "Achievement filters": "成就筛选",
  "Search the achievement guide": "搜索成就攻略",
  "Search achievements, items, bosses, or locations…": "搜索成就、物品、头目或地点…",
  "Filter by completion": "按完成状态筛选",
  "All": "全部",
  "Filter by category": "按类别筛选",
  "Every category": "全部类别",
  "Filter by chapter": "按章节筛选",
  "Every chapter": "全部章节",
  "Expand visible guides": "展开当前攻略",
  "Collapse visible guides": "收起当前攻略",
  "No ordeals match those filters": "没有符合条件的成就",
  "Try a broader search or reset one of the filters.": "试试更宽泛的关键词，或重置筛选条件。",
  "Journey Ledger": "取经行记",
  "An independent save-analysis companion for Black Myth: Wukong.": "《黑神话：悟空》非官方存档解析助手。",
  "Journey Ledger · Black Myth: Wukong Achievement Guide": "取经行记 · 黑神话：悟空成就攻略",
  "Analyze a Black Myth: Wukong save and follow a complete, beginner-friendly guide to all 81 achievements.": "解析《黑神话：悟空》存档，查看易于上手的完整八十一难成就攻略。",
  "Drop exactly one .sav file.": "请只拖入一个 .sav 存档。",
  "Choose a Black Myth: Wukong file ending in .sav.": "请选择扩展名为 .sav 的《黑神话：悟空》存档。",
  "That save file is empty.": "该存档文件为空。",
  "That save is larger than the 4 MB upload limit.": "该存档超过 4 MB 上传限制。",
  "Choose a .sav file first.": "请先选择 .sav 存档。",
  "Uploading and decoding the save in memory…": "正在上传并在内存中解析存档…",
  "That upload exceeds the server's size limit.": "该文件超过服务器的上传大小限制。",
  "The server could not analyze this save.": "服务器未能解析该存档。",
  "The save could not be analyzed.": "无法解析该存档。",
  "Upload one Black Myth: Wukong .sav file.": "请上传一个《黑神话：悟空》.sav 存档。",
  "Choose exactly one .sav file to analyze.": "请只选择一个 .sav 存档进行解析。",
  "That file is not a .sav file.": "该文件不是 .sav 存档。",
  "The uploaded save file is empty.": "上传的存档为空。",
  "The uploaded save file is too large. The limit is 4 MB.": "上传的存档过大，限制为 4 MB。",
  "The upload is too large. Choose a .sav file no larger than 4 MB.": "上传文件过大，请选择不超过 4 MB 的 .sav 存档。",
  "This file could not be decoded as a Black Myth: Wukong save. Make sure it is an unmodified .sav file and try again.": "无法将此文件解析为《黑神话：悟空》存档。请确认它是未经修改的 .sav 文件后重试。",
  "Every recorded ordeal is complete. The ledger is whole.": "所有记录的成就均已完成，行记圆满。",
  "No further route is needed.": "旅途已圆满。",
  "The final fulfillment remains.": "尚待最后一难。",
  "The final seal above marks every achievement complete.": "上方的功成印记代表全部成就均已完成。",
  "Open Ordeal 81 in the full ledger to review the last platform trigger.": "打开第81难攻略，查看最后的平台解锁条件。",
  "Achievement description hidden while spoiler protection is on.": "已开启剧透保护，成就说明已隐藏。",
  "Open this guide →": "查看攻略 →",
  "No save-verified collection items are missing. Guide-only cleanup may still remain.": "存档可核对的收集项已无缺失，仍可能有需要按攻略查漏的内容。",
  "Missing item names and locations are hidden while spoiler protection is on.": "已开启剧透保护，缺失物品的名称与位置已隐藏。",
  "Missable": "易错过",
  "New Game+": "再入轮回",
  "Guide-only progress": "仅供攻略参考",
  "Open full requirement guide": "展开完整成就攻略",
  "Trigger pending": "等待触发",
  "Not exposed yet": "存档暂未记录",
  "The achievement description, route details, and collectible locations are hidden.": "成就说明、路线细节和收集位置已隐藏。",
  "Exact route": "获取路线",
  "Do this before moving on": "继续前务必完成",
  "Prerequisites": "前置条件",
  "Walkthrough": "详细步骤",
  "Completion check": "完成确认",
  "This is a single trigger achievement. Follow the route on the left; the uploaded save supplies the final complete / incomplete state.": "此成就由单次事件触发。请按攻略路线完成，最终状态以所上传的存档为准。",
  "Additional route milestones": "其他路线要点",
  "Route milestones": "路线要点",
  "These notes add route context; automatically checked requirements are shown above.": "以下内容补充路线信息；自动核对的要求见上方清单。",
  "These are route notes. The uploaded save exposes the overall achievement result, not a separate state for each step.": "以下为攻略参考。存档只记录成就整体结果，不会单独标记每个步骤。",
  "The uploaded save automatically marks each requirement collected or missing.": "根据上传的存档，自动标记各项已收集或缺失。",
  "Collected": "已收集",
  "Missing": "缺失",
  "Story": "主线", "Story Boss": "主线头目", "Crafting": "铸造炼制", "Combat": "战斗", "Exploration": "探索",
  "Secret Boss": "隐藏头目", "Boss Chain": "系列头目", "Side Quest": "支线", "Optional Boss": "可选头目",
  "Collection": "收集", "Secret Area": "隐藏区域", "Upgrade": "升级", "Completion": "全收集", "Progression": "流程",
  "Story Collection": "主线收集", "Boss Cleanup": "头目查漏", "Prologue": "序章", "Endgame": "通关阶段", "All Chapters": "全部章节"
};
function t(text) { return language === "zh-CN" ? (uiZh[text] ?? text) : text; }
function chapterLabel(value) {
  if (language !== "zh-CN") return value;
  return uiZh[value] ?? String(value).replace(/^Chapters? (\d)(?:[-–](\d))?$/, (_, first, last) => last ? `第${first}—${last}回` : `第${first}回`);
}
function guideText(item, field) {
  if (language === "zh-CN" && field === "routeHint") {
    return zhGuides[item.achievementId]?.routeHint ?? zhGuides[item.achievementId]?.requirementSummary ?? item[field];
  }
  return language === "zh-CN" ? (zhGuides[item.achievementId]?.[field] ?? item[field]) : item[field];
}
function targetText(item, target, field) {
  return language === "zh-CN" ? (zhTargets[`${item.achievementId}:${target.id}`]?.[field] ?? target[field]) : target[field];
}

// Capture only the original static text nodes. Player names, filenames and API values
// are never run through this dictionary or replaced by HTML translation strings.
const staticTranslations = [];
const walker = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT);
while (walker.nextNode()) {
  const node = walker.currentNode;
  if (node.parentElement.closest("script, style, code")) continue;
  const key = node.textContent.trim().replace(/\s+/g, " ");
  if (uiZh[key]) staticTranslations.push({ node, original: node.textContent, key });
}
const staticAttributes = [];
document.querySelectorAll("[aria-label], [placeholder], meta[name=description]").forEach(node => {
  for (const name of ["aria-label", "placeholder", "content"]) {
    const value = node.getAttribute(name);
    if (value && uiZh[value]) staticAttributes.push({ node, name, value });
  }
});
const englishTitle = document.title;
function translateStaticPage() {
  document.documentElement.lang = language;
  document.title = t(englishTitle);
  for (const { node, original, key } of staticTranslations) node.textContent = language === "zh-CN" ? original.replace(original.trim(), t(key)) : original;
  for (const { node, name, value } of staticAttributes) node.setAttribute(name, t(value));
}
