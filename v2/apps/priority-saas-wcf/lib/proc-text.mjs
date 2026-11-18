/**
 * FR #409 / FR-027: read procedure body from the EXEC form (read-only).
 * FR #410 / FR-028: write procedure body through the EXEC form (not raw SQL).
 * FR #696: when EXEC TEXT is empty, fall back to EPROG -> PROG -> PROGTEXT
 * (read-only; does not require Form Prep of PROG).
 * Keep TEXT field naming shared. Never log tokens or echo connection secrets.
 * Never return the file body on write failure.
 */
import fs from 'node:fs';
import {
  formStart,
  formGetRows,
  formSave,
  formStartSubForm,
  safeEnd,
} from './form.mjs';

function pick(obj, keys) {
  if (!obj || typeof obj !== 'object') return '';
  for (const k of keys) {
    const v = obj[k];
    if (v != null && String(v).length) return String(v);
  }
  return '';
}

/** Return trimmed body text, or null when missing/blank. */
export function bodyFromExecRow(row) {
  const raw = pick(row, ['TEXT', 'Text', 'text', 'TRIGTEXT', 'BODY', 'body']);
  const body = String(raw ?? '').replace(/^\uFEFF/, '');
  if (!body.trim()) return null;
  return body;
}

/**
 * Read-only: open EXEC, search ENAME, return {name, body}.
 * Missing row or empty TEXT -> reason proc_text_missing.
 */
export async function readProcTextViaExecForm(session, instance, name, {
  messages = [],
  formStart: startFn,
  formGetRows: getRowsFn,
  safeEnd: endFn,
} = {}) {
  const ename = String(name || '').trim();
  if (!ename) return { ok: false, reason: 'name_required' };
  if (!session?.ok || !session.priority) {
    return { ok: false, reason: session?.reason || 'session_required' };
  }
  const start = startFn || formStart;
  const getRows = getRowsFn || formGetRows;
  const end = endFn || safeEnd;
  let form;
  try {
    form = await start(session.priority, 'EXEC', instance?.company, { messages });
    if (typeof form.setSearchFilter === 'function') {
      await form.setSearchFilter({
        or: 0,
        QueryValues: [
          { field: 'ENAME', fromval: ename, toval: '', op: '=', sort: 0, isdesc: 0 },
        ],
      });
    }
    const got = await getRows(form, 1);
    if (!got || got.count < 1 || !got.rows?.length) {
      return { ok: false, reason: 'proc_text_missing', name: ename };
    }
    let row = got.rows[0];
    for (const candidate of got.rows) {
      const rowName = String(candidate.ENAME || candidate.NAME || '').trim();
      if (rowName === ename) {
        row = candidate;
        break;
      }
    }
    const body = bodyFromExecRow(row);
    if (body == null) {
      return { ok: false, reason: 'proc_text_empty', name: ename, form: 'EXEC' };
    }
    return { ok: true, name: ename, body, form: 'EXEC', source: 'exec' };
  } catch (e) {
    return {
      ok: false,
      reason: 'form_failed',
      name: ename,
      message: String(e?.message || e),
    };
  } finally {
    if (form) await end(form);
  }
}

function stepPos(row) {
  const raw = pick(row, ['POS', 'Pos', 'pos', 'LINE', 'Line']);
  if (!raw) return null;
  const n = Number(raw);
  return Number.isFinite(n) ? n : raw;
}

function stepEname(row) {
  return pick(row, ['ENAME', 'Ename', 'NAME', 'Name']) || '';
}

/**
 * FR #696: read-only archaeology via EPROG subforms.
 * Returns concatenated body plus structured steps [{POS, ENAME, TEXT}].
 */
