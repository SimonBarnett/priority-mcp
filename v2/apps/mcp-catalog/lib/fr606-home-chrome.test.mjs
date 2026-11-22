/**
 * FR #606 links stay. FR #731 replaces the card chrome with plan.ntsa.uk document formatting.
 */
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import test from 'node:test';

const root = join(dirname(fileURLToPath(import.meta.url)), '..');
const page = readFileSync(join(root, 'app', 'page.tsx'), 'utf8');
const css = readFileSync(join(root, 'app', 'globals.css'), 'utf8');

test('fr606 home links bob.ntsa.uk and priority-mcp GitHub', () => {
  assert.match(page, /https:\/\/bob\.ntsa\.uk\/?/);
  assert.match(page, /https:\/\/github\.com\/SimonBarnett\/priority-mcp/);
  assert.match(page, /bob\.ntsa\.uk/);
  assert.match(page, /GitHub/);
});

test('fr731 home uses plan.ntsa.uk document formatting', () => {
  assert.match(css, /max-width:\s*44rem/);
  assert.match(css, /FR #731/);
  assert.match(page, /className="meta"/);
  assert.match(page, /className="badge"/);
  assert.match(page, /mcp-home/);
  assert.match(page, /className="skill-list"/);
  assert.match(page, /<h2>MCP<\/h2>/);
  assert.match(page, /<h2>Also<\/h2>/);
  assert.match(page, /https:\/\/plan\.ntsa\.uk/);
  assert.doesNotMatch(page, /className="shell"/);
  assert.doesNotMatch(page, /className="site-header"/);
});
