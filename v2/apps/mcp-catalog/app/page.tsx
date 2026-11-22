import { headers } from 'next/headers';
import { listSkills } from '../lib/catalog';
import { catalogPublicOrigin } from '../lib/public-origin';
import { resolvePriorityAgentDownload } from '../lib/priority-agent-download';
import {
  TRAINING_EXPLAINER_BODY,
  TRAINING_EXPLAINER_POINTS,
  TRAINING_EXPLAINER_TITLE,
} from '../lib/training-explainer';

export const runtime = 'nodejs';
export const dynamic = 'force-dynamic';

export default async function Home() {
  const requestHeaders = headers();
  const origin = catalogPublicOrigin({
    host: requestHeaders.get('host'),
    forwardedHost: requestHeaders.get('x-forwarded-host'),
    proto: requestHeaders.get('x-forwarded-proto'),
  });
  const skills = listSkills();
  let agentDownload: Awaited<ReturnType<typeof resolvePriorityAgentDownload>> = null;
  try {
    agentDownload = await resolvePriorityAgentDownload();
  } catch {
    agentDownload = null;
  }
  return (
    <>
      <p className="meta">
        <a href="https://bob.ntsa.uk">bob.ntsa.uk</a>
        {' · '}
        <span className="badge">mcp-home</span>
      </p>
      <h1>Priority MCP</h1>
      <p>
        Priority MCP for Priority agent skills. Agents <strong>grab</strong> a skill here, then run
        it against instances the <strong>user</strong> allowlisted. This host does not compile
        forms and cannot see on-prem SQL.
      </p>

      <h2>MCP</h2>
      <p>Connect an MCP client to:</p>
      <pre>{`${origin}/mcp`}</pre>
      <p>
        <code>{`grok mcp add --transport http priority-catalog ${origin}/mcp`}</code>
      </p>
      <p>
        Cursor / Claude: HTTP MCP URL <code>{`${origin}/mcp`}</code> (one JSON-RPC object per POST,
        protocol 2024-11-05). This deployment does not serve stdio or a long-lived event stream.
      </p>
      {agentDownload ? (
        <p>
          <a href={agentDownload.exeUrl}>
            Download Priority Agent
            {agentDownload.versionLabel ? ` v${agentDownload.versionLabel}` : ''}
          </a>
          {agentDownload.sha256Url ? (
            <>
              {' · '}
              <a href={agentDownload.sha256Url}>sha256</a>
            </>
          ) : null}
        </p>
      ) : (
        <p>Priority Agent download unavailable.</p>
      )}

      <h2 id="training-explainer-heading">{TRAINING_EXPLAINER_TITLE}</h2>
      <p>{TRAINING_EXPLAINER_BODY}</p>
      <ul>
        {TRAINING_EXPLAINER_POINTS.map((point) => (
          <li key={point}>{point}</li>
        ))}
      </ul>

      <h2>Skills ({skills.length})</h2>
      <p>
        Add another skill later by dropping <code>catalog/&lt;name&gt;/meta.json</code> and{' '}
        <code>SKILL.md</code> — no MCP code change.
      </p>
      <ul className="skill-list">
        {skills.map((s) => (
          <li key={s.id}>
            <strong>
              {s.meta.title} (<code>{s.id}</code>) v{s.meta.version}
            </strong>
            {' — '}
            {s.meta.description}
          </li>
        ))}
      </ul>

      <h2>Execute plane (local plugins)</h2>
      <p>
        User file <code>%USERPROFILE%\.priority-formprep\instances.json</code> (schema from{' '}
        <code>get_instance_schema</code>). CredMan for passwords. Local Grok plugins:{' '}
        <code>priority-formprep</code> (<code>prepare_form</code>), <code>priority-shell-compile</code>{' '}
        (<code>compile_shell</code>), <code>priority-shell-install</code> (<code>install_shell</code>
        ), <code>priority-odata-dev</code> (<code>odata_get</code> / <code>odata_query</code> /{' '}
        <code>odata_dump_procedure</code> / <code>formlimited_audit</code>). Compile, install, and
        OData are separate tools. This host does not run them.
      </p>

      <h2>Hard rules</h2>
      <ul>
        <li>Never SQL-flip UPD=N.</li>
        <li>Success = UPD=N and LASTPREPDATE advanced.</li>
        <li>Never invent a WCF URL; only user-listed instance ids.</li>
        <li>
          RESTFLAG-only FORMLIMITED can hide UI sibling tabs. See <code>priority-odata-dev</code>.
        </li>
      </ul>

      <h2>Also</h2>
      <ul>
        <li>
          <a href="https://github.com/SimonBarnett/priority-mcp">GitHub: priority-mcp</a>
        </li>
        <li>
          <a href="https://bob.ntsa.uk">bob.ntsa.uk</a>
        </li>
        <li>
          <a href="https://plan.ntsa.uk">Sibling: Plan catalog plan.ntsa.uk</a>
        </li>
      </ul>
    </>
  );
}
