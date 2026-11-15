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
  const hostLabel = new URL(origin).host;
  const skills = listSkills();
  let agentDownload: Awaited<ReturnType<typeof resolvePriorityAgentDownload>> = null;
  try {
    agentDownload = await resolvePriorityAgentDownload();
  } catch {
    agentDownload = null;
  }
  return (
    <div className="shell">
      <header className="site-header">
        <a className="brand" href="/">
          <span className="brand-mark" aria-hidden="true">
            M
          </span>
          <span className="brand-text">
            <strong>{hostLabel}</strong>
            <span>Priority skill catalog</span>
          </span>
        </a>
        <nav className="nav" aria-label="Primary">
          <a href="https://bob.ntsa.uk/">bob.ntsa.uk</a>
          <a href="/" aria-current="page">
            Catalog
          </a>
          <a href="https://github.com/SimonBarnett/priority-mcp">GitHub</a>
        </nav>
      </header>

      <section className="hero">
        <div className="hero-copy">
          <div className="eyebrow">Catalog MCP</div>
          <h1>Skills the Priority agent knows how to grab.</h1>
          <p>
            Catalog MCP for Priority agent skills. Agents <strong>grab</strong> a skill here, then run
            it against instances the <strong>user</strong> allowlisted. This host does not compile
            forms and cannot see on-prem SQL.
          </p>
          <div className="actions">
            {agentDownload ? (
              <a className="btn btn-gold" href={agentDownload.exeUrl}>
                Download <strong>Priority Agent</strong>
                {agentDownload.versionLabel ? ` v${agentDownload.versionLabel}` : ''}
              </a>
            ) : (
              <span className="btn btn-gold" aria-disabled="true">
                <strong>Priority Agent</strong> download unavailable
              </span>
            )}
            {agentDownload?.sha256Url ? (
              <a className="btn btn-ghost" href={agentDownload.sha256Url}>
                sha256
              </a>
            ) : null}
            <a className="btn btn-primary" href={`${origin}/mcp`}>
              MCP endpoint
            </a>
            <a className="btn btn-ghost" href="https://bob.ntsa.uk/">
              Back to bob.ntsa.uk
            </a>
            <a className="btn btn-ghost" href="https://github.com/SimonBarnett/priority-mcp">
              GitHub
            </a>
          </div>
        </div>
        <aside className="hero-panel">
          <h2>Add the MCP</h2>
          <div className="stat">
            <span>Transport</span>
            <code>HTTP</code>
          </div>
          <div className="stat">
            <span>URL</span>
            <code>{origin}/mcp</code>
          </div>
          <pre>{`grok mcp add --transport http priority-catalog ${origin}/mcp`}</pre>
          <p style={{ margin: '0.75rem 0 0', fontSize: '0.88rem', opacity: 0.85 }}>
            Cursor / Claude: HTTP MCP URL <code>{origin}/mcp</code> (one JSON-RPC object per POST,
            protocol 2024-11-05). This deployment does not serve stdio or a long-lived event stream.
          </p>
        </aside>
      </section>

      <section className="panel" aria-labelledby="training-explainer-heading">
        <h2 id="training-explainer-heading">{TRAINING_EXPLAINER_TITLE}</h2>
        <p>{TRAINING_EXPLAINER_BODY}</p>
        <ul>
          {TRAINING_EXPLAINER_POINTS.map((point) => (
            <li key={point}>{point}</li>
          ))}
        </ul>
      </section>

      <div className="section-title">
        <h2>Skills</h2>
        <span>{skills.length} in catalog</span>
      </div>
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
            <div>{s.meta.description}</div>
          </li>
        ))}
      </ul>

      <div className="panel">
        <h2>Execute plane (local plugins)</h2>
        <p>
          User file <code>%USERPROFILE%\.priority-formprep\instances.json</code> (schema from{' '}
          <code>get_instance_schema</code>). CredMan for passwords. Local Grok plugins:{' '}
          <code>priority-formprep</code> (<code>prepare_form</code>),{' '}
          <code>priority-shell-compile</code> (<code>compile_shell</code>),{' '}
          <code>priority-shell-install</code> (<code>install_shell</code>),{' '}
          <code>priority-odata-dev</code> (<code>odata_get</code> / <code>odata_query</code> /{' '}
          <code>odata_dump_procedure</code> / <code>formlimited_audit</code>). Compile, install, and
          OData are separate tools. This host does not run them.
        </p>
      </div>

      <div className="panel">
        <h2>Hard rules</h2>
        <ul>
          <li>Never SQL-flip UPD=N.</li>
          <li>Success = UPD=N and LASTPREPDATE advanced.</li>
          <li>Never invent a WCF URL; only user-listed instance ids.</li>
          <li>
            RESTFLAG-only FORMLIMITED can hide UI sibling tabs. See <code>priority-odata-dev</code>.
          </li>
        </ul>
      </div>

      <footer className="site-footer">
        <span>FR #606 · shared charcoal / paper / gold chrome with bob.ntsa.uk</span>
        <span>
          <a href="https://bob.ntsa.uk/">bob.ntsa.uk</a>
          {' · '}
          <a href="https://github.com/SimonBarnett/priority-mcp">GitHub</a>
        </span>
      </footer>
    </div>
  );
}
