// -----------------------------------------------------------------------------
// File:        extension.ts
// Author:      Piergiorgio Vagnozzi
// Description: VS Code extension entry point for the Chishiki Agent.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------

import * as vscode from "vscode";
import * as cp from "child_process";
import * as path from "path";
import * as http from "http";

let agentProcess: cp.ChildProcess | undefined;
let chatPanel: vscode.WebviewPanel | undefined;

// ── Extension Activation ──────────────────────────────────────────────────────

export function activate(context: vscode.ExtensionContext): void {
	const config = vscode.workspace.getConfiguration("chishiki");
	const agentUrl: string = config.get("agentUrl", "http://localhost:5100");
	const autoStart: boolean = config.get("autoStart", true);

	if (autoStart) {
		void startAgentIfNotRunning(agentUrl);
	}

	// ── Commands ──────────────────────────────────────────────────────────────
	context.subscriptions.push(
		vscode.commands.registerCommand("chishiki.openChat", () =>
			openChatPanel(context, agentUrl),
		),

		vscode.commands.registerCommand("chishiki.analyzeFile", () =>
			analyzeCurrentFile(agentUrl),
		),

		vscode.commands.registerCommand("chishiki.startAgent", () =>
			startAgentIfNotRunning(agentUrl),
		),
	);

	// ── Inline completions provider ───────────────────────────────────────────
	const inlineProvider = vscode.languages.registerInlineCompletionItemProvider(
		{ pattern: "**" },
		{
			async provideInlineCompletionItems(document, position, _ctx, token) {
				const line = document.lineAt(position.line).text;
				if (line.trim().length < 3) {
					return;
				}

				const completion = await fetchCompletion(agentUrl, {
					model: config.get("defaultModel", "local"),
					messages: [
						{
							role: "system",
							content:
								"You are a code completion assistant. Complete the code snippet. Output only the completion, no explanation.",
						},
						{ role: "user", content: `Complete this code:\n${line}` },
					],
				});

				if (!completion || token.isCancellationRequested) {
					return;
				}

				return {
					items: [{ insertText: completion }],
				};
			},
		},
	);
	context.subscriptions.push(inlineProvider);
}

export function deactivate(): void {
	agentProcess?.kill();
	chatPanel?.dispose();
}

// ── Agent Process Management ──────────────────────────────────────────────────

async function startAgentIfNotRunning(agentUrl: string): Promise<void> {
	if (await isAgentRunning(agentUrl)) {
		void vscode.window.showInformationMessage(
			"Chishiki Agent is already running.",
		);
		return;
	}

	const agentBin = vscode.workspace
		.getConfiguration("chishiki")
		.get<string>("agentBinary", "chishiki-agent");

	agentProcess = cp.spawn(agentBin, ["--headless"], {
		detached: false,
		stdio: "ignore",
	});

	agentProcess.on("error", (err) => {
		void vscode.window.showWarningMessage(
			`Could not start Chishiki Agent (${err.message}). Start it manually.`,
		);
	});

	void vscode.window.showInformationMessage(
		`Chishiki Agent starting on ${agentUrl}…`,
	);
}

function isAgentRunning(agentUrl: string): Promise<boolean> {
	return new Promise((resolve) => {
		const req = http.get(`${agentUrl}/health`, (res) => {
			resolve(res.statusCode === 200);
		});
		req.on("error", () => resolve(false));
		req.setTimeout(1500, () => {
			req.destroy();
			resolve(false);
		});
	});
}

// ── Chat Panel ────────────────────────────────────────────────────────────────

function openChatPanel(
	context: vscode.ExtensionContext,
	agentUrl: string,
): void {
	if (chatPanel) {
		chatPanel.reveal(vscode.ViewColumn.Beside);
		return;
	}

	chatPanel = vscode.window.createWebviewPanel(
		"chishikiChat",
		"Chishiki Chat",
		vscode.ViewColumn.Beside,
		{
			enableScripts: true,
			retainContextWhenHidden: true,
		},
	);

	chatPanel.webview.html = getChatWebviewHtml(agentUrl);

	chatPanel.onDidDispose(
		() => {
			chatPanel = undefined;
		},
		null,
		context.subscriptions,
	);
}

