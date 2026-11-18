/**
 * SaaS prep success gate (FR #339).
 * On-prem uses SQL EXECPREPLOCK. SaaS often has no local SQL/prep disk —
 * prefer OData re-read when configured; otherwise mark gate unavailable
 * and never claim prepared=true.
 */

export function gateUnavailable(reason = 'no_sql_no_odata', extra = {}) {
  return {
    ok: false,
    status: 'unavailable',
    preparedClaim: false,
    reason,
    ...extra,
  };
}

export function parseODataPrepRow(row) {
  if (!row || typeof row !== 'object') {
    return { upd: null, lastPrepDate: null, ename: null };
  }
  const ename = row.ENAME ?? row.ename ?? row.Name ?? null;
  const updRaw = row.UPD ?? row.Upd ?? row.upd ?? null;
  const upd = updRaw == null ? null : String(updRaw).trim();
  const last =
    row.LASTPREPDATE ??
    row.LastPrepDate ??
    row.lastPrepDate ??
    row.LASTPREP ??
    null;
  return {
    ename: ename == null ? null : String(ename).trim(),
    upd,
    lastPrepDate: last == null || last === '' ? null : String(last).trim(),
    source: 'odata',
    rawKeys: Object.keys(row),
  };
}

/**
 * Compare before/after snapshots. Success = UPD=N and LASTPREPDATE advanced
 * when a before lastPrepDate is known. If before is missing lastprep, require
 * UPD=N only and note weak_gate.
 */
export function evaluatePrepGate(before, after) {
  if (!after) {
    return { ok: false, status: 'no_after', preparedClaim: false };
  }
  const upd = String(after.upd || '').trim().toUpperCase();
  if (upd && upd !== 'N') {
    return {
      ok: false,
      status: 'still_unprepared',
      preparedClaim: false,
      upd,
      lastPrepDate: after.lastPrepDate,
      source: after.source,
    };
  }
  const beforeLast = before?.lastPrepDate != null ? String(before.lastPrepDate) : null;
  const afterLast = after.lastPrepDate != null ? String(after.lastPrepDate) : null;
  if (beforeLast != null && afterLast != null) {
    // bigint-ish compare as BigInt when possible
    let advanced = false;
    try {
      advanced = BigInt(afterLast) > BigInt(beforeLast);
    } catch {
      advanced = afterLast !== beforeLast && afterLast > beforeLast;
    }
    if (!advanced) {
      return {
        ok: false,
        status: 'lastprep_unchanged',
        preparedClaim: false,
        before: beforeLast,
        after: afterLast,
        upd,
        source: after.source,
      };
    }
    return {
      ok: true,
      status: 'prepared',
      preparedClaim: true,
      upd: upd || 'N',
      before: beforeLast,
      after: afterLast,
      source: after.source,
    };
  }
  if (upd === 'N') {
    return {
      ok: true,
      status: 'prepared_weak',
      preparedClaim: true,
      weakGate: true,
      message: 'UPD=N but no before/after LASTPREPDATE pair',
      upd,
      lastPrepDate: afterLast,
      source: after.source,
    };
  }
  return {
    ok: false,
    status: 'insufficient_gate_fields',
    preparedClaim: false,
    after,
  };
}

function odataPrepEntityList(instance) {
  const primary = String(instance?.odataPrepPath || instance?.odataPrepEntity || 'FORMS').replace(/^\/+/, '');
  const raw = instance?.odataPrepFallbacks;
  const extras = Array.isArray(raw)
    ? raw.map((e) => String(e || '').replace(/^\/+/, '').trim()).filter(Boolean)
    : String(raw || '')
        .split(',')
        .map((e) => e.replace(/^\/+/, '').trim())
        .filter(Boolean);
  const out = [];
  for (const e of [primary, ...extras]) {
    if (e && !out.includes(e)) out.push(e);
  }
  return out;
}

