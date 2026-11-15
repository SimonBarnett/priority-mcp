/**
 * FR #607: resolve the current PriorityAgent.exe download for mcp.ntsa.uk home.
 *
 * Latest-selection policy (document in PR):
 * 1. Walk GitHub releases for SimonBarnett/priority-mcp newest-first (API default),
 *    including prereleases (formal "Latest" UAT tags often have no exe assets).
 * 2. Pick the first release that has an asset exactly named `PriorityAgent.exe`.
 * 3. Optional companion `PriorityAgent.exe.sha256` when present on the same release.
 * 4. Version label = release tag with a leading `launcher-v` / `v` stripped when present
 *    (UI never says "launcher").
 */

const DEFAULT_API =
  'https://api.github.com/repos/SimonBarnett/priority-mcp/releases?per_page=20';
const ASSET_EXE = 'PriorityAgent.exe';
const ASSET_SHA = 'PriorityAgent.exe.sha256';

/**
 * @param {unknown} releases
 * @returns {{
 *   tag: string,
 *   versionLabel: string,
 *   exeUrl: string,
 *   sha256Url: string | null,
 * } | null}
 */
export function pickPriorityAgentDownload(releases) {
  if (!Array.isArray(releases)) return null;
  for (const rel of releases) {
    if (!rel || typeof rel !== 'object') continue;
    const assets = Array.isArray(rel.assets) ? rel.assets : [];
    const exe = assets.find((a) => a && a.name === ASSET_EXE && a.browser_download_url);
    if (!exe) continue;
    const sha = assets.find((a) => a && a.name === ASSET_SHA && a.browser_download_url);
    const tag = String(rel.tag_name || '').trim();
    const versionLabel = tag.replace(/^launcher-v/i, '').replace(/^v/i, '') || tag || 'unknown';
    return {
      tag,
      versionLabel,
      exeUrl: String(exe.browser_download_url),
      sha256Url: sha ? String(sha.browser_download_url) : null,
    };
  }
  return null;
}

/**
 * @param {{ fetchImpl?: typeof fetch, apiUrl?: string } } [opts]
 */
export async function resolvePriorityAgentDownload(opts = {}) {
  const fetchImpl = opts.fetchImpl || fetch;
  const apiUrl = opts.apiUrl || DEFAULT_API;
  const res = await fetchImpl(apiUrl, {
    headers: {
      Accept: 'application/vnd.github+json',
      'User-Agent': 'mcp-ntsa-uk-priority-agent-download',
    },
  });
  if (!res.ok) {
    throw new Error(`GitHub releases HTTP ${res.status}`);
  }
  const body = await res.json();
  return pickPriorityAgentDownload(body);
}
