/**
 * FR #606: mcp.ntsa.uk home uses FR-A twin tokens + bob.ntsa.uk / GitHub links.
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

test('fr606 globals use shared charcoal/paper/gold token language', () => {
  for (const token of ['--ink', '--gold', '--charcoal', '--paper']) {
    assert.ok(css.includes(token), `missing ${token}`);
  }
  assert.match(css, /FR #606|FR-A|butler gold|charcoal/i);
});

test('fr606 page uses shell/header/nav chrome classes', () => {
  assert.match(page, /className="shell"/);
  assert.match(page, /className="site-header"/);
  assert.match(page, /className="nav"/);
  assert.match(page, /className="site-footer"/);
});