function odataHttpGateFailure(status, { entity, entitiesTried, bodyPreview, ename }) {
  const code = Number(status) || 0;
  const hint404 =
    'Dictionary/EXECPREPLOCK-style entity may be missing on this company odataBase. ' +
    'Set instance.odataPrepPath to an entity that appears in the service document, and/or ' +
    'odataPrepFallbacks (array or comma list). walkOk alone is never preparedClaim — do not ' +
    're-prepare forever. For procedure step SQLI archaeology when top-level PROG stays ' +
    'unprepared, use EPROG -> PROG -> PROGTEXT (docs/skill-sources/programming/PROC_TEXT_VIA_EPROG_SUBFORM.md).';
  return {
    ok: false,
    status: 'odata_http_' + code,
    preparedClaim: false,
    entity,
    entitiesTried: entitiesTried || [entity],
    ename,
    bodyPreview: bodyPreview == null ? undefined : String(bodyPreview).slice(0, 300),
    message:
      'OData prep gate HTTP ' +
      code +
      ' for entity ' +
      entity +
      '; walkOk is not proof of preparedClaim.',
    hint: code === 404 || code === 405 ? hint404 : 'Check odataBase auth and entity path; walkOk is not preparedClaim.',
  };
}

/**
 * Fetch one prep lock row via OData (allowlist odataBase + PAT/Bearer).
 * Entity path defaults to instance.odataPrepPath or 'FORMS'.
 * FR #698: on HTTP 404/405, try instance.odataPrepFallbacks; always return a
 * clear message that walkOk is not preparedClaim.
 */
export async function fetchODataPrepSnapshot(instance, ename, { accessToken = '', fetchImpl = fetch } = {}) {
  const base = String(instance?.odataBase || instance?.odataUrl || '').replace(/\/+$/, '');
  if (!base) {
    return gateUnavailable('no_odata_base', { hint: 'Set instance.odataBase for SaaS EXECPREPLOCK-style re-read' });
  }
  const entities = odataPrepEntityList(instance);
  const filter = encodeURIComponent(`ENAME eq '${String(ename).replace(/'/g, "''")}'`);
  const headers = { Accept: 'application/json' };
  const pat = String(process.env.PRIORITY_SAAS_ODATA_PAT || process.env.PRIORITY_ODATA_PAT || '').trim();
  // PAT only from env — never from allowlist JSON.
  const bearer = String(accessToken || process.env.PRIORITY_SAAS_TOKEN || '').trim();
  if (pat) {
    // Priority REST PAT: username = token value, password = literal "PAT"
    // (not "token:<pat>", which 401s on Cloud OData).
    headers.Authorization = 'Basic ' + Buffer.from(`${pat}:PAT`).toString('base64');
  } else if (bearer) {
    headers.Authorization = 'Bearer ' + bearer;
  } else {
    return gateUnavailable('no_odata_auth', { hint: 'Set PRIORITY_SAAS_ODATA_PAT or pass accessToken' });
  }

  let lastHttpFail = null;
  for (const entity of entities) {
    const url = `${base}/${entity}?$filter=${filter}&$top=1`;
    let res;
    try {
      res = await fetchImpl(url, { headers });
    } catch (e) {
      return {
        ok: false,
        status: 'odata_fetch_failed',
        preparedClaim: false,
        message: String(e.message || e),
        url: base + '/' + entity,
        entity,
        entitiesTried: entities,
      };
    }
    const text = await res.text();
    if (!res.ok) {
      lastHttpFail = odataHttpGateFailure(res.status, {
        entity,
        entitiesTried: entities,
        bodyPreview: text,
        ename,
      });
      // FR #698: missing dictionary entity — try configured fallbacks before giving up.
      if ((res.status === 404 || res.status === 405) && entity !== entities[entities.length - 1]) {
        continue;
      }
      return lastHttpFail;
    }
    let json;
    try {
      json = JSON.parse(text);
    } catch (e) {
      return {
        ok: false,
        status: 'odata_invalid_json',
        preparedClaim: false,
        message: String(e.message || e),
        entity,
        entitiesTried: entities,
      };
    }
    const rows = json.value || json.d?.results || (Array.isArray(json) ? json : [json]);
    const row = Array.isArray(rows) ? rows[0] : rows;
    if (!row) {
      return {
        ok: false,
        status: 'odata_row_missing',
        preparedClaim: false,
        ename,
        entity,
        entitiesTried: entities,
        message: 'OData returned no row for ' + ename + ' on entity ' + entity + '; walkOk is not preparedClaim.',
        hint: 'Confirm ENAME and odataPrepPath; do not treat a successful prepare walk as preparedClaim.',
      };
    }
    const parsed = parseODataPrepRow(row);
    return { ok: true, status: 'snapshot', ...parsed, entity, entitiesTried: entities };
  }
  return (
    lastHttpFail ||
    gateUnavailable('no_odata_entity', {
      preparedClaim: false,
      entitiesTried: entities,
      message: 'No odataPrepPath/fallbacks left to try; walkOk is not preparedClaim.',
    })
  );
}
