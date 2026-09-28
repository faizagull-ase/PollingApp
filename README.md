# PulsePoll: Requirements & Feature Reference

**Practice project — as-built reference**

Status: matches current code · Auth: none (poll-code only) · Backend: ASP.NET Core + SignalR + EF Core (SQL Server) · Frontend: two static HTML pages

This document describes what PulsePoll actually does today, based on the code in `PulsePoll_Api/PulsePoll/` and `PulsePoll_frontend/`. It intentionally leaves out production/deployment concerns (auth, rate limiting, scaling, observability, backplanes) since this is a practice project, not a system being shipped to real users.

---

## Table of contents

1. [Overview](#1-overview)
2. [Backend endpoints](#2-backend-endpoints)
3. [SignalR hub](#3-signalr-hub)
4. [Data model & persistence](#4-data-model--persistence)
5. [Frontend: operator page](#5-frontend-operator-page)
6. [Frontend: participant page](#6-frontend-participant-page)
7. [Known limitations](#7-known-limitations)

---

## 1. Overview

PulsePoll is a live-polling app: an operator builds a set of questions, starts a poll, and shares a short code. Participants join with that code and answer questions in real time on their own device. The operator advances questions and closes the poll; everyone sees live vote tallies as they come in.

There is no login and no participant identity — the poll code is the only thing that gates access, and votes are deduplicated per SignalR connection, not per person.

---

## 2. Backend endpoints

Only two HTTP endpoints exist, both `POST`, both used once each in the flow (everything else happens over SignalR):

| Endpoint | Purpose |
|---|---|
| `POST /api/templates` | Create a template: title + a list of questions, each with up to 4 options and an optional correct-option index. Validated by `TemplateValidator`, saved to SQL Server via EF Core. Returns the new template's id, title, and question count. |
| `POST /api/polls` | Start a poll from an existing `templateId`. Loads the template from SQL, generates a 5-character poll code, builds the in-memory poll state, and returns the poll code + question count. |

There is no list/get/update/delete for templates and no template file-upload flow — only creation exists.

---

## 3. SignalR hub

`PollHub` (`/hubs/poll`) exposes five methods, all usable by anyone who has the poll code:

| Method | What it does |
|---|---|
| `JoinPoll(pollCode)` | Adds the connection to the poll's SignalR group; returns the current question/options/tally, or a rejection if the code is invalid/poll is closed. |
| `SubmitAnswer(pollCode, questionIndex, optionIndex)` | Records a vote if the poll is open and this connection hasn't already answered this question; broadcasts the updated tally to the group. |
| `NextQuestion(pollCode)` | Advances to the next question and broadcasts it to the group. |
| `ClosePoll(pollCode)` | Marks the poll closed and broadcasts the final tally. |
| `GetPollSnapshot(pollCode)` | Returns current poll state — used by clients to resync (e.g. after a page refresh). |

There are no `OnConnectedAsync`/`OnDisconnectedAsync` overrides — reconnects are handled entirely by the client re-calling `JoinPoll` + `GetPollSnapshot`, not by any server-side session recovery.

---

## 4. Data model & persistence

Two tiers, and only two:

- **SQL Server (via EF Core)** — `Template` (Id, Title, CreatedAt) and `Question` (Id, TemplateId, Order, Text, Options as JSON, optional CorrectOptionIndex). This is the only durable data in the system.
- **In-memory cache (`IMemoryCache`, single process, 24h TTL)** — everything about a *running* poll: current question, open/closed status, live tallies, and the set of connection IDs that have already answered each question. This is created fresh from a template when a poll starts and is not written back to SQL.

There is no persisted answer/response history — once a poll closes or the app restarts, only the final tally that was broadcast at close is what anyone saw; nothing is stored for later reporting.

---

## 5. Frontend: operator page

`operator.html` lets the operator:

1. Build a template in the browser — add/remove questions (up to 4 options each), mark one option correct — then submit it (`POST /api/templates` followed by `POST /api/polls`), which auto-fills the poll code and joins the poll.
2. Alternatively, type in an existing poll code and connect directly.
3. Drive the live poll: **Next Question** / **Close Poll** buttons, with the current question and live tallies displayed as votes come in.
4. See a **Results** panel that accumulates each question's tallies client-side as the poll progresses — this is built from what the browser has seen in this session, not fetched from a backend reporting endpoint.

---

## 6. Frontend: participant page

`participant.html` lets a participant:

1. Enter a poll code and join.
2. See the current question with one button per option; tapping an option submits the answer immediately (buttons disable after tap) and shows accepted/rejected feedback.
3. On reconnect, the page re-joins and re-fetches the snapshot automatically to resume where it left off.
4. See a "poll closed" message when the operator ends the session. There is no results/report view for participants.

---

## 7. Known limitations

These are accepted for a practice project, not gaps to fix unless the project's scope changes:

- **No operator authentication** — anyone with the poll code can advance questions or close the poll.
- **No exactly-once voting per person** — dedup is per SignalR connection, so a reconnecting participant can vote again.
- **No persisted reporting** — tallies live only in memory while the poll runs; nothing is saved for after-the-fact analysis beyond the final tally shown at close.
- **No rate limiting, health checks, or metrics** — none implemented.
- **Single-process only** — poll state is in local memory cache, so it wouldn't survive multiple app instances or a restart.
- **Template management is create-only** — no editing, listing, or deleting templates from the API.

---


