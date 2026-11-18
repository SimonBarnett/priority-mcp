/**
 * FR #697: form subform applies parent filter (--request / --filter) before startSubForm.
 */
import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { spawnSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';
import { positionParentForSubform } from '../lib/form.mjs';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const ROOT = path.resolve(__dirname, '..');
const BIN = path.join(ROOT, 'bin', 'priority-wcf.mjs');

test('FR #697: positionParentForSubform sets search then activates first row', async () => {
  const filters = [];
  const active = [];
  const form = {
    name: 'EPROG',
    setSearchFilter: async (f) => {
      filters.push(f);
    },
    setActiveRow: async (idx) => {
      active.push(idx);
    },
    getRows: async () => ({
      EPROG: {
        1: { ENAME: 'OTHER', _idx: 1 },
        2: { ENAME: 'ZOBW_DOWNLOAD_PORT_A', _idx: 2 },
      },
    }),
  };
  const filter = {
    or: 0,
    QueryValues: [
      { field: 'ENAME', fromval: 'ZOBW_DOWNLOAD_PORT_A', toval: '', op: '=', sort: 0, isdesc: 0 },
    ],
  };
  const r = await positionParentForSubform(form, filter, {
    getRows: async (f) => {
      const pack = (await f.getRows(1)).EPROG;
      // Simulate SDK returning only the filtered row after setSearchFilter.
      assert.equal(filters.length, 1);
      return {
        formName: 'EPROG',
        count: 1,
        rows: [{ _idx: 2, ENAME: 'ZOBW_DOWNLOAD_PORT_A' }],
      };
    },
  });
  assert.equal(r.searchCalls, 1);
  assert.equal(r.parentRows, 1);
  assert.equal(r.activeIdx, 2);
  assert.deepEqual(active, [2]);
});

test('FR #697: no filter still opens parent without search', async () => {
  const filters = [];
  const form = {
    setSearchFilter: async (f) => filters.push(f),
    setActiveRow: async () => {},
    getRows: async () => ({ EPROG: { 1: { ENAME: 'A', _idx: 1 } } }),
  };
  const r = await positionParentForSubform(form, null, {
    getRows: async () => ({
      formName: 'EPROG',
      count: 1,
      rows: [{ _idx: 1, ENAME: 'A' }],
    }),
  });
  assert.equal(r.searchCalls, 0);
  assert.equal(filters.length, 0);
  assert.equal(r.activeIdx, 1);
});

test('FR #697: README documents parent filter for form subform', () => {
  const readme = fs.readFileSync(path.join(ROOT, 'README.md'), 'utf8');
  assert.match(readme, /form subform[\s\S]{0,400}filter/i);
  assert.match(readme, /parent/i);
});

test('FR #697: help mentions filter for form subform', () => {
  const bin = fs.readFileSync(BIN, 'utf8');
  assert.match(bin, /form subform[\s\S]{0,300}filter/i);
  assert.match(bin, /positionParentForSubform|FR #697/);
});
