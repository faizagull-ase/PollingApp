const HUB_URL = window.appConfig.hubUrl;

let connection = null;
let pollCode = null;
let currentQuestionIndex = 0;
let questionCount = 0;
let currentOptions = [];
let questionsBuilt = []; // client-side record of each question seen, for the results panel
let resultsByIndex = {};

const questionsContainer = document.getElementById("questions-container");
const setupCard = document.getElementById("setup-card");
const liveCard = document.getElementById("live-card");
const resultsCard = document.getElementById("results-card");
const setupError = document.getElementById("setup-error");
const liveMsg = document.getElementById("live-msg");

function addQuestionBlock() {
  const block = document.createElement("div");
  block.className = "question-block";
  block.innerHTML = `
    <div class="row">
      <input type="text" class="q-text" placeholder="Question text">
      <button type="button" class="secondary remove-question">Remove</button>
    </div>
    <div class="options"></div>
    <button type="button" class="secondary add-option">+ Add option</button>
  `;
  questionsContainer.appendChild(block);

  const optionsDiv = block.querySelector(".options");
  const addOption = () => {
    if (optionsDiv.children.length >= 4) return;
    const row = document.createElement("div");
    row.className = "option-row";
    row.innerHTML = `
      <input type="radio" name="correct-${Date.now()}-${Math.random()}" class="opt-correct">
      <input type="text" class="opt-text" placeholder="Option text">
      <button type="button" class="secondary remove-option">×</button>
    `;
    optionsDiv.appendChild(row);
    row.querySelector(".remove-option").addEventListener("click", () => row.remove());
  };
  addOption();
  addOption();

  block.querySelector(".add-option").addEventListener("click", addOption);
  block.querySelector(".remove-question").addEventListener("click", () => block.remove());
}

document.getElementById("add-question-btn").addEventListener("click", addQuestionBlock);
addQuestionBlock();

function collectTemplatePayload() {
  const title = document.getElementById("title-input").value.trim();
  const questions = [];

  document.querySelectorAll(".question-block").forEach(block => {
    const text = block.querySelector(".q-text").value.trim();
    const optionRows = [...block.querySelectorAll(".option-row")];
    const options = optionRows.map(r => r.querySelector(".opt-text").value.trim());
    let correctOptionIndex = null;
    optionRows.forEach((r, i) => {
      if (r.querySelector(".opt-correct").checked) correctOptionIndex = i;
    });
    questions.push({ text, options, correctOptionIndex });
  });

  return { title, questions };
}

function showError(el, message) {
  el.textContent = message;
  el.classList.remove("hidden");
}

function clearError(el) {
  el.classList.add("hidden");
  el.textContent = "";
}

document.getElementById("create-poll-btn").addEventListener("click", async () => {
  clearError(setupError);
  const payload = collectTemplatePayload();

  if (!payload.title) {
    showError(setupError, "Title is required.");
    return;
  }
  if (payload.questions.length === 0) {
    showError(setupError, "Add at least one question.");
    return;
  }
  for (const q of payload.questions) {
    if (!q.text) { showError(setupError, "Every question needs text."); return; }
    if (q.options.length < 2 || q.options.length > 4) { showError(setupError, "Every question must have between 2 and 4 options."); return; }
    if (q.options.some(o => !o)) { showError(setupError, "Options cannot be empty."); return; }
  }

  try {
    const pollRes = await fetch("/Operator/CreatePoll", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    });
    if (!pollRes.ok) {
      const body = await pollRes.json().catch(() => ({}));
      throw new Error(body.error || `Poll creation failed (${pollRes.status})`);
    }
    const poll = await pollRes.json();

    document.getElementById("join-code-input").value = poll.pollCode;
    questionsBuilt = payload.questions;
    await connectAndJoin(poll.pollCode);
  } catch (err) {
    showError(setupError, err.message);
  }
});

document.getElementById("join-code-btn").addEventListener("click", async () => {
  clearError(setupError);
  const code = document.getElementById("join-code-input").value.trim();
  if (!code) { showError(setupError, "Enter a poll code."); return; }
  try {
    await connectAndJoin(code);
  } catch (err) {
    showError(setupError, err.message);
  }
});

