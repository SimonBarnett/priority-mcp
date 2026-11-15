import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import test from 'node:test';

import { pickPriorityAgentDownload } from './priority-agent-download.mjs';

const __dirname = dirname(fileURLToPath(import.meta.url));
const pagePath = join(__dirname, '..', 'app', 'page.tsx');

test('FR #607 pick prefers newest release that has PriorityAgent.exe', () => {
  const picked = pickPriorityAgentDownload([
    { tag_name: 'uat-2026-11-06', assets: [] },
    {
      tag_name: 'launcher-v1.0.74',
      assets: [
        {
          name: 'PriorityAgent.exe',
          browser_download_url:
            'https://github.com/SimonBarnett/priority-mcp/releases/download/launcher-v1.0.74/PriorityAgent.exe',
        },
        {
          name: 'PriorityAgent.exe.sha256',
          browser_download_url:
            'https://github.com/SimonBarnett/priority-mcp/releases/download/launcher-v1.0.74/PriorityAgent.exe.sha256',
        },
      ],
    },
    {
      tag_name: 'launcher-v1.0.1',
      assets: [
        {
          name: 'PriorityAgent.exe',
          browser_download_url:
            'https://github.com/SimonBarnett/priority-mcp/releases/download/launcher-v1.0.1/PriorityAgent.exe',
        },
      ],
    },
  ]);
  assert.ok(picked);
  assert.equal(picked.tag, 'launcher-v1.0.74');
  assert.equal(picked.versionLabel, '1.0.74');
  assert.match(picked.exeUrl, /PriorityAgent\.exe$/);
  assert.match(picked.sha256Url, /PriorityAgent\.exe\.sha256$/);
});

test('FR #607 pick skips releases without the exe asset', () => {
  assert.equal(
    pickPriorityAgentDownload([{ tag_name: 'uat-x', assets: [{ name: 'notes.txt', browser_download_url: 'https://x' }] }]),
    null,
  );
});

test('FR #607 home page UI says Priority Agent and never launcher', () => {
  const src = readFileSync(pagePath, 'utf8');
  assert.match(src, /Priority Agent/);
  assert.match(src, /resolvePriorityAgentDownload|priority-agent-download/);
  assert.doesNotMatch(src, /\blauncher\b/i);
});
