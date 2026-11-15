/**
 * FR #632: the catalogue site and MCP display name are always "Priority MCP".
 */
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import test from 'node:test';

const root = join(dirname(fileURLToPath(import.meta.url)), '..');
const files = [
  'app/page.tsx',
  'app/layout.tsx',
  'lib/mcp.ts',
  'stdio.mjs',
  'catalog/README.md',
];
const OLD = [
  'Priority skill catalog',
  'Priority skills',
  'Catalog MCP',
  'MCP catalog',
  'MCP catalogue',
  "name: 'mcp-priority'",
  'name: "mcp-priority"',
];

test('FR #632 served chrome and MCP display name say Priority MCP', () => {
  for (const rel of files) {
    const text = readFileSync(join(root, rel), 'utf8');
    assert.match(text, /Priority MCP/, `${rel} must say Priority MCP`);
    for (const old of OLD) {
      assert.equal(text.includes(old), false, `${rel} still says ${old}`);
    }
  }
});