async function connectAndJoin(code) {
  pollCode = code.toUpperCase();

  if (!connection) {
    connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL)
      .withAutomaticReconnect()
      .build();

    connection.on("QuestionChanged", payload => renderQuestion(payload));
    connection.on("AnswerTally", payload => renderTally(payload));
    connection.on("PollClosed", payload => renderClosed(payload));

    connection.onreconnected(async () => {
      await connection.invoke("JoinPoll", pollCode);
      const snapshot = await connection.invoke("GetPollSnapshot", pollCode);
      applySnapshot(snapshot);
    });

    await connection.start();
  }

  const joinResult = await connection.invoke("JoinPoll", pollCode);
  if (joinResult.reason) {
    throw new Error(`Join rejected: ${joinResult.reason}`);
  }

  applySnapshot(joinResult);
  setupCard.classList.add("hidden");
  liveCard.classList.remove("hidden");
  resultsCard.classList.remove("hidden");
  document.getElementById("poll-code-display").textContent = pollCode;
}

function applySnapshot(snapshot) {
  currentQuestionIndex = snapshot.questionIndex;
  questionCount = snapshot.questionCount;
  currentOptions = snapshot.options;
  renderQuestionPanel(snapshot.question, snapshot.options);
  renderTallyBars(snapshot.currentTally || []);
  recordResultsSnapshot(currentQuestionIndex, snapshot.question, snapshot.options, snapshot.currentTally || []);
}

function renderQuestion(payload) {
  currentQuestionIndex = payload.questionIndex;
  questionCount = payload.questionCount;
  currentOptions = payload.options;
  renderQuestionPanel(payload.question, payload.options);
  renderTallyBars(new Array(payload.options.length).fill(0));
  recordResultsSnapshot(currentQuestionIndex, payload.question, payload.options, new Array(payload.options.length).fill(0));
  liveMsg.textContent = "";
}

function renderQuestionPanel(text, options) {
  document.getElementById("question-text").textContent = `Q${currentQuestionIndex + 1}/${questionCount}: ${text}`;
}

function renderTallyBars(counts) {
  const list = document.getElementById("options-list");
  list.innerHTML = "";
  const total = counts.reduce((a, b) => a + b, 0) || 1;
  currentOptions.forEach((opt, i) => {
    const count = counts[i] || 0;
    const pct = Math.round((count / total) * 100);
    const row = document.createElement("div");
    row.className = "tally-row";
    row.innerHTML = `
      <div style="min-width: 140px;">${opt}</div>
      <div class="tally-bar-track"><div class="tally-bar-fill" style="width:${pct}%"></div></div>
      <div class="tally-count">${count}</div>
    `;
    list.appendChild(row);
  });
}

function renderTally(payload) {
  if (payload.questionIndex !== currentQuestionIndex) return;
  renderTallyBars(payload.counts);
  recordResultsSnapshot(currentQuestionIndex, document.getElementById("question-text").textContent, currentOptions, payload.counts);
}

function recordResultsSnapshot(index, question, options, counts) {
  resultsByIndex[index] = { question, options, counts };
  renderResults();
}

function renderResults() {
  const container = document.getElementById("results-container");
  container.innerHTML = "";
  Object.keys(resultsByIndex).sort((a, b) => a - b).forEach(idx => {
    const r = resultsByIndex[idx];
    const total = r.counts.reduce((a, b) => a + b, 0) || 1;
    const block = document.createElement("div");
    block.className = "results-block";
    let rows = r.options.map((opt, i) => {
      const count = r.counts[i] || 0;
      const pct = Math.round((count / total) * 100);
      return `<div class="tally-row"><div style="min-width:140px;">${opt}</div>
        <div class="tally-bar-track"><div class="tally-bar-fill" style="width:${pct}%"></div></div>
        <div class="tally-count">${count}</div></div>`;
    }).join("");
    block.innerHTML = `<h3>Q${Number(idx) + 1}: ${r.question}</h3>${rows}`;
    container.appendChild(block);
  });
}

document.getElementById("next-question-btn").addEventListener("click", async () => {
  try {
    await connection.invoke("NextQuestion", pollCode);
  } catch (err) {
    liveMsg.textContent = err.message;
  }
});

document.getElementById("close-poll-btn").addEventListener("click", async () => {
  try {
    await connection.invoke("ClosePoll", pollCode);
  } catch (err) {
    liveMsg.textContent = err.message;
  }
});

function renderClosed(payload) {
  document.getElementById("status-pill").textContent = "Closed";
  document.getElementById("status-pill").className = "pill closed";
  document.getElementById("next-question-btn").disabled = true;
  document.getElementById("close-poll-btn").disabled = true;
  liveMsg.textContent = "Poll closed.";

  payload.finalTally.forEach((counts, idx) => {
    const existing = resultsByIndex[idx] || { question: `Question ${idx + 1}`, options: currentOptions, counts };
    existing.counts = counts;
    resultsByIndex[idx] = existing;
  });
  renderResults();
}
