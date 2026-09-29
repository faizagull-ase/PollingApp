const HUB_URL = window.appConfig.hubUrl;

let connection = null;
let pollCode = null;
let currentQuestionIndex = -1;
let answeredQuestions = new Set();

const joinCard = document.getElementById("join-card");
const questionCard = document.getElementById("question-card");
const closedCard = document.getElementById("closed-card");
const joinError = document.getElementById("join-error");
const answerMsg = document.getElementById("answer-msg");

function showMsg(el, text, kind) {
  el.textContent = text;
  el.className = "msg" + (kind ? " " + kind : "");
  el.classList.remove("hidden");
}

document.getElementById("join-btn").addEventListener("click", async () => {
  const code = document.getElementById("code-input").value.trim();
  if (!code) {
    showMsg(joinError, "Enter a poll code.", "error");
    return;
  }
  joinError.classList.add("hidden");
  await joinPoll(code);
});

async function joinPoll(code) {
  pollCode = code.toUpperCase();

  try {
    if (!connection) {
      connection = new signalR.HubConnectionBuilder()
        .withUrl(HUB_URL)
        .withAutomaticReconnect()
        .build();

      connection.on("QuestionChanged", payload => onQuestionChanged(payload));
      connection.on("PollClosed", () => onPollClosed());

      connection.onreconnected(async () => {
        try {
          await connection.invoke("JoinPoll", pollCode);
          const snapshot = await connection.invoke("GetPollSnapshot", pollCode);
          applySnapshot(snapshot);
        } catch {
          onPollClosed();
        }
      });

      await connection.start();
    }

    const result = await connection.invoke("JoinPoll", pollCode);
    if (result.reason) {
      if (result.reason === "poll_closed") {
        onPollClosed();
      } else {
        showMsg(joinError, `Could not join: ${result.reason}`, "error");
      }
      return;
    }

    applySnapshot(result);
    joinCard.classList.add("hidden");
    questionCard.classList.remove("hidden");
  } catch (err) {
    showMsg(joinError, err.message, "error");
  }
}

function applySnapshot(snapshot) {
  answeredQuestions.clear();
  renderQuestion(snapshot.question, snapshot.options, snapshot.questionIndex, snapshot.questionCount);
}

function onQuestionChanged(payload) {
  renderQuestion(payload.question, payload.options, payload.questionIndex, payload.questionCount);
}

function renderQuestion(text, options, questionIndex, questionCount) {
  currentQuestionIndex = questionIndex;
  document.getElementById("question-count").textContent = `Question ${questionIndex + 1} of ${questionCount}`;
  document.getElementById("question-text").textContent = text;
  answerMsg.classList.add("hidden");

  const container = document.getElementById("options-container");
  container.innerHTML = "";
  const alreadyAnswered = answeredQuestions.has(questionIndex);

  options.forEach((opt, i) => {
    const btn = document.createElement("button");
    btn.type = "button";
    btn.className = "option-btn";
    btn.textContent = opt;
    btn.disabled = alreadyAnswered;
    btn.addEventListener("click", () => submitAnswer(questionIndex, i, btn, container));
    container.appendChild(btn);
  });

  questionCard.classList.remove("hidden");
  closedCard.classList.add("hidden");
}

async function submitAnswer(questionIndex, optionIndex, btn, container) {
  [...container.children].forEach(b => b.disabled = true);
  btn.classList.add("selected");

  try {
    const result = await connection.invoke("SubmitAnswer", pollCode, questionIndex, optionIndex);
    if (result.reason) {
      btn.classList.remove("selected");
      btn.classList.add("rejected");
      showMsg(answerMsg, `Not counted: ${result.reason}`, "error");
      if (result.reason !== "duplicate" && result.reason !== "stale_question") {
        [...container.children].forEach(b => b.disabled = false);
        btn.classList.remove("rejected");
      }
    } else {
      answeredQuestions.add(questionIndex);
      showMsg(answerMsg, "Answer submitted.", "success");
    }
  } catch (err) {
    showMsg(answerMsg, err.message, "error");
  }
}

function onPollClosed() {
  questionCard.classList.add("hidden");
  joinCard.classList.add("hidden");
  closedCard.classList.remove("hidden");
}
