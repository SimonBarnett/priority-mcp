/**
 * FR #696: when EXEC TEXT is empty, proc text falls back to EPROG->PROG->PROGTEXT.
 * Returns structured steps (POS, ENAME, TEXT); does not require Form Prep of PROG.
 */
import test from 'node:test';
import assert from 'node:assert/strict';
import {
  readProcTextViaEprogSteps,
  readProcText,
} from '../lib/proc-text.mjs';

function makeEprogStack({ ename = 'MYPROC', steps }) {
  const progTextForms = [];
  const progForm = {
    name: 'PROG',
    setActiveRow: async () => {},
    getRows: async () => ({
      PROG: Object.fromEntries(
        steps.map((s, i) => [
          String(i + 1),
          { POS: s.POS, ENAME: s.ENAME, _idx: i + 1 },
        ]),
      ),
    }),
    startSubForm: async (child) => {
      assert.equal(child, 'PROGTEXT');
      const step = steps[progTextForms.length];
      const childForm = {
        name: 'PROGTEXT',
        getRows: async () => ({
          PROGTEXT: Object.fromEntries(
            (step.textRows || [step.TEXT]).map((t, i) => [
              String(i + 1),
              { TEXT: t, _idx: i + 1 },
            ]),
          ),
        }),
        endCurrentForm: async () => {},
      };
      progTextForms.push(childForm);
      return childForm;
    },
    endCurrentForm: async () => {},
  };
  const eprog = {
    name: 'EPROG',
    setSearchFilter: async () => {},
    setActiveRow: async () => {},
    getRows: async () => ({
      EPROG: { 1: { ENAME: ename, _idx: 1 } },
    }),
    startSubForm: async (child) => {
      assert.equal(child, 'PROG');
      return progForm;
    },
    endCurrentForm: async () => {},
  };
  return { eprog, progForm, progTextForms };
}

test('FR #696: EPROG steps concatenate TEXT and return structured steps', async () => {
  const steps = [
    { POS: 1, ENAME: 'SQLI', TEXT: 'SELECT 1' },
    { POS: 2, ENAME: 'INPUT', TEXT: '-- input' },
    { POS: 3, ENAME: 'SQLI', TEXT: 'SELECT 2', textRows: ['SELECT 2', ' FROM DUMMY'] },
  ];
  const { eprog } = makeEprogStack({ steps });
  const ended = [];
  const r = await readProcTextViaEprogSteps(
    { ok: true, priority: {} },
    { company: 'CO' },
    'MYPROC',
    {
      formStart: async (p, name) => {
        assert.equal(name, 'EPROG');
        return eprog;
      },
      formGetRows: async (f) => {
        const pack = (await f.getRows(1))[f.name] || {};
        const list = Object.keys(pack).map((i) => ({ _idx: Number(i), ...pack[i] }));
        return { formName: f.name, count: list.length, rows: list };
      },
      formStartSubForm: async (parent, child) => parent.startSubForm(child),
      safeEnd: async (f) => {
        ended.push(f.name);
      },
    },
  );
  assert.equal(r.ok, true);
  assert.equal(r.name, 'MYPROC');
  assert.equal(r.form, 'EPROG');
  assert.equal(r.source, 'eprog-steps');
  assert.equal(r.steps.length, 3);
  assert.deepEqual(
    r.steps.map((s) => ({ POS: s.POS, ENAME: s.ENAME })),
    [
      { POS: 1, ENAME: 'SQLI' },
      { POS: 2, ENAME: 'INPUT' },
      { POS: 3, ENAME: 'SQLI' },
    ],
  );
  assert.equal(r.steps[2].TEXT, 'SELECT 2 FROM DUMMY');
  assert.match(r.body, /SELECT 1/);
  assert.match(r.body, /SELECT 2 FROM DUMMY/);
  assert.ok(ended.includes('PROGTEXT') || ended.includes('PROG') || ended.includes('EPROG'));
});

test('FR #696: empty EXEC TEXT falls back to EPROG steps', async () => {
  const steps = [{ POS: 10, ENAME: 'SQLI', TEXT: '/* body from steps */' }];
  const { eprog } = makeEprogStack({ ename: 'EMPTYPROC', steps });
  const execForm = {
    name: 'EXEC',
    setSearchFilter: async () => {},
    getRows: async () => ({
      EXEC: { 1: { ENAME: 'EMPTYPROC', TEXT: '', _idx: 1 } },
    }),
    endCurrentForm: async () => {},
  };
  const r = await readProcText(
    { ok: true, priority: {} },
    { company: 'CO' },
    'EMPTYPROC',
    {
      formStart: async (p, name) => (name === 'EXEC' ? execForm : eprog),
      formGetRows: async (f) => {
        const pack = (await f.getRows(1))[f.name] || {};
        const list = Object.keys(pack).map((i) => ({ _idx: Number(i), ...pack[i] }));
        return { formName: f.name, count: list.length, rows: list };
      },
      formStartSubForm: async (parent, child) => parent.startSubForm(child),
      safeEnd: async () => {},
    },
  );
  assert.equal(r.ok, true);
  assert.equal(r.source, 'eprog-steps');
  assert.equal(r.steps[0].TEXT, '/* body from steps */');
  assert.equal(r.body, '/* body from steps */');
});

test('FR #696: EXEC body still preferred when present', async () => {
  const execForm = {
    name: 'EXEC',
    setSearchFilter: async () => {},
    getRows: async () => ({
      EXEC: { 1: { ENAME: 'HASBODY', TEXT: 'SELECT header', _idx: 1 } },
    }),
    endCurrentForm: async () => {},
  };
  let eprogStarts = 0;
  const r = await readProcText(
    { ok: true, priority: {} },
    { company: 'CO' },
    'HASBODY',
    {
      formStart: async (p, name) => {
        if (name === 'EPROG') eprogStarts += 1;
        return execForm;
      },
      formGetRows: async (f) => {
        const pack = (await f.getRows(1))[f.name] || {};
        const list = Object.keys(pack).map((i) => ({ _idx: Number(i), ...pack[i] }));
        return { formName: f.name, count: list.length, rows: list };
      },
      safeEnd: async () => {},
    },
  );
  assert.equal(r.ok, true);
  assert.equal(r.body, 'SELECT header');
  assert.equal(r.form, 'EXEC');
  assert.equal(eprogStarts, 0);
  assert.equal(r.steps, undefined);
});

test('FR #696: both EXEC empty and EPROG missing -> proc_text_missing', async () => {
  const execForm = {
    name: 'EXEC',
    setSearchFilter: async () => {},
    getRows: async () => ({
      EXEC: { 1: { ENAME: 'GONE', TEXT: '', _idx: 1 } },
    }),
    endCurrentForm: async () => {},
  };
  const eprog = {
    name: 'EPROG',
    setSearchFilter: async () => {},
    getRows: async () => ({ EPROG: {} }),
    endCurrentForm: async () => {},
  };
  const r = await readProcText(
    { ok: true, priority: {} },
    { company: 'CO' },
    'GONE',
    {
      formStart: async (p, name) => (name === 'EXEC' ? execForm : eprog),
      formGetRows: async (f) => {
        const pack = (await f.getRows(1))[f.name] || {};
        const list = Object.keys(pack).map((i) => ({ _idx: Number(i), ...pack[i] }));
        return { formName: f.name, count: list.length, rows: list };
      },
      safeEnd: async () => {},
    },
  );
  assert.equal(r.ok, false);
  assert.equal(r.reason, 'proc_text_missing');
});
