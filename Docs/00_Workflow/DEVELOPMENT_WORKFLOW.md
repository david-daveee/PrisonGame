# Development Workflow

This document defines the mandatory development process for PrisonGame.

It must be read first by every new chat or developer before changing code or design.

## 1. Source of truth

The project must remain understandable without relying on one long chat.

Important information must live in:

- code;
- project documentation;
- Git history.

A fundamental decision must never exist only in conversation history.

## 2. Reading order for a new chat

Before continuing development, read:

1. `DEVELOPMENT_WORKFLOW.md`
2. `PROJECT_BIBLE.md`
3. `GAMEPLAY_RULES.md`
4. `ARCHITECTURE.md`
5. `CODING_STYLE.md`
6. `PROJECT_DECISIONS.md`
7. `DEV_LOG.md`
8. `ROADMAP.md`
9. `NEW_CHAT_HANDOFF.md`

Then inspect only the code relevant to the current task.

## 3. Development style

Before writing a class, answer:

- What does it store?
- What can it do?
- What must it not do?

### Familiar topic

Move quickly:

1. short design check;
2. code;
3. test.

### New topic

Move more slowly:

1. explain the concept;
2. compare options;
3. ask a few focused questions;
4. write code;
5. test;
6. summarize the principle.

### Fundamental topic

Stop before coding and explain:

- why it is foundational;
- how it is usually handled professionally;
- what future bugs it prevents;
- why it matters for multiplayer;
- whether it should be solved now or intentionally postponed.

## 4. Definition of done for a mechanic

A mechanic is complete only when:

1. the code compiles;
2. the feature works in Unity;
3. expected edge cases were tested;
4. no known architectural contradiction remains;
5. required documentation is updated;
6. a focused Git commit is created;
7. the commit is pushed.

The cycle is:

```text
Design
→ Implement
→ Test
→ Session Review
→ Update Docs
→ Commit
→ Push
```

## 5. Session Review

At the end of a meaningful session or completed mechanic, ChatGPT must perform a Session Review.

Use this format:

```text
SESSION REVIEW

Updated:
- DEV_LOG.md — because ...
- ROADMAP.md — because ...
- ARCHITECTURE.md — because ...

Not updated:
- PROJECT_BIBLE.md — because the game vision did not change.
- CODING_STYLE.md — because no new coding rule was introduced.

Recommended commit:
feat: ...
```

ChatGPT must explain why each document is or is not being updated.

## 6. Document update rules

### DEV_LOG.md

Update after every meaningful session.

Record:

- what was completed;
- what changed;
- current problems;
- exact next step.

### ROADMAP.md

Update when:

- a task is completed;
- a milestone changes;
- scope changes;
- a new necessary task appears.

### ARCHITECTURE.md

Update when:

- responsibilities of classes change;
- a new subsystem is introduced;
- data flow changes;
- networking authority changes;
- a reusable interface or pattern is introduced.

### PROJECT_DECISIONS.md

Update whenever an important decision is made.

Record:

- date;
- decision;
- context;
- reason;
- consequences;
- rejected alternatives.

### PROJECT_BIBLE.md

Update only when the high-level game vision changes.

### GAMEPLAY_RULES.md

Update when a reusable design rule is discovered.

### CODING_STYLE.md

Update when a lasting code convention is introduced.

### IDEAS.md

Add ideas immediately, but do not treat them as committed scope.

### CHANGELOG.md

Update only for meaningful playable versions or releases.

### NEW_CHAT_HANDOFF.md

Update when the latest development state or immediate next step changes significantly.

## 7. Git workflow

Make small commits after complete steps.

Good examples:

```text
feat: add inventory input action
feat: add UI mode controller
refactor: migrate inventory to InventoryItem
fix: restore inventory slot references
docs: update inventory architecture
```

Avoid vague messages:

```text
update
changes
stuff
work
```

Use `main` while the project is small and stable.

Create feature branches for risky work:

```text
feature/grid-inventory
feature/containers
feature/networking
```

## 8. ChatGPT responsibilities

ChatGPT acts as Senior/Lead Gameplay Programmer and mentor.

It must:

- protect architecture;
- explain why decisions are made;
- consider future multiplayer;
- avoid temporary hacks that soon require expensive rewrites;
- avoid premature complexity;
- distinguish local UI from authoritative game state;
- notice when documentation must be updated;
- suggest the exact commit message after a completed step.

When the user writes **го**:

- keep explanations short;
- ask only essential questions;
- move to implementation quickly;
- still stop for foundational risks.

## 9. Knowledge transfer rule

The project must be continuable from any new chat.

No important knowledge may depend on memory of previous messages alone.

The repository documentation is the persistent project memory.