export async function readProcTextViaEprogSteps(session, instance, name, {
  messages = [],
  formStart: startFn,
  formGetRows: getRowsFn,
  formStartSubForm: startSubFn,
  safeEnd: endFn,
} = {}) {
  const ename = String(name || '').trim();
  if (!ename) return { ok: false, reason: 'name_required' };
  if (!session?.ok || !session.priority) {
    return { ok: false, reason: session?.reason || 'session_required' };
  }
  const start = startFn || formStart;
  const getRows = getRowsFn || formGetRows;
  const startSub = startSubFn || formStartSubForm;
  const end = endFn || safeEnd;
  let eprog;
  let prog;
  try {
    eprog = await start(session.priority, 'EPROG', instance?.company, { messages });
    if (typeof eprog.setSearchFilter === 'function') {
      await eprog.setSearchFilter({
        or: 0,
        QueryValues: [
          { field: 'ENAME', fromval: ename, toval: '', op: '=', sort: 0, isdesc: 0 },
        ],
      });
    }
    const got = await getRows(eprog, 1);
    if (!got || got.count < 1 || !got.rows?.length) {
      return { ok: false, reason: 'proc_text_missing', name: ename, form: 'EPROG' };
    }
    let row = got.rows[0];
    for (const candidate of got.rows) {
      const rowName = String(candidate.ENAME || candidate.NAME || '').trim();
      if (rowName === ename) {
        row = candidate;
        break;
      }
    }
    if (typeof eprog.setActiveRow === 'function' && row._idx != null) {
      await eprog.setActiveRow(Number(row._idx));
    }
    prog = await startSub(eprog, 'PROG', { messages });
    const stepsGot = await getRows(prog, 1);
    const stepRows = Array.isArray(stepsGot?.rows) ? stepsGot.rows : [];
    if (!stepRows.length) {
      return { ok: false, reason: 'proc_text_missing', name: ename, form: 'EPROG', detail: 'no_steps' };
    }
    const steps = [];
    for (const stepRow of stepRows) {
      if (typeof prog.setActiveRow === 'function' && stepRow._idx != null) {
        await prog.setActiveRow(Number(stepRow._idx));
      }
      let textForm;
      try {
        textForm = await startSub(prog, 'PROGTEXT', { messages });
        const textGot = await getRows(textForm, 1);
        const parts = [];
        for (const trow of textGot?.rows || []) {
          const piece = bodyFromExecRow(trow);
          if (piece != null) parts.push(piece);
        }
        const TEXT = parts.join('');
        steps.push({
          POS: stepPos(stepRow),
          ENAME: stepEname(stepRow) || null,
          TEXT,
        });
      } finally {
        if (textForm) await end(textForm);
      }
    }
    const bodyParts = steps.map((s) => s.TEXT).filter((t) => String(t || '').trim());
    if (!bodyParts.length) {
      return {
        ok: false,
        reason: 'proc_text_missing',
        name: ename,
        form: 'EPROG',
        steps,
        detail: 'steps_empty_text',
      };
    }
    return {
      ok: true,
      name: ename,
      body: bodyParts.join('\n'),
      steps,
      form: 'EPROG',
      source: 'eprog-steps',
    };
  } catch (e) {
    return {
      ok: false,
      reason: 'form_failed',
      name: ename,
      form: 'EPROG',
      message: String(e?.message || e),
    };
  } finally {
    if (prog) await end(prog);
    if (eprog) await end(eprog);
  }
}

/**
 * FR #409 / #696: prefer EXEC TEXT; fall back to EPROG steps when empty/missing.
 */
export async function readProcText(session, instance, name, opts = {}) {
  const viaExec = await readProcTextViaExecForm(session, instance, name, opts);
  if (viaExec.ok) return viaExec;
  // Empty EXEC TEXT or missing EXEC row — try EPROG archaeology (FR #696).
  if (
    viaExec.reason === 'proc_text_empty'
    || viaExec.reason === 'proc_text_missing'
  ) {
    const viaEprog = await readProcTextViaEprogSteps(session, instance, name, opts);
    if (viaEprog.ok) return viaEprog;
    return {
      ok: false,
      reason: 'proc_text_missing',
      name: String(name || '').trim(),
      execReason: viaExec.reason,
      eprogReason: viaEprog.reason,
      eprogDetail: viaEprog.detail,
    };
  }
  return viaExec;
}

export function loadProcTextFile(filePath) {
  const p = String(filePath || '').trim();
  if (!p) {
    return { ok: false, reason: 'request_invalid', detail: 'path_missing' };
  }
  let text;
  try {
    text = fs.readFileSync(p, 'utf8');
  } catch {
    return { ok: false, reason: 'request_invalid', detail: 'read_failed', path: p };
  }
  if (text.charCodeAt(0) === 0xfeff) text = text.slice(1);
  return { ok: true, body: text, path: p };
}

export async function applyProcTextToForm(form, body) {
  const text = String(body ?? '');
  if (typeof form.fieldUpdate !== 'function') {
    return { ok: false, reason: 'form_failed', detail: 'no_fieldUpdate' };
  }
  await form.fieldUpdate('TEXT', text, true);
  if (typeof formSave === 'function') {
    await formSave(form);
  }
  return { ok: true, set: 1 };
}

export async function writeProcTextViaExecForm(session, instance, name, body, { messages = [] } = {}) {
  const ename = String(name || '').trim();
  if (!ename) return { ok: false, reason: 'name_required' };
  let form;
  try {
    form = await formStart(session.priority, 'EXEC', instance.company, { messages });
    if (typeof form.setSearchFilter === 'function') {
      await form.setSearchFilter({
        or: 0,
        QueryValues: [
          { field: 'ENAME', fromval: ename, toval: '', op: '=', sort: 0, isdesc: 0 },
        ],
      });
    }
    const got = await formGetRows(form, 1);
    if (!got || got.count < 1) {
      return { ok: false, reason: 'proc_text_missing', name: ename };
    }
    if (typeof form.setActiveRow === 'function') {
      await form.setActiveRow(Number(got.rows[0]._idx));
    }
    const applied = await applyProcTextToForm(form, body);
    return { ...applied, name: ename, form: 'EXEC' };
  } finally {
    if (form) await safeEnd(form);
  }
}