function getChatWebviewHtml(agentUrl: string): string {
	return `<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <title>Chishiki Chat</title>
  <style>
    body { margin: 0; font-family: var(--vscode-font-family); background: var(--vscode-editor-background); color: var(--vscode-editor-foreground); display: flex; flex-direction: column; height: 100vh; }
    #messages { flex: 1; overflow-y: auto; padding: 12px; }
    .msg { margin-bottom: 10px; }
    .msg.user { color: var(--vscode-textLink-foreground); }
    .msg.assistant { color: var(--vscode-editor-foreground); }
    .msg strong { font-weight: 600; }
    #inputRow { display: flex; gap: 8px; padding: 8px; border-top: 1px solid var(--vscode-panel-border); }
    #input { flex: 1; background: var(--vscode-input-background); color: var(--vscode-input-foreground); border: 1px solid var(--vscode-input-border, #555); padding: 6px 10px; border-radius: 4px; font-size: 13px; }
    button { background: var(--vscode-button-background); color: var(--vscode-button-foreground); border: none; padding: 6px 14px; border-radius: 4px; cursor: pointer; }
  </style>
</head>
<body>
  <div id="messages"></div>
  <div id="inputRow">
    <input id="input" placeholder="Ask Chishiki Agent…" autocomplete="off" />
    <button id="send">Send</button>
  </div>
  <script>
    const agentUrl = ${JSON.stringify(agentUrl)};
    const messages = document.getElementById('messages');
    const input = document.getElementById('input');
    const sendBtn = document.getElementById('send');
    const history = [];

    async function sendMessage() {
      const text = input.value.trim();
      if (!text) return;
      input.value = '';
      history.push({ role: 'user', content: text });
      appendMessage('user', text);

      const resp = await fetch(agentUrl + '/v1/chat/completions', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ model: 'local', messages: history, stream: false })
      }).catch(() => null);

      if (!resp || !resp.ok) { appendMessage('assistant', '[Error contacting agent]'); return; }
      const data = await resp.json();
      const content = data?.message?.content ?? data?.choices?.[0]?.message?.content ?? '';
      history.push({ role: 'assistant', content });
      appendMessage('assistant', content);
    }

    function appendMessage(role, text) {
      const div = document.createElement('div');
      div.className = 'msg ' + role;
      div.innerHTML = '<strong>' + (role === 'user' ? 'You' : 'Agent') + ':</strong> ' + escapeHtml(text);
      messages.appendChild(div);
      messages.scrollTop = messages.scrollHeight;
    }

    function escapeHtml(str) {
      return str.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;');
    }

    sendBtn.addEventListener('click', sendMessage);
    input.addEventListener('keydown', e => { if (e.key === 'Enter') sendMessage(); });
  </script>
</body>
</html>`;
}

// ── File Analysis ─────────────────────────────────────────────────────────────

async function analyzeCurrentFile(agentUrl: string): Promise<void> {
	const editor = vscode.window.activeTextEditor;
	if (!editor) {
		void vscode.window.showWarningMessage("No active editor.");
		return;
	}

	const source = editor.document.getText();
	const pluginId = "code-analysis";
	const capabilityName = "analyze-complexity";

	await vscode.window.withProgress(
		{
			location: vscode.ProgressLocation.Notification,
			title: "Chishiki: Analysing…",
			cancellable: false,
		},
		async () => {
			try {
				const res = await fetch(
					`${agentUrl}/v1/plugins/${pluginId}/capabilities/${capabilityName}`,
					{
						method: "POST",
						headers: { "Content-Type": "application/json" },
						body: JSON.stringify({ capabilityName, parameters: { source } }),
					},
				);

				if (!res.ok) {
					throw new Error(await res.text());
				}
				const data = (await res.json()) as { data?: { report?: string } };
				const report = data?.data?.report ?? "No report returned.";

				const doc = await vscode.workspace.openTextDocument({
					content: report,
					language: "plaintext",
				});
				await vscode.window.showTextDocument(doc, vscode.ViewColumn.Beside);
			} catch (err) {
				void vscode.window.showErrorMessage(`Analysis failed: ${String(err)}`);
			}
		},
	);
}

// ── HTTP Helpers ──────────────────────────────────────────────────────────────

interface ChatMessage {
	role: string;
	content: string;
}
interface CompletionRequest {
	model: string;
	messages: ChatMessage[];
	stream: boolean;
}

async function fetchCompletion(
	agentUrl: string,
	request: CompletionRequest,
): Promise<string | undefined> {
	try {
		const res = await fetch(`${agentUrl}/v1/chat/completions`, {
			method: "POST",
			headers: { "Content-Type": "application/json" },
			body: JSON.stringify(request),
		});
		if (!res.ok) {
			return undefined;
		}
		const data = (await res.json()) as { message?: { content?: string } };
		return data?.message?.content;
	} catch {
		return undefined;
	}
}
